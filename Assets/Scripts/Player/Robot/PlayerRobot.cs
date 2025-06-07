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
        Climb,
        Flight,
        CreativeFlight
    }
    
    public class PlayerRobot : MonoBehaviour, IPlayerStartupListener
    {
        [SerializeField] private PlayerRobotUI mPlayerRobotUI;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] internal PlayerColliders playerColliders;
        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private Collider2D leftSlopePlatformCollider;
        [SerializeField] private Collider2D rightSlopePlatformCollider;
        [SerializeField] private PlayerDeathScreenUI deathScreenUIPrefab;
        [SerializeField] private PlayerParticles playerParticles;
        [SerializeField] internal DirectionalMovementStats MovementStats;
        [SerializeField] internal JumpMovementStats JumpStats;
        
        private PlayerMovementState movementState;
        private HashSet<CollisionState> collisionStates = new HashSet<CollisionState>();
        public Collider2D PlatformCollider => platformCollider;
        
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
        public int InvincibilityFrames { get; private set; }
        public bool Dead => robotData.Health <= 0;
        
        public int LiveYUpdates { get; private set; }
        public int IgnorePlatformFrames { get; private set; }
        public int IgnoreSlopePlatformFrames { get; private set; }
        
        public bool IsUsingTool { get; private set; }
        private bool paused;
        public int BlockClimbingFrames { get; set; }
        // Upgrades
        
        // References
        private PlayerScript playerScript;
        public FluidCollisionInformation fluidCollisionInformation {get; private set;}
        public RobotArmController gunController;
        public DevMode DevMode { get; private set; }
        public PlayerAnimationController AnimationController { get; private set; }

        public PlayerParticles PlayerParticles => playerParticles;

        public PlayerDamage PlayerDamage { get; private set; }
        
        // Default values
        public float DefaultGravityScale { get; private set; }
        private float defaultBoxColliderWidth;
        private float defaultBoxColliderEdge;
        
        // Layers
        private int blockLayer;
        
        
        private PlayerTeleportEvent playerTeleportEvent;
        
        public const float BASE_MOVE_SPEED = 5f;
        private BasePlayerMovement currentMovement;
        
        private void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            playerScript = GetComponent<PlayerScript>();
            blockLayer = 1 << LayerMask.NameToLayer("Block");
            
            DefaultGravityScale = rb.gravityScale;
            
            gunController.Initialize(this);
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            defaultBoxColliderWidth = boxCollider.size.x;
            defaultBoxColliderEdge = boxCollider.edgeRadius;
            
            PlayerDamage = new PlayerDamage(this);
            DevMode = GetComponent<DevMode>();
            fluidCollisionInformation = new();
            AnimationController = new PlayerAnimationController(this, GetComponent<Animator>());
            
            
            InputActions.ConstantMovementActions constantMovementActions = playerScript.InputActions.ConstantMovement;
            ControlUtils.AssignAction(constantMovementActions.Teleport,PlayerControl.Teleport,Teleport);
            constantMovementActions.Enable();
            
            InitializeMovementState();
            
            enabled = false;
        }

       

        public void InitializeMovementState()
        {
            PlayerMovementState initialState = PlayerMovementState.Standard;
            if (DevMode.flight) initialState = PlayerMovementState.CreativeFlight;
            else if (RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Flight) > 0)
            {
                initialState = PlayerMovementState.Flight;
            }
            SetMovementState(initialState);
        }

        public void SetMovementEventListenerState(bool allowMovement)
        {
            currentMovement?.SetMovementStatus(allowMovement);
            if (allowMovement)
            {
                playerScript.InputActions.ConstantMovement.Enable();
            }
            else
            {
                playerScript.InputActions.ConstantMovement.Disable();
            }
        }
        public void SetMovementState(PlayerMovementState newMovementState)
        {
            if (movementState == newMovementState && currentMovement != null) return;
            currentMovement?.Disable();
            movementState = newMovementState;
            currentMovement = GetMovementHandler(movementState);
        }

        private BasePlayerMovement GetMovementHandler(PlayerMovementState state)
        {
            switch (state)
            {
                case PlayerMovementState.Standard:
                    return new StandardPlayerMovement(this);
                case PlayerMovementState.Climb:
                    return new ClimbingMovement(this);
                case PlayerMovementState.Flight:
                    return new FlightMovement(this);
                case PlayerMovementState.CreativeFlight:
                    return new CreativeFlightMovement(this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void Update()
        {
            if (paused) return;
            mPlayerRobotUI.Display(this);
            if (robotData.Health <= 0) return;
            currentMovement.MovementUpdate();
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
                // Makes it so using a tool doesn't reset walk animation
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
                if (currentMovement is IMovementGroundedListener movementGroundedListener) movementGroundedListener.OnGrounded();
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

            if (state is CollisionState.OnWallLeft or CollisionState.OnWallRight && currentMovement is IOnWallCollisionMovementListener wallListener)
            {
                wallListener.OnWallCollision();
            }
            
            if (state is CollisionState.InFluid)
            {
                if (currentMovement is IOnFluidCollisionMovementListener fluidListener)
                {
                    fluidListener.OnFluidCollision();
                }
                Vector3 position = transform.position;
                position.z = 2;
                transform.position = position;
            }
        }

        public void OnSlopeAddUpdate(Direction slopeDirection)
        {
            if (currentMovement is IOnSlopeCollisionMovementListener slopeListener)
            {
                slopeListener.OnSlopeCollision(slopeDirection);
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
            if (state is CollisionState.OnSlope or CollisionState.OnLeftSlopePlatform or CollisionState.OnRightSlopePlatform && currentMovement is IOnSlopeExitMovementListener slopeExitListener)
            {
                slopeExitListener.OnSlopeExit();
            }
        }

        public void OnSlopeStay(Direction slopeDirection)
        {
            if (currentMovement is IOnSlopeStayMovementListener slopeListener)
            {
                slopeListener.OnSlopeStay(slopeDirection);
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

        
        private void Teleport(InputAction.CallbackContext context)
        {
            if (RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Teleport) <= 0) return;
            if (playerTeleportEvent != null && !playerTeleportEvent.Expired()) return;
            
            if (!TryConsumeEnergy(SelfRobotUpgradeInfo.TELEPORT_COST, 0)) return;
            Camera mainCamera = Camera.main;
            if (!mainCamera) return;
            Vector2 teleportPosition = mainCamera.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            playerTeleportEvent = new PlayerTeleportEvent(transform, teleportPosition, spriteRenderer.sprite.bounds,Time.time);
            bool teleported = playerTeleportEvent.TryTeleport();
            if (!teleported) return;
            playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Teleportations]++;
            PlayerParticles.PlayParticle(PlayerParticle.Teleportation);
            if (currentMovement is IOnTeleportMovementListener teleportListener)
            {
                teleportListener.OnTeleport();
            }
            
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
            LiveYUpdates = 3;
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
        
        public float GetMaxHealth()
        {
            return currentRobot.BaseHealth + SelfRobotUpgradeInfo.HEALTH_PER_UPGRADE * RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Health);
        }

        public ulong GetEnergyStorage()
        {
            return currentRobot.MaxEnergy * 2 << RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Energy);
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
            }

            int layer = blockLayer;
            
            RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,Global.TILE_SIZE/2f),0,Vector2.zero,Mathf.Infinity,layer);
            return !ReferenceEquals(raycastHit.collider, null);
        }
        
        
        public void FixedUpdate()
        {
            if (paused) return;

            BlockClimbingFrames--;
            IgnorePlatformFrames--;
            IgnoreSlopePlatformFrames--;
            InvincibilityFrames--;
            
            if (currentRobot is IEnergyRechargeRobot energyRechargeRobot) EnergyRechargeUpdate(energyRechargeRobot);
            
            if (PlayerDamage.TimeSinceDamaged > SelfRobotUpgradeInfo.NANO_BOT_DELAY && robotData.nanoBotTime > 0)
            {
                NanoBotHeal();
            }
            
            currentMovement?.FixedMovementUpdate();
            LiveYUpdates--;
        }
        
        
        private void EnergyRechargeUpdate(IEnergyRechargeRobot energyRechargeRobot)
        {
            ulong maxEnergy = GetEnergyStorage();
            if (robotData.Energy >= maxEnergy) return;
            
            robotData.Energy += energyRechargeRobot.EnergyRechargeRate;
            if (robotData.Energy > maxEnergy) robotData.Energy = maxEnergy;
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

        
        

        private PlayerPickUp GetPlayerPick()
        {
            return GetComponentInChildren<PlayerPickUp>();
        }

        public void PausePlayer()
        {
            paused = true;
            fluidCollisionInformation.Clear();
            PlayerPickUp playerPickup = GetPlayerPick();
            playerPickup.CanPickUp = false;
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.velocity = Vector2.zero;
            }
            
            InvincibilityFrames = int.MaxValue;
            if (currentMovement is StandardPlayerMovement standardPlayerMovement)
            {
                standardPlayerMovement.OnPlayerPaused();
            }
            
        }

        public void TemporarilyPausePlayer(float delay = 0.1f)
        {
            StartCoroutine(UnPausePlayer(delay));
            if (paused) return;
            PausePlayer();
        }

        private IEnumerator UnPausePlayer(float delay)
        {
            yield return new WaitForSeconds(delay);
            InvincibilityFrames = 0;
            fluidCollisionInformation.Clear();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.velocity = Vector2.zero;
            }
            
            PlayerPickUp playerPickup = GetComponentInChildren<PlayerPickUp>();
            playerPickup.CanPickUp = true;
            List<CollisionState> stateArray = collisionStates.ToList();
            foreach (CollisionState collisionState in stateArray)
            {
                RemoveCollisionState(collisionState);
            }
            paused = false;
            if (currentMovement is StandardPlayerMovement standardPlayerMovement)
            {
                standardPlayerMovement.OnPlayerUnpaused();
            }
        }
        
        public IClimableTileEntity GetClimbable(Vector2 position) {
            int objectLayer = (1 << LayerMask.NameToLayer("Object"));
            RaycastHit2D objHit = Physics2D.BoxCast(position,new Vector2(0.5f,0.5f),0,Vector2.zero,Mathf.Infinity,objectLayer);
            
            WorldTileMap worldTileMap = objHit.collider?.GetComponent<WorldTileMap>();
            if (ReferenceEquals(worldTileMap, null)) return null;
       
            const float OFFSET = 0.1f;
            TileItem tileItem = worldTileMap.getTileItem(Global.WorldToCell(position+Vector2.up*OFFSET));
            if (!tileItem)
            {
                tileItem = worldTileMap.getTileItem(Global.WorldToCell(position+Vector2.down*OFFSET));
            }
            return tileItem?.tileEntity as IClimableTileEntity;
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
            CanvasController.Instance.DisplayObject(playerDeathScreenUI.gameObject,terminateOnEscape:false);
            playerScript.PlayerInventory.DropAll();
        }

        public void TogglePlatformCollider()
        {
            platformCollider.enabled = IgnorePlatformFrames < 0 && rb.velocity.y < 0.01f;
        }

        internal void ToggleSlopePlatformCollider(SlopePlatformCollisionState slopePlatformCollisionState, bool ignoreSlopedPlatforms)
        {
            switch (slopePlatformCollisionState)
            {
                case SlopePlatformCollisionState.Left:
                    leftSlopePlatformCollider.enabled = IgnoreSlopePlatformFrames < 0 && collisionStates.Contains(CollisionState.OnLeftSlopePlatform) && !ignoreSlopedPlatforms;
                    break;
                case SlopePlatformCollisionState.Right:
                    rightSlopePlatformCollider.enabled = IgnoreSlopePlatformFrames < 0 && collisionStates.Contains(CollisionState.OnRightSlopePlatform) && !ignoreSlopedPlatforms;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slopePlatformCollisionState), slopePlatformCollisionState, null);
            }
        }
        
        
        

        

        public void InitializeRobot(ItemSlot itemSlot, RobotUpgradeLoadOut loadOutData)
        {
            mPlayerRobotUI.Initialize();
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
            spriteRenderer.sprite = currentRobot.defaultSprite; 
        }

        public void SafeTeleport(Vector2 position)
        {
            transform.position = position;
            TemporarilyPausePlayer(.1f);
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

        public void OnFlightUpgradeChange()
        {
            if (movementState == PlayerMovementState.CreativeFlight) return;
            int upgrades = RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Flight);
            SetMovementState(upgrades > 0 ? PlayerMovementState.Flight : PlayerMovementState.Standard);
        }
        
        public void OnRocketBootUpgradeChange()
        {
            if (currentMovement is not StandardPlayerMovement standardPlayerMovement) return;
            int upgrades = RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.RocketBoots);
            standardPlayerMovement.ToggleRocketBoots(upgrades > 0);
        }
        

        public void ResetIgnorePlatformFrames()
        {
            IgnorePlatformFrames = 8;
        }

        public void ResetIgnoreSlopePlatformFrames()
        {
            IgnoreSlopePlatformFrames = 10;
        }

        public void SetLiveY(int frames)
        {
            LiveYUpdates = frames;
        }

        public void OnInitialized()
        {
            enabled = true;
        }
        
        internal enum SlopePlatformCollisionState
        {
            Left = CollisionState.OnLeftSlopePlatform,
            Right = CollisionState.OnRightSlopePlatform,
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

    [System.Serializable]
    internal class PlayerColliders
    {
        public BoxCollider2D MainCollider;
        public BoxCollider2D GroundTrigger;
        public BoxCollider2D SlopeTrigger;
        public BoxCollider2D PlatformTrigger;
        public BoxCollider2D LeftWallTrigger;
        public BoxCollider2D RightWallTrigger;
        public BoxCollider2D HeadTrigger;
        public BoxCollider2D LeftPlatformSlopeTrigger;
        public BoxCollider2D RightPlatformSlopeTrigger;

        public void SetStateAll(bool state)
        {
            MainCollider.enabled = state;
            GroundTrigger.enabled = state;
            SlopeTrigger.enabled = state;
            PlatformTrigger.enabled = state;
            LeftWallTrigger.enabled = state;
            RightWallTrigger.enabled = state;
            HeadTrigger.enabled = state;
            LeftPlatformSlopeTrigger.enabled = state;
            RightPlatformSlopeTrigger.enabled = state;
        }

        public void SetStateFlight(bool creative)
        {
            MainCollider.enabled = !creative;
            GroundTrigger.enabled = false;
            SlopeTrigger.enabled = false;
            PlatformTrigger.enabled = false;
            LeftWallTrigger.enabled = !creative;
            RightWallTrigger.enabled = !creative;
            HeadTrigger.enabled = !creative;
            LeftPlatformSlopeTrigger.enabled = false;
            RightPlatformSlopeTrigger.enabled = false;
        }

        public void SetStateStandard()
        {
            SetStateAll(true);
        }

        public void SetStateClimbing()
        {
            MainCollider.enabled = true;
            GroundTrigger.enabled = true;
            SlopeTrigger.enabled = false;
            PlatformTrigger.enabled = true;
            LeftWallTrigger.enabled = false;
            RightWallTrigger.enabled = false;
            HeadTrigger.enabled = false;
            LeftPlatformSlopeTrigger.enabled = true;
            RightPlatformSlopeTrigger.enabled = true;
        }
    }

}
