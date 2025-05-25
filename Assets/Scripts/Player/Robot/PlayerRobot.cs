    using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Chunks;
using Chunks.Systems;
using Dimensions;
using Item.Slot;
using Items;
using Items.Tags;
using Player.Controls;
using Player.Inventory;
using Player.Movement;
using Player.Movement.Standard;
using Player.Robot;
using Player.Tool;
using Player.UI;
using PlayerModule;
using PlayerModule.KeyPress;
using Robot;
using Robot.Tool;
using Robot.Tool.Instances.Gun;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.Instances.RocketBoots;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using RobotModule;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Place;
using Tiles;
using UI;
using UI.Statistics;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player {
    public enum CollisionState
    {
        OnWallLeft,
        OnWallRight,
        OnGround,
        OnSlope,
        HeadContact,
        OnPlatform,
        OnLeftSlopePlatform,
        OnRightSlopePlatform,
        InFluid,
    }

    public enum PlayerMovementState
    {
        Standard,
        Climbing,
        Flying
    }
    
    public class PlayerRobot : MonoBehaviour
    {
        [SerializeField] private PlayerRobotUI mPlayerRobotUI;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private Collider2D leftSlopePlatformCollider;
        [SerializeField] private Collider2D rightSlopePlatformCollider;
        [SerializeField] private PlayerDeathScreenUI deathScreenUIPrefab;

        [SerializeField] internal DirectionalMovementStats MovementStats;
        [SerializeField] internal JumpMovementStats JumpStats;
        [SerializeField] private RobotUpgradeAssetReferences RobotUpgradeAssets;
        
        private PlayerMovementInput playerMovement;
        private PlayerMovementState movementState;
        private bool climbing;
        
        private HashSet<CollisionState> collisionStates = new HashSet<CollisionState>();
        
        
        // Robot Data
        public ItemSlot robotItemSlot;
        private RobotObject currentRobot;
        public RobotObject CurrentRobot => currentRobot;
        private RobotItemData robotData;
        public RobotItemData RobotData => robotData;
        public List<IRobotToolInstance> RobotTools;
        private Dictionary<RobotToolType, RobotToolObject> currentRobotToolObjects;
        public List<RobotToolType> ToolTypes => robotData.ToolData.Types;
        public RobotUpgradeLoadOut RobotUpgradeLoadOut;
        
        // State Information
        private float fallTime;
        private bool immuneToNextFall = false;
        public int InvincibilityFrames { get; private set; }
        public TileMovementType CurrentTileMovementType { get; private set; }
        public bool Dead => robotData.Health <= 0;

        private const float TERMINAL_VELOCITY = 20f;
        public int LiveYUpdates { get; private set; }
        public int IgnorePlatformFrames { get; private set; }
        public int IgnoreSlopePlatformFrames { get; private set; }
        public int SlipperyFrames { get; private set; }
        private float moveDirTime;
        public int CoyoteFrames{ get; private set; }
        public int HighDragFrames { get; private set; }
        public bool IsUsingTool { get; private set; }
        private bool recalling;
        
        private bool freezeY;
        
        // Upgrades
        
        // References
        private PlayerScript playerScript;
        public FluidCollisionInformation fluidCollisionInformation {get; private set;}
        public RobotArmController gunController;
        private CanvasController canvasController;
        public bool BlockMovement => canvasController.BlockKeyInput;
        public DevMode DevMode { get; private set; }
        public PlayerAnimationController AnimationController { get; private set; }
        public PlayerParticles PlayerParticles { get; private set; }

        public PlayerDamage PlayerDamage { get; private set; }
        
        // Default values
        public float DefaultLinearDrag  { get; private set; }
        public float DefaultGravityScale { get; private set; }
        private float defaultBoxColliderWidth;
        private float defaultBoxColliderEdge;
        
        // Layers
        private int blockLayer;
        private int baseCollidableLayer;
        
        
        private PlayerTeleportEvent playerTeleportEvent;
        
        // Movement
        private StandardPlayerMovement standardPlayerMovement;
        
        public const float BASE_MOVE_SPEED = 5f;
        
        private void Awake()
        {
            playerMovement = new PlayerMovementInput();
            playerMovement.PlayerMovement.Enable();
        }

        
        private void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            playerScript = GetComponent<PlayerScript>();
            blockLayer = 1 << LayerMask.NameToLayer("Block");
            baseCollidableLayer = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform"));
            DefaultGravityScale = rb.gravityScale;
            DefaultLinearDrag = rb.drag;
            
            gunController.Initialize(this);
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            defaultBoxColliderWidth = boxCollider.size.x;
            defaultBoxColliderEdge = boxCollider.edgeRadius;
            canvasController = CanvasController.Instance;

            PlayerDamage = new PlayerDamage(this);
            DevMode = GetComponent<DevMode>();
            fluidCollisionInformation = new();
            AnimationController = new PlayerAnimationController(this, GetComponent<Animator>());

            standardPlayerMovement = new StandardPlayerMovement(this);

            StartCoroutine(LoadAsyncAssets());
        }

        private IEnumerator LoadAsyncAssets()
        {
            GameObject container = new GameObject("ParticleContainer");
            container.transform.SetParent(transform,false);
            container.transform.localPosition = Vector3.zero;
            IEnumerator LoadAsset(AssetReference assetReference, Action<GameObject> onLoad)
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
                yield return handle;
                var instantiated = GameObject.Instantiate(handle.Result, container.transform, false);
                instantiated.transform.localPosition = new Vector3(0,0,5);
                onLoad(instantiated);
                Addressables.Release(handle);
            }
            ParticleSystem bonusJumpParticles = null;
            ParticleSystem teleportParticles = null;
            ParticleSystem nanoBotParticles = null;
            var a = StartCoroutine(LoadAsset(RobotUpgradeAssets.BonusJumpParticles, (GameObject result) =>
            {
                bonusJumpParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
            var b = StartCoroutine(LoadAsset(RobotUpgradeAssets.TeleportParticles, (GameObject result) =>
            {
                teleportParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
            var c = StartCoroutine(LoadAsset(RobotUpgradeAssets.NanoBotParticles, (GameObject result) =>
            {
                nanoBotParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
            yield return a;
            yield return b;
            yield return c;
            PlayerParticles = new PlayerParticles(this,bonusJumpParticles,teleportParticles,nanoBotParticles);
        }

        public void Update()
        {
            if (robotData == null) return; // Don't like this
            mPlayerRobotUI.Display(this);
            if (robotData.Health <= 0) return;
            MoveUpdate();
            FluidDamageUpdate();
            PlayerDamage.IterateDamageTime(Time.deltaTime);
        }

        public void SetIsUsingTool(bool value)
        {
            if (IsUsingTool == value) return;
            IsUsingTool = value;
            
            AnimationController.ToggleBool(PlayerAnimationState.Action, IsUsingTool);
            gunController.gameObject.GetComponent<SpriteRenderer>().enabled = value;
            gunController.gameObject.GetComponent<Animator>().enabled = value;
            
            if (!value)
            {
                gunController.OnNoClick();
            }
            
            if (IsGrounded() && Mathf.Abs(rb.velocity.x) > 0.05f)
            {
                float time = AnimationController.GetCurrentAnimationTime();
                AnimationController.PlayAnimation(PlayerAnimation.Walk,IsUsingTool,time);
            }
        }
        

        private void FluidDamageUpdate()
        {
            if (!fluidCollisionInformation.Colliding || fluidCollisionInformation.Damage <= 0.05f) return;
            fluidCollisionInformation.DamageCounter += Time.deltaTime;
            if (fluidCollisionInformation.DamageCounter < 1f) return;
            PlayerDamage.Damage(fluidCollisionInformation.Damage);
            fluidCollisionInformation.DamageCounter = 0;
        }

        
        public void AddCollisionState(CollisionState state)
        {
            if (!collisionStates.Add(state)) return;
            if (state is CollisionState.OnGround or CollisionState.OnSlope or CollisionState.OnPlatform or CollisionState.OnLeftSlopePlatform or CollisionState.OnRightSlopePlatform)
            {
                standardPlayerMovement.OnGrounded();
            }

            if (state is CollisionState.OnGround or CollisionState.OnSlope)
            {
                // Added this to prevent this player getting stuck, if they still get stuck might want to increase live updates
                LiveYUpdates = 2;
                var vector2 = rb.velocity;
                vector2.y = 0.005f;
                rb.velocity = vector2;
            }

            if (state is CollisionState.OnGround)
            {
                BoxCollider2D boxCollider2d = GetComponent<BoxCollider2D>();
                boxCollider2d.edgeRadius = defaultBoxColliderEdge;
                var size = boxCollider2d.size;
                size.x = defaultBoxColliderWidth;
                boxCollider2d.size = size;
            }

            if (state is CollisionState.OnWallLeft or CollisionState.OnWallRight)
            {
                SlipperyFrames = 0;
            }

            if (state is CollisionState.OnSlope)
            {
                float bonusSpeed = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);
                rb.drag = MovementStats.defaultDragOnSlope + bonusSpeed*MovementStats.speedUpgradeDragIncrease;
                HighDragFrames = int.MaxValue;
            }

            if (state is CollisionState.InFluid)
            {
                fallTime = 0;
                Vector3 position = transform.position;
                position.z = 2;
                transform.position = position;
            }
        }

        
        public void FaceMousePosition(Vector2 mousePosition)
        {
            Vector2 dif = (Vector2)transform.position - mousePosition;
            spriteRenderer.flipX = dif.x > 0;
        }
        

        public void AddFluidCollisionData(FluidTileItem fluidTileItem)
        {
            if (!fluidTileItem) return;
            
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                float slowFactor = fluidTileItem.fluidOptions.SpeedSlowFactor;
                var vector2 = rb.velocity;
                if (slowFactor > 0.8f)
                {
                    vector2.y *= slowFactor;
                }
                else
                {
                    vector2.y *= 0.1f * slowFactor;
                }
                
                rb.velocity = vector2;
            }
            
            if (fluidTileItem.fluidOptions.DamagePerSecond > 0)
            {
                // Deal half damage on first collision
                PlayerDamage.Damage(fluidTileItem.fluidOptions.DamagePerSecond/2f);
            }
            fluidCollisionInformation.SetFluidItem(fluidTileItem);
            
        }

        public void RemoveCollisionState(CollisionState state)
        {
            if (!collisionStates.Remove(state)) return;
            
            if (state == CollisionState.InFluid)
            {
                Vector3 position = transform.position;
                position.z = -5;
                transform.position = position;
                fluidCollisionInformation.Clear();
            }

            if (state is CollisionState.OnGround)
            {
                const float EDGE_RADIUS = 0.005f;
                
                BoxCollider2D boxCollider2d = GetComponent<BoxCollider2D>();
                boxCollider2d.edgeRadius = EDGE_RADIUS;
                var size = boxCollider2d.size;
                size.x = defaultBoxColliderWidth + defaultBoxColliderEdge - EDGE_RADIUS/2f;
                boxCollider2d.size = size;
            }
            if (state is CollisionState.OnSlope)
            {
                //rb.drag = defaultLinearDrag;
                HighDragFrames = 3;
            }
        }

        public bool IsMoving()
        {
            return rb.velocity.magnitude > 0.1f;
        }

        public bool CollisionStateActive(CollisionState state)
        {
            return collisionStates.Contains(state);
        }

        public bool InFluid()
        {
            return collisionStates.Contains(CollisionState.InFluid);
        }

        private void MoveUpdate()
        {
            if (!canvasController.BlockKeyInput)
            {
                if (RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Teleport) > 0)
                {
                    TeleportUpdate();
                }
                
                if (!recalling && ControlUtils.GetControlKeyDown(PlayerControl.Recall))
                {
                    StartCoroutine(RecallCoroutine());
                }
            }
            if (DevMode.Instance.flight)
            {
                AnimationController.ToggleBool(PlayerAnimationState.Walk,false);
                if (canvasController.BlockKeyInput) return;
                CreativeFlightMovementUpdate(transform);
                return;
            }
            
            float flight = RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Flight);


            if (flight > 0 && TryConsumeEnergy(SelfRobotUpgradeInfo.FLIGHT_COST, 0))
            {
                AnimationController.ToggleBool(PlayerAnimationState.Walk,false);
                FlightMoveUpdate();
                return;
            }
            
            if (climbing)
            {
                AnimationController.SetAnimationSpeed(1);
                AnimationController.ToggleBool(PlayerAnimationState.Walk,false);
                AnimationController.PlayAnimation(PlayerAnimation.Air,IsUsingTool);
                bool exitKeyCode = ControlUtils.GetControlKeyDown(PlayerControl.MoveLeft) || ControlUtils.GetControlKeyDown(PlayerControl.MoveRight) || ControlUtils.GetControlKeyDown(PlayerControl.Jump);
                if (!exitKeyCode) return;
                
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                climbing = false;
                rb.gravityScale = DefaultGravityScale;
                return;
            }
            standardPlayerMovement.MovementUpdate();
        }
        

        private IEnumerator RecallCoroutine()
        {
            recalling = true;
            // TODO some sound effect and animation
            const float RECALL_DELAY = 0.2f;
            yield return new WaitForSeconds(RECALL_DELAY);
            DimensionManager.Instance.SetPlayerSystem(playerScript,0,Vector2Int.zero);
            recalling = false;
        }

        private void TeleportUpdate()
        {
            if (playerTeleportEvent != null)
            {
                playerTeleportEvent.IterateTime(Time.deltaTime);
                if (playerTeleportEvent.Expired()) playerTeleportEvent = null;
            }

            if (!ControlUtils.GetControlKeyDown(PlayerControl.Teleport) || playerTeleportEvent != null) return;
            if (!TryConsumeEnergy(SelfRobotUpgradeInfo.TELEPORT_COST, 0)) return;
            Camera mainCamera = Camera.main;
            if (!mainCamera) return;
            Vector2 teleportPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            playerTeleportEvent = new PlayerTeleportEvent(transform, teleportPosition, spriteRenderer.sprite.bounds);
            bool teleported = playerTeleportEvent.TryTeleport();
            if (!teleported) return;
            playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Teleportations]++;
            PlayerParticles.PlayParticle(PlayerParticle.Teleportation);
            fallTime = 0;

            
            IEnumerator DelayForceUpdate()
            {
                // Sometimes partitions are not loaded on teleport
                yield return new WaitForSeconds(0.5f);
                mainCamera.GetComponent<CameraBounds>().ForceUpdatePartition();
                yield return new WaitForSeconds(0.5f);
                mainCamera.GetComponent<CameraBounds>().ForceUpdatePartition();
            }

            StartCoroutine(DelayForceUpdate());
        }

        public void OnJump()
        {
            CoyoteFrames = 0;
            LiveYUpdates = 3;
            rb.drag = DefaultLinearDrag;
            fallTime = 0;
            SlipperyFrames /= 2;
            IgnoreSlopePlatformFrames = 2;
        }

        public bool IsGrounded()
        {
            return CollisionStateActive(CollisionState.OnPlatform) || CollisionStateActive(CollisionState.OnGround) || CollisionStateActive(CollisionState.OnSlope) || IsOnSlopedPlatform();
        }

        public bool IsOnSlopedPlatform()
        {
            return CollisionStateActive(CollisionState.OnLeftSlopePlatform) || CollisionStateActive(CollisionState.OnRightSlopePlatform);
        }
        
        private void FlightMoveUpdate()
        {
            rb.gravityScale = 0;
            bool blockInput = canvasController.BlockKeyInput;
            if (blockInput)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            bool leftInput = ControlUtils.GetControlKey(PlayerControl.MoveLeft);
            bool rightInput = ControlUtils.GetControlKey(PlayerControl.MoveRight);
            bool upInput = ControlUtils.GetControlKey(PlayerControl.MoveUp);
            bool downInput = ControlUtils.GetControlKey(PlayerControl.MoveDown);
             
            Vector2 velocity = rb.velocity;
            
            float speed = BASE_MOVE_SPEED; 
            float speedUpgrades = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);
            if (TryConsumeEnergy((ulong)(speedUpgrades * 16 * (SelfRobotUpgradeInfo.SPEED_INCREASE_COST_PER_SECOND*Time.deltaTime)), 0.1f))
            {
                speed += speedUpgrades;
            }
            
            bool horizontalMovement = leftInput != rightInput;
            bool verticalMovement = upInput != downInput;
            
            if (!horizontalMovement)
            {
                velocity.x = 0;
            }
            else
            {
                if (leftInput)
                {
                    velocity.x = -speed;
                    spriteRenderer.flipX = true;
                }
                else
                {
                    velocity.x = speed;
                    spriteRenderer.flipX = false;
                }
            }
            if (!verticalMovement)
            {
                velocity.y = 0;
            }
            else
            {
                if (upInput)
                {
                    velocity.y = speed;
                }
                else
                {
                    velocity.y = -speed;
                }
            }
            rb.velocity = velocity;
            
        }

        
        public float GetMaxHealth()
        {
            return currentRobot.BaseHealth + SelfRobotUpgradeInfo.HEALTH_PER_UPGRADE * RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Health);
        }

        public ulong GetEnergyStorage()
        {
            return currentRobot.MaxEnergy * 2 << RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Energy);
        }

        
        public float GetFriction()
        {
            if (SlipperyFrames > 0) return MovementStats.iceFriction;

            return IsOnGround() ? MovementStats.friction : MovementStats.airFriction;
        }
        
        public bool IsOnGround()
        {
            return CollisionStateActive(CollisionState.OnGround) || (CollisionStateActive(CollisionState.OnPlatform) || CollisionStateActive(CollisionState.OnSlope)) && IgnorePlatformFrames < 0 && rb.velocity.y < 0.05;
        }

        public bool WalkingIntoSlope(Direction direction)
        {
            Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y+Global.TILE_SIZE/2f);
            float playerWidth = spriteRenderer.sprite.bounds.extents.x;
            const float BONUS = 0.1f;
            float xExtent = playerWidth / 2f+BONUS;
            switch (direction)
            {
                case Direction.Left:
                    bottomCenter.x -= xExtent;
                    break;
                case Direction.Right:
                    bottomCenter.x += xExtent;
                    break;
                default:
                    break;
            }

            int layer = blockLayer;
            
            RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,Global.TILE_SIZE/2f),0,Vector2.zero,Mathf.Infinity,layer);
            return !ReferenceEquals(raycastHit.collider, null);
        }
        
        
        public void FixedUpdate()
        {
            CoyoteFrames--;
            SlipperyFrames--;
            IgnorePlatformFrames--;
            IgnoreSlopePlatformFrames--;
            InvincibilityFrames--;
            HighDragFrames--;
            if (currentRobot is IEnergyRechargeRobot energyRechargeRobot) EnergyRechargeUpdate(energyRechargeRobot);
            
            CanStartClimbing();
            if (PlayerDamage.TimeSinceDamaged > SelfRobotUpgradeInfo.NANO_BOT_DELAY && robotData.nanoBotTime > 0)
            {
                NanoBotHeal();
            }
            
            if (climbing) {
                HandleClimbing();
                return;
            }

            if (HighDragFrames == 0)
            {
                rb.drag = DefaultLinearDrag;
            }

            platformCollider.enabled = IgnorePlatformFrames < 0 && rb.velocity.y < 0.01f;
            bool ignoreSlopedPlatforms = ControlUtils.GetControlKey(PlayerControl.MoveDown) && collisionStates.Contains(CollisionState.OnPlatform);
            leftSlopePlatformCollider.enabled = IgnoreSlopePlatformFrames < 0 && collisionStates.Contains(CollisionState.OnLeftSlopePlatform) && !ignoreSlopedPlatforms;
            rightSlopePlatformCollider.enabled = IgnoreSlopePlatformFrames < 0 && collisionStates.Contains(CollisionState.OnRightSlopePlatform) && !ignoreSlopedPlatforms;
            bool grounded = IsGrounded();
            AnimationController.ToggleBool(PlayerAnimationState.Air,CoyoteFrames < 0 && !grounded);
            if (grounded)
            {
                CoyoteFrames = JumpStats.coyoteFrames;
            }
            else
            {
                if (CoyoteFrames < 0)
                {
                    AnimationController.PlayAnimation(PlayerAnimation.Air, IsUsingTool);
                }
                
                if ((DevMode.Instance.flight || RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Flight) > 0) && playerScript.PlayerStatisticCollection != null)
                {
                    playerScript.PlayerStatisticCollection.ContinuousValues[PlayerStatistic.Flight_Time] += Time.fixedDeltaTime;
                }
            }

            if (!InFluid() && IsOnGround())
            {
                CurrentTileMovementType = GetTileMovementModifier();
                SlipperyFrames = CurrentTileMovementType == TileMovementType.Slippery ? MovementStats.iceNoAirFrictionFrames : 0;
            }
            
            if (!DevMode.Instance.flight)
            {
                CalculateFallTime();
                ClampFallSpeed();
                LiveYUpdates--;
                

                rb.constraints = GetFreezeConstraints();
            }
        }

        private RigidbodyConstraints2D GetFreezeConstraints()
        {
            const RigidbodyConstraints2D FREEZE_Y =
                RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            const RigidbodyConstraints2D FREEZE_Z = RigidbodyConstraints2D.FreezeRotation;
            ;
            const float epilson = 0.1f;
            if (freezeY) return FREEZE_Y;
            if (CurrentTileMovementType == TileMovementType.Slippery && CollisionStateActive(CollisionState.OnSlope)) return RigidbodyConstraints2D.FreezeRotation;

            if (LiveYUpdates > 0) return FREEZE_Z;
            
            if (CollisionStateActive(CollisionState.OnSlope) || IsOnSlopedPlatform())
            {
                bool moving = ControlUtils.GetControlKey(PlayerControl.MoveLeft) || ControlUtils.GetControlKey(PlayerControl.MoveRight);
                bool touchingWall = CollisionStateActive(CollisionState.OnWallLeft) || CollisionStateActive(CollisionState.OnWallRight);
                return moving && !touchingWall ? FREEZE_Z : FREEZE_Y;
            }
            if (CollisionStateActive(CollisionState.OnWallLeft) || CollisionStateActive(CollisionState.OnWallRight)) return FREEZE_Z;
            return IsOnGround() && rb.velocity.y < epilson
                ? FREEZE_Y
                : FREEZE_Z;
        }
        

        private void EnergyRechargeUpdate(IEnergyRechargeRobot energyRechargeRobot)
        {
            ulong maxEnergy = GetEnergyStorage();
            if (robotData.Energy >= currentRobot.MaxEnergy) return;
            
            robotData.Energy += energyRechargeRobot.EnergyRechargeRate;
            if (robotData.Energy > currentRobot.MaxEnergy) robotData.Energy = currentRobot.MaxEnergy;
        }

        /// <summary>
        /// Inserts energy into the player robot
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>Returns amount taken</returns>
        public ulong GiveEnergy(ulong amount)
        {
            ulong storage = GetEnergyStorage();
            if (robotData.Energy >= storage)
            {
                return 0;
            }
            ulong sum = robotData.Energy+=amount;
            if (sum > storage) {
                robotData.Energy = storage;
                return sum - storage;
            }
            robotData.Energy = sum;
            return amount;
        }

        
        public bool TryConsumeEnergy(ulong energy, float minPercent)
        {
            if (DevMode.Instance.NoEnergyCost) return true;
            ulong current = robotData.Energy;
            ulong max = currentRobot.MaxEnergy;
            if (current < energy || (float)(current - energy)/max < minPercent) return false;
            robotData.Energy -= energy;
            return true;
        }
        
        /// <summary>
        /// This function consumes a given float value from the robot's energy.
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="minPercent"></param>
        /// <example>2.25f -> Consumes 2 energy + 1 25% of the time</example>
        /// <returns></returns>
        public bool TryConsumeEnergy(float energy, float minPercent)
        {
            if (DevMode.Instance.NoEnergyCost) return true;
            ulong current = robotData.Energy;
            ulong max = currentRobot.MaxEnergy;
            if (current < energy || (float)(current - energy)/max < minPercent) return false;
            ulong intCost = (ulong)energy;
            
            float remaining = energy-intCost;
            if (remaining > 0.01f)
            {
                float ran = UnityEngine.Random.value;
                intCost += Convert.ToUInt64(remaining < ran);
            }
            robotData.Energy -= intCost;
            return true;
        }

        private void ClampFallSpeed()
        {
            if (!(rb.velocity.y < -TERMINAL_VELOCITY)) return;
            
            var vector2 = rb.velocity;
            vector2.y = -TERMINAL_VELOCITY;
            rb.velocity = vector2;
        }
        

        private PlayerPickUp GetPlayerPick()
        {
            return GetComponentInChildren<PlayerPickUp>();
        }

        public void TemporarilyPausePlayer()
        {
            fluidCollisionInformation.Clear();
            PlayerPickUp playerPickup = GetPlayerPick();
            playerPickup.CanPickUp = false;
            immuneToNextFall = true;
            InvincibilityFrames = int.MaxValue;
            freezeY = true;
            StartCoroutine(UnPausePlayer());
        }

        private IEnumerator UnPausePlayer()
        {
            yield return new WaitForSeconds(0.2f);
            InvincibilityFrames = 0;
            fluidCollisionInformation.Clear();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            PlayerPickUp playerPickup = GetComponentInChildren<PlayerPickUp>();
            playerPickup.CanPickUp = true;
            freezeY = false;
            List<CollisionState> stateArray = collisionStates.ToList();
            foreach (CollisionState collisionState in stateArray)
            {
                RemoveCollisionState(collisionState);
            }
        }

        private void CalculateFallTime()
        {
            if (!IsOnGround() && !InFluid())
            {
                if (rb.velocity.y < 0) fallTime += Time.fixedDeltaTime;
                return;
            }
            
            if (immuneToNextFall)
            {
                immuneToNextFall = false;
                fallTime = 0;
                return;
            }
            
            if (fallTime <= 0) return;
            
            
            
            const float DAMAGE_RATE = 2;
            const float MIN_DAMAGE = 1f;

            float damage = DAMAGE_RATE * fallTime * fallTime;
    
            fallTime = 0f;
            if (damage < MIN_DAMAGE) return;
            
            PlayerDamage.Damage(damage);
        }

        public void SetFlightProperties()
        {
            rb.bodyType = DevMode.Instance.flight ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
        }

        private void CreativeFlightMovementUpdate(Transform playerTransform)
        {
            Vector3 position = playerTransform.position;
            float movementSpeed = DevMode.Instance.FlightSpeed * Time.deltaTime;
            if (ControlUtils.GetControlKey(PlayerControl.MoveLeft) || Input.GetKey(KeyCode.LeftArrow)) {
                position.x -= movementSpeed;
                spriteRenderer.flipX = true;
            }
            if (ControlUtils.GetControlKey(PlayerControl.MoveRight) || Input.GetKey(KeyCode.RightArrow)) {
                position.x += movementSpeed;
                spriteRenderer.flipX = false;
            }
            if (ControlUtils.GetControlKey(PlayerControl.MoveUp) || Input.GetKey(KeyCode.UpArrow)) {
                position.y += movementSpeed;
            }
            if (ControlUtils.GetControlKey(PlayerControl.MoveDown) || Input.GetKey(KeyCode.DownArrow)) {
                position.y -= movementSpeed;
            }
            playerTransform.position = position;
        }

        public void Heal(float amount)
        {
            robotData.Health += amount;
            PlayerParticles.PlayParticle(PlayerParticle.NanoBots);
            float maxHealth = GetMaxHealth();
            if (robotData.Health > maxHealth) robotData.Health = maxHealth;
        }

        public void NanoBotHeal()
        {
            float maxHealth = GetMaxHealth();
            if (robotData.Health >= maxHealth) return;
            robotData.Health += maxHealth * 0.0025f;
            robotData.nanoBotTime -= Time.fixedDeltaTime;
            PlayerParticles.PlayParticle(PlayerParticle.NanoBots);
            if (robotData.Health > maxHealth) robotData.Health = maxHealth;
        }
        
        public void RefreshNanoBots()
        {
            PlayerParticles.PlayParticle(PlayerParticle.NanoBots);
            robotData.nanoBotTime = SelfRobotUpgradeInfo.NANO_BOT_TIME_PER_UPGRADE * RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.NanoBots);
            float maxHealth = GetMaxHealth();
            if (robotData.Health >= maxHealth) return;
            robotData.Health += maxHealth * 0.0025f;
            if (robotData.Health > maxHealth) robotData.Health = maxHealth;
        }
        

        public void ResetInvinicibleFrames()
        {
            InvincibilityFrames = 15;
        }

        public void ResetLiveYFrames()
        {
            LiveYUpdates = 3;
        }
        public void Respawn()
        {
            spriteRenderer.enabled = true;
            fluidCollisionInformation.Clear();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            robotData.Health = GetMaxHealth();
            DimensionManager.Instance.SetPlayerSystem(playerScript,0,new Vector2Int(0,0));
        }

        public void Die()
        {
            CollisionState[] stateArray = collisionStates.ToArray();
            foreach (CollisionState state in stateArray)
            {
                RemoveCollisionState(state);
            }
            spriteRenderer.enabled = false;
            PlayerPickUp playerPickup = GetPlayerPick();
            playerPickup.CanPickUp = false;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            PlayerDeathScreenUI playerDeathScreenUI = Instantiate(deathScreenUIPrefab);
            playerDeathScreenUI.Initialize(playerScript);
            CanvasController.Instance.DisplayOnParentCanvas(playerDeathScreenUI.gameObject);
            playerScript.PlayerInventory.DropAll();
            
        }
        private void CanStartClimbing() {
            if (rb.bodyType == RigidbodyType2D.Static) {
                return;
            }
            
            bool climbKeyInput = ControlUtils.GetControlKey(PlayerControl.MoveUp) || ControlUtils.GetControlKey(PlayerControl.MoveDown);
            if (climbing || !climbKeyInput || GetClimbable(transform.position) == null) return;
            
            climbing = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0;
            Vector3 position = transform.position;
            float x = position.x;
            x = Mathf.Floor(x*2)/2f+0.25f;
            position.x = x;
            transform.position = position;
        }

        private TileMovementType GetTileMovementModifier()
        {
            Vector2 extent = spriteRenderer.sprite.bounds.extents;
            float y = gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y - Global.TILE_SIZE/4f;
            float xDif = extent.x-0.1f;
            Vector2 bottomLeft = new Vector2(gameObject.transform.position.x-xDif, y);
            Vector2 bottomRight = new Vector2(gameObject.transform.position.x+xDif, y);
            TileMovementType left = GetMovementTypeAtWorldPosition(bottomLeft);
            TileMovementType right = GetMovementTypeAtWorldPosition(bottomRight);
            
            if (left == TileMovementType.Slow || right == TileMovementType.Slow) return TileMovementType.Slow;
            if (left == TileMovementType.Slippery || right == TileMovementType.Slippery) return TileMovementType.Slippery;
            return TileMovementType.None;
        }

        private TileItem GetTileItemBelow(Vector2 position)
        {
            // This can be made more efficent without a get component
            Vector2 tileCenter = TileHelper.getRealTileCenter(position);
            RaycastHit2D objHit = Physics2D.BoxCast(tileCenter,new Vector2(Global.TILE_SIZE-0.02f,Global.TILE_SIZE-0.02f),0,Vector2.zero,Mathf.Infinity,baseCollidableLayer);
            if (ReferenceEquals(objHit.collider, null)) return null;
            ILoadedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(tileCenter);
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            
            return partition?.GetTileItem(positionInPartition, TileMapLayer.Base);
        }
        private TileMovementType GetMovementTypeAtWorldPosition(Vector2 position)
        {
            TileItem tileItem = GetTileItemBelow(position);
            return tileItem?.tileOptions.movementModifier ?? TileMovementType.None;
        }
        private IClimableTileEntity GetClimbable(Vector2 position) {
            int objectLayer = (1 << LayerMask.NameToLayer("Object"));
            RaycastHit2D objHit = Physics2D.BoxCast(position,new Vector2(0.5f,0.1f),0,Vector2.zero,Mathf.Infinity,objectLayer);
            
            WorldTileMap worldTileMap = objHit.collider?.GetComponent<WorldTileMap>();
            if (ReferenceEquals(worldTileMap, null)) return null;
            
            TileItem tileItem = worldTileMap.getTileItem(Global.GetCellPositionFromWorld(position));
            return tileItem?.tileEntity as IClimableTileEntity;
        }

        private void HandleClimbing() {
            IClimableTileEntity climableTileEntity = GetClimbable(transform.position);
            if (climableTileEntity == null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                climbing = false;
                rb.gravityScale = DefaultGravityScale;
                return;
            }
            IClimableTileEntity below = GetClimbable((Vector2)transform.position + Vector2.down);
            platformCollider.enabled = below == null;
            Vector2 velocity = rb.velocity;
            fallTime = 0;
            if (ControlUtils.GetControlKey(PlayerControl.MoveUp)) {
                velocity.y = climableTileEntity.GetSpeed();
            } else if (ControlUtils.GetControlKey(PlayerControl.MoveDown)) {
                velocity.y = -climableTileEntity.GetSpeed();
            } else {
                velocity.y = 0;
            }
            rb.velocity = velocity;
        }

        public void Initialize(ItemSlot itemSlot, RobotUpgradeLoadOut loadOutData)
        {
            SetRobot(itemSlot);
            RobotUpgradeLoadOut = RobotUpgradeUtils.VerifyIntegrityOfLoadOut(loadOutData,robotData);
            InitializeTools();
        }
        
        public void SetRobot(ItemSlot newRobot)
        {
            this.robotItemSlot = newRobot;
            if (ItemSlotUtils.IsItemSlotNull(robotItemSlot) || robotItemSlot.itemObject is not RobotItem robotItem) {
                Debug.LogWarning("Tried to set invalid robot");
                robotItemSlot = RobotDataFactory.GetDefaultRobot();
                robotItem = robotItemSlot.itemObject as RobotItem;
            }
            currentRobot = robotItem.robot;
            
            ItemTagCollection tags = this.robotItemSlot.tags;
            if (tags?.Dict == null || !tags.Dict.ContainsKey(ItemTag.RobotData) || tags.Dict[ItemTag.RobotData] is not RobotItemData)
            {
                this.robotItemSlot = RobotDataFactory.GetDefaultRobot();
            }
            
            robotData = (RobotItemData)robotItemSlot.tags.Dict[ItemTag.RobotData];
            SetFlightProperties();
            spriteRenderer.sprite = currentRobot.defaultSprite; 
        }

        public void SetRobotLoadOut(RobotUpgradeLoadOut loadOut)
        {
            this.RobotUpgradeLoadOut = loadOut;
        }
        private void InitializeTools()
        {
            ItemRobotToolData itemRobotToolData = robotData.ToolData;
            currentRobotToolObjects = RobotToolFactory.GetDictFromCollection(currentRobot.ToolCollection);

            foreach (var (robotToolType, robotToolObject) in currentRobotToolObjects)
            {
                if (!itemRobotToolData.Types.Contains(robotToolType))
                {
                    itemRobotToolData.Types.Add(robotToolType);
                    itemRobotToolData.Tools.Add(RobotToolFactory.GetDefault(robotToolType));
                    itemRobotToolData.Upgrades.Add(new List<RobotUpgradeData>());
                    RobotUpgradeLoadOut.ToolLoadOuts[robotToolType] = RobotUpgradeUtils.CreateNewLoadOutCollection(RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Tool,(int)robotToolType));
                }
            }

            while (itemRobotToolData.Tools.Count < itemRobotToolData.Types.Count)
            {
                itemRobotToolData.Tools.Add(null);
            }
            while (itemRobotToolData.Upgrades.Count < itemRobotToolData.Types.Count)
            {
                itemRobotToolData.Upgrades.Add(new List<RobotUpgradeData>());
            }
            
            RobotTools = new List<IRobotToolInstance>();
            for (int i = 0; i < itemRobotToolData.Tools.Count; i++)
            {
                var type = itemRobotToolData.Types[i];
                itemRobotToolData.Tools[i] ??= RobotToolFactory.GetDefault(type);
                var data = itemRobotToolData.Tools[i];
                
                var loadOut = RobotUpgradeLoadOut.ToolLoadOuts.GetValueOrDefault(type);
                if (!currentRobotToolObjects.TryGetValue(type, out var toolObject)) continue;
                
                RobotTools.Add(RobotToolFactory.GetInstance(type,toolObject,data,loadOut,playerScript));
            }
        }

        public class FluidCollisionInformation
        {
            public float DamageCounter;
            public bool Colliding;
            public float SpeedModifier;
            public float GravityModifier => SpeedModifier / 4f;
            public float Damage;

            public void SetFluidItem(FluidTileItem fluidTileItem)
            {
                DamageCounter = 0;
                Colliding = true;
                SpeedModifier = fluidTileItem.fluidOptions.SpeedSlowFactor;
                Damage = fluidTileItem.fluidOptions.DamagePerSecond;
            }

            public void Clear()
            {
                DamageCounter = 0;
                Colliding = false;
            }
        }
        
        

        [System.Serializable]
        private class RobotUpgradeAssetReferences
        {
            public AssetReference RocketBootParticles;
            public AssetReference BonusJumpParticles;
            public AssetReference TeleportParticles;
            public AssetReference NanoBotParticles;
        }

        public void ResetIgnorePlatformFrames()
        {
            IgnorePlatformFrames = 8;
        }

        public void ResetIgnoreSlopePlatformFrames()
        {
            IgnoreSlopePlatformFrames = 12;
        }

        public void SetLiveY(int frames)
        {
            LiveYUpdates = frames;
        }
    }
    [System.Serializable]
    internal class DirectionalMovementStats
    {
        public float minMove = 0.2f;
        public float accelationModifier = 3f;
        public float friction = 10;
        public float turnRate = 10f;
        public float moveModifier = 2f;
        public float airFriction = 5;
        public float speed = 5f;
        public float airSpeedIncrease = 1.1f;
        public float iceFriction = 0.1f;
        public float slowSpeedReduction = 0.25f;
        public int iceNoAirFrictionFrames = 10;
        public float defaultDragOnSlope = 5f;
        public float speedUpgradeDragIncrease = 5f;
    }

    [System.Serializable]
    internal class JumpMovementStats
    {
        public float initialGravityPercent = 0;
        public float jumpVelocity = 8f;
        public float maxGravityTime = 0.5f;
        public int coyoteFrames;
    }

}
