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

namespace Player {
    public enum CollisionState
    {
        OnWallLeft,
        OnWallRight,
        OnGround,
        OnSlope,
        HeadContact,
        OnPlatform,
        HeadInFluid,
        FeetInFluid,
    }
    public class PlayerRobot : MonoBehaviour
    {
        private enum RobotParticleSystems
        {
            Teleport,
            BonusJump,
            Heal
        }
        private static readonly int Walk = Animator.StringToHash("IsWalking");
        private static readonly int Air = Animator.StringToHash("InAir");

        [SerializeField] private PlayerRobotUI mPlayerRobotUI;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private PlayerDeathScreenUI deathScreenUIPrefab;
        private PolygonCollider2D polygonCollider;
        private bool climbing;
        private bool autoJumping;
        private Animator animator;
        private HashSet<CollisionState> collisionStates = new HashSet<CollisionState>();
        [SerializeField] public ItemSlot robotItemSlot;
        private RobotObject currentRobot;
        public RobotObject CurrentRobot => currentRobot;
        private RobotItemData robotData;
        public RobotItemData RobotData => robotData;
        public List<IRobotToolInstance> RobotTools;
        private Dictionary<RobotToolType, RobotToolObject> currentRobotToolObjects;
        public List<RobotToolType> ToolTypes => robotData.ToolData.Types;
        private float fallTime;
        private float defaultGravityScale;
        private bool immuneToNextFall = false;
        private int iFrames;
        private TileMovementType currentTileMovementType;
        public bool Dead => robotData.Health <= 0;
        private CameraBounds cameraBounds;

        private const float TERMINAL_VELOCITY = 30f;
        private int liveYUpdates = 0;
        private int blockLayer;
        private int baseCollidableLayer;
        private int ignorePlatformFrames;
        private int slipperyFrames;
        private float moveDirTime;
        private int coyoteFrames;
        private int bonusJumps;
        private bool recalling;
        private RocketBoots rocketBoots;
        private PlayerScript playerScript;
        
        [SerializeField] internal DirectionalMovementStats MovementStats;
        [SerializeField] internal JumpMovementStats JumpStats;
        [SerializeField] private RobotUpgradeAssetReferences RobotUpgradeAssets;

        public RobotUpgradeLoadOut RobotUpgradeLoadOut;
        
        private JumpEvent jumpEvent;
        private bool freezeY;
        private PlayerTeleportEvent playerTeleportEvent;
        private ParticleSystem bonusJumpParticles;
        private ParticleSystem teleportParticles;
        private ParticleSystem nanoBotParticles;
        private float timeSinceDamaged = 0;
        private FluidCollisionInformation fluidCollisionInformation = new();

        public const float BASE_MOVE_SPEED = 5f;
        
        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            playerScript = GetComponent<PlayerScript>();
            blockLayer = 1 << LayerMask.NameToLayer("Block");
            baseCollidableLayer = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform"));
            defaultGravityScale = rb.gravityScale;
            cameraBounds = Camera.main.GetComponent<CameraBounds>();
            animator = GetComponent<Animator>();
            LoadAsyncAssets();
        }

        private void LoadAsyncAssets()
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
            
            
            StartCoroutine(LoadAsset(RobotUpgradeAssets.BonusJumpParticles, (GameObject result) =>
            {
                bonusJumpParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
            StartCoroutine(LoadAsset(RobotUpgradeAssets.TeleportParticles, (GameObject result) =>
            {
                teleportParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
            StartCoroutine(LoadAsset(RobotUpgradeAssets.NanoBotParticles, (GameObject result) =>
            {
                nanoBotParticles = result.gameObject.GetComponent<ParticleSystem>();
            }));
        }

        public void Update()
        {
            mPlayerRobotUI.Display(this);
            MoveUpdate();
            FluidDamageUpdate();
            MiscKeyListens();
            timeSinceDamaged += Time.deltaTime;
        }

        private void FluidDamageUpdate()
        {
            if (!fluidCollisionInformation.Colliding || fluidCollisionInformation.Damage <= 0.05f) return;
            fluidCollisionInformation.DamageCounter += Time.deltaTime;
            if (fluidCollisionInformation.DamageCounter < 1f) return;
            Damage(fluidCollisionInformation.Damage);
            fluidCollisionInformation.DamageCounter = 0;
        }

        private void MiscKeyListens()
        {
            if (PlayerKeyPressUtils.BlockKeyInput) return;
            if (ControlUtils.GetControlKeyDown(PlayerControl.SwapRobotLoadOut))
            {
                int direction = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                RobotUpgradeLoadOut.SelfLoadOuts.IncrementCurrent(direction);
            }
        }

        public void AddCollisionState(CollisionState state)
        {
            if (!collisionStates.Add(state)) return;
            if (state is CollisionState.OnGround or CollisionState.OnSlope or CollisionState.OnPlatform)
            {
                liveYUpdates = 3;
                var vector2 = rb.velocity;
                vector2.y = 0;
                rb.velocity = vector2;
                if (bonusJumps <= 0)
                {
                    bonusJumps = RobotUpgradeLoadOut?.SelfLoadOuts?.GetCurrent()?.GetDiscreteValue((int)RobotUpgrade.BonusJump) ?? 0;
                }

                int rocketBootUpgrades = RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.RocketBoots);
                if (rocketBootUpgrades > 0)
                {
                    rocketBoots?.Terminate();
                    rocketBoots ??= new RocketBoots();
                    rocketBoots.FlightTime = 1+rocketBootUpgrades;
                }
                else
                {
                    rocketBoots = null;
                }
            }

            if (state is CollisionState.FeetInFluid)
            {
                var vector2 = rb.velocity;
                vector2.y = vector2.y * 0.05f;
                rb.velocity = vector2;
                fallTime = 0;

                Vector3 position = transform.position;
                position.z = 2;
                transform.position = position;
            }
        }

        public void AddFluidCollisionData(CollisionState collisionState, FluidTileItem fluidTileItem)
        {
            if (!collisionStates.Contains(collisionState)) return;
            if (collisionState != CollisionState.FeetInFluid) return;
            if (!fluidTileItem) return;
            if (fluidTileItem.fluidOptions.DamagePerSecond > 0)
            {
                // Deal half damage on first collision
                Damage(fluidTileItem.fluidOptions.DamagePerSecond/2f);
            }
            fluidCollisionInformation.SetFluidItem(fluidTileItem);
            
        }

        public void RemoveCollisionState(CollisionState state)
        {
            if (!collisionStates.Remove(state)) return;
            if (state == CollisionState.FeetInFluid)
            {
                Vector3 position = transform.position;
                position.z = -5;
                transform.position = position;
                fluidCollisionInformation.Clear();
            }
        }

        public bool CollisionStateActive(CollisionState state)
        {
            return collisionStates.Contains(state);
        }

        public bool InFluid()
        {
            return collisionStates.Contains(CollisionState.HeadInFluid) && collisionStates.Contains(CollisionState.FeetInFluid);
        }

        private void MoveUpdate()
        {
            if (!PlayerKeyPressUtils.BlockKeyInput)
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
                animator.SetBool(Walk,false);
                if (PlayerKeyPressUtils.BlockKeyInput) return;
                CreativeFlightMovementUpdate(transform);
                return;
            }
            
            float flight = RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Flight);


            if (flight > 0 && TryConsumeEnergy(SelfRobotUpgradeInfo.FLIGHT_COST, 0))
            {
                animator.SetBool(Walk,false);
                FlightMoveUpdate();
            }
            else
            {
                StandardMoveUpdate();
            }
            
        }

        private IEnumerator RecallCoroutine()
        {
            recalling = true;
            // TODO some sound effect and animation
            const float RECALL_DELAY = 0.2f;
            yield return new WaitForSeconds(RECALL_DELAY);
            DimensionManager.Instance.SetPlayerSystem(GetComponent<PlayerScript>(),0,Vector2Int.zero);
            recalling = false;
        }

        private void TeleportUpdate()
        {
            if (playerTeleportEvent != null)
            {
                playerTeleportEvent.IterateTime(Time.deltaTime);
                if (playerTeleportEvent.Expired()) playerTeleportEvent = null;
            }
            if (ControlUtils.GetControlKeyDown(PlayerControl.Teleport) && playerTeleportEvent == null)
            {
                if (!TryConsumeEnergy(SelfRobotUpgradeInfo.TELEPORT_COST, 0)) return;
                Camera mainCamera = Camera.main;
                if (!mainCamera) return;
                Vector2 teleportPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                playerTeleportEvent = new PlayerTeleportEvent(transform, teleportPosition, spriteRenderer.sprite.bounds);
                bool teleported = playerTeleportEvent.TryTeleport();
                if (teleported)
                {
                    playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Teleportations]++;
                    teleportParticles.Play();
                    fallTime = 0;
                }
            }
        }

        public bool CanJump()
        {
            return IsGrounded() || (bonusJumps > 0 && TryConsumeEnergy(SelfRobotUpgradeInfo.BONUS_JUMP_COST, 0));
        }

        public bool IsGrounded()
        {
            return CollisionStateActive(CollisionState.OnPlatform) || CollisionStateActive(CollisionState.OnGround) || CollisionStateActive(CollisionState.OnSlope);
        }

        private void FlightMoveUpdate()
        {
            rb.gravityScale = 0;
            bool blockInput = PlayerKeyPressUtils.BlockKeyInput;
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

        private void StandardMoveUpdate()
        {
            bool blockInput = PlayerKeyPressUtils.BlockKeyInput;
            Vector2 velocity = rb.velocity;
            
            bool movedLeft = !CollisionStateActive(CollisionState.OnWallLeft) && !blockInput && DirectionalMovementUpdate(Direction.Left, PlayerControl.MoveLeft);
            bool movedRight = !CollisionStateActive(CollisionState.OnWallRight) && !blockInput && DirectionalMovementUpdate(Direction.Right, PlayerControl.MoveRight);

            bool moveUpdate = movedLeft != movedRight; // xor
            if (!moveUpdate)
            {
                float dif = GetFriction();
                
                if (moveDirTime > 0)
                {
                    moveDirTime -= dif;
                    if (moveDirTime < 0) moveDirTime = 0;
                }
                if (moveDirTime < 0)
                {
                    moveDirTime += dif;
                    if (moveDirTime > 0) moveDirTime = 0;
                }
            }
            
            if (IsGrounded())
            {
                if (!moveUpdate)
                {
                    animator.Play("Idle");
                    animator.speed = 1;
                }
                else
                {
                    const float ANIMATOR_SPEED_INCREASE = 0.25f;
                    animator.speed = 1 + ANIMATOR_SPEED_INCREASE*RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);
                    animator.Play("Walk");
                }
            }


            animator.SetBool(Walk,moveUpdate);

            const float MAX_MOVE_DIR = 1;
            
            if (moveDirTime > MAX_MOVE_DIR) moveDirTime = MAX_MOVE_DIR;
            if (moveDirTime < -MAX_MOVE_DIR) moveDirTime = -MAX_MOVE_DIR;
            
            int sign = moveDirTime < 0 ? -1 : 1;
            float wishdir = MovementStats.accelationModifier*moveDirTime * sign;
            
            float speed = MovementStats.speed;
            float speedUpgrades = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);;
            if (wishdir > 0.05f && TryConsumeEnergy(speedUpgrades * SelfRobotUpgradeInfo.SPEED_INCREASE_COST_PER_SECOND*Time.deltaTime, 0.1f))
            {
                speed += speedUpgrades;
            }

            if (fluidCollisionInformation.Colliding) speed *= fluidCollisionInformation.SpeedModifier;
            switch (currentTileMovementType)
            {
                case TileMovementType.None:
                    break;
                case TileMovementType.Slippery:
                    speed *= 1.2f;
                    break;
                case TileMovementType.Slow:
                    speed *= MovementStats.slowSpeedReduction;
                    break;
            }
            if (!IsOnGround()) speed *= MovementStats.airSpeedIncrease;
            velocity.x = sign * Mathf.Lerp(0,speed,wishdir);

            SpaceBarMovementUpdate(ref velocity);
            UpdateVerticalMovement(ref velocity);
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

        private void UpdateVerticalMovement(ref Vector2 velocity)
        {
            float fluidGravityModifer = fluidCollisionInformation.Colliding ? fluidCollisionInformation.GravityModifier : 1f;
            if (climbing) return;
            if (jumpEvent != null)
            {
                if (collisionStates.Contains(CollisionState.HeadContact))
                {
                    rb.gravityScale = fluidGravityModifer * defaultGravityScale;
                    jumpEvent = null;
                } else if (ControlUtils.GetControlKey(PlayerControl.Jump))
                {
                    rb.gravityScale = fluidGravityModifer * jumpEvent.GetGravityModifier(JumpStats.initialGravityPercent,JumpStats.maxGravityTime) * defaultGravityScale;
                    jumpEvent.IterateTime();
                    if (ControlUtils.GetControlKey(PlayerControl.MoveDown)) 
                    {
                        jumpEvent.IterateTime();
                        rb.gravityScale *= 1.5f;
                    }
                }
                else
                {
                    rb.gravityScale = fluidGravityModifer*defaultGravityScale;   
                }
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    jumpEvent = null;
                }
                return;
            }
            
            float fluidSpeedModifier = fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;
            
            bool blockInput = PlayerKeyPressUtils.BlockKeyInput;
            if (rocketBoots != null && rocketBoots.Active)
            {
                if (blockInput)
                {
                    rocketBoots.Terminate();
                    rocketBoots = null;
                    return;
                }
                if (rocketBoots.Boost > 0)
                {
                    rb.gravityScale = 0;
                    float bonusJumpHeight = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.JumpHeight);
                    velocity.y = fluidSpeedModifier * rocketBoots.Boost * (1+0.33f*bonusJumpHeight);
                }
                else
                {
                    rb.gravityScale = fluidGravityModifer*defaultGravityScale;
                }
            }
            if (blockInput)
            {
                return;
            }

            const float BONUS_FALL_MODIFIER = 1.25f;
            rb.gravityScale = ControlUtils.GetControlKey(PlayerControl.MoveDown) ? defaultGravityScale * BONUS_FALL_MODIFIER : defaultGravityScale;
            rb.gravityScale *= fluidGravityModifer;
        }

        private void SpaceBarMovementUpdate(ref Vector2 velocity)
        {
            if (PlayerKeyPressUtils.BlockKeyInput)
            {
                rb.gravityScale = defaultGravityScale;   
                jumpEvent = null;
                return;
            }
            if (CollisionStateActive(CollisionState.OnPlatform) && ControlUtils.GetControlKey(PlayerControl.Jump) && ControlUtils.GetControlKey(PlayerControl.MoveDown))
            {
                ignorePlatformFrames = 3;
                return;
            }
            
            if (ignorePlatformFrames <= 0 && (CanJump() || coyoteFrames > 0) && ControlUtils.GetControlKeyDown(PlayerControl.Jump))
            {
                if (bonusJumps > 0 && coyoteFrames <= 0)
                {
                    bonusJumpParticles.Play();
                    bonusJumps--;
                }

                float fluidModifier = fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;
                float bonusJumpHeight = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.JumpHeight); 
                velocity.y = fluidModifier*(JumpStats.jumpVelocity+bonusJumpHeight);
                coyoteFrames = 0;
                liveYUpdates = 3;
                
                fallTime = 0;
                jumpEvent = new JumpEvent();
                return;
            }
            
            if (!IsOnGround() && rocketBoots != null)
            {
                if (!rocketBoots.Active && ControlUtils.GetControlKeyDown(PlayerControl.Jump))
                {
                    StartCoroutine(rocketBoots.Activate(RobotUpgradeAssets.RocketBootParticles, transform));
                }

                if (rocketBoots.Active && TryConsumeEnergy(SelfRobotUpgradeInfo.ROCKET_BOOTS_COST_PER_SECOND * Time.deltaTime, 0))
                {
                    rocketBoots.UpdateBoost(ControlUtils.GetControlKey(PlayerControl.Jump));
                    if (rocketBoots.FlightTime < 0)
                    {
                        rocketBoots.Terminate();
                        rocketBoots = null;
                        rb.gravityScale =  fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f * defaultGravityScale;
                    }
                }
            }
        }
        
        private float GetFriction()
        {
            if (currentTileMovementType == TileMovementType.Slippery || slipperyFrames > 0) return MovementStats.iceFriction;

            return IsOnGround() ? MovementStats.friction : MovementStats.airFriction;
        }

        private bool DirectionalMovementUpdate(Direction direction, PlayerControl playerControl)
        {
            if (!ControlUtils.GetControlKey(playerControl)) return false;
            switch (direction)
            {
                case Direction.Left:
                    float lmodifier = moveDirTime < 0 ? MovementStats.moveModifier : MovementStats.turnRate;
                    moveDirTime -= lmodifier * Time.deltaTime;
                    
                    spriteRenderer.flipX = true;
                    if (moveDirTime < 0 && moveDirTime > -MovementStats.minMove) moveDirTime = -MovementStats.minMove;
                    break;
                case Direction.Right:
                    float rmodifier = moveDirTime > 0 ? MovementStats.moveModifier : MovementStats.turnRate;
                    moveDirTime += rmodifier * Time.deltaTime;
                    spriteRenderer.flipX = false;
                    if (moveDirTime > 0 && moveDirTime < MovementStats.minMove) moveDirTime = MovementStats.minMove;
                    break;
            }
            
            
            if (!IsOnGround()) return true;

            if (CanAutoJump(direction)) return true;
            
            if (WalkingIntoSlope(direction))
            {
                liveYUpdates = 3;
            }
            

            return true;
        }

        private bool IsOnGround()
        {
            return CollisionStateActive(CollisionState.OnGround) || (CollisionStateActive(CollisionState.OnPlatform) && ignorePlatformFrames < 0 && rb.velocity.y < 0.05);
        }

        private bool WalkingIntoSlope(Direction direction)
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
            
            RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,Global.TILE_SIZE/2f),0,Vector2.zero,Mathf.Infinity,blockLayer);
            return !ReferenceEquals(raycastHit.collider, null);
        }

        private bool CanAutoJump(Direction direction)
        {
            if (autoJumping) return false;
            Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y+Global.TILE_SIZE/2f);
            Vector2 adjacentTilePosition = bottomCenter + (spriteRenderer.sprite.bounds.extents.x) * (direction == Direction.Left ? Vector2.left : Vector2.right);
            

            const float EPSILON = 0.02f;
            
            var cast = Physics2D.BoxCast(adjacentTilePosition, 
                new Vector2(Global.TILE_SIZE-EPSILON, Global.TILE_SIZE/4f), 
                0f, Vector2.zero, Mathf.Infinity, blockLayer
            );
            if (ReferenceEquals(cast.collider,null)) return false;
            
            var wallCast = Physics2D.BoxCast(
                adjacentTilePosition+Vector2.up*Global.TILE_SIZE, 
                new Vector2(Global.TILE_SIZE-EPSILON, Global.TILE_SIZE/4f), 
                0f, Vector2.zero, Mathf.Infinity, blockLayer
            );
            if (!ReferenceEquals(wallCast.collider,null)) return false;
            
            WorldTileGridMap worldTileMap = cast.collider.GetComponent<WorldTileGridMap>();
            if (ReferenceEquals(worldTileMap, null)) return false;
            ILoadedChunkSystem iLoadedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            Vector2Int cellPosition = Global.getCellPositionFromWorld(adjacentTilePosition);
            var (partition, positionInPartition) = iLoadedChunkSystem.GetPartitionAndPositionAtCellPosition(cellPosition);
            if (partition == null) return false;

            TileItem tileItem = partition.GetTileItem(positionInPartition, TileMapLayer.Base);
            if (tileItem?.tile is not HammerTile) return false;
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
     
            if (baseTileData.state != 1 && baseTileData.state != 3) return false;
            StartCoroutine(AutoJumpCoroutine());
            return true;
        }
        
        private IEnumerator AutoJumpCoroutine()
        {
            autoJumping = true;
            const int STEPS = 3;
            var jumpDestination = transform.position;
            jumpDestination.y += Global.TILE_SIZE / 2f;
            jumpDestination.y = Mathf.CeilToInt(4 * jumpDestination.y) / 4f;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            freezeY = true;
            for (int i = 1; i <= STEPS; i++)
            {
                jumpDestination.x = transform.position.x;
                Vector3 position = Vector3.Lerp(transform.position, jumpDestination, (float)i / STEPS);
                transform.position = position;
                yield return waitForFixedUpdate;
            }

            autoJumping = false;
            freezeY = false;

        }
        public void FixedUpdate()
        {
            coyoteFrames--;
            slipperyFrames--;
            ignorePlatformFrames--;
            iFrames--;
            
            CanStartClimbing();
            if (timeSinceDamaged > SelfRobotUpgradeInfo.NANO_BOT_DELAY && robotData.nanoBotTime > 0)
            {
                NanoBotHeal();
            }
            if (climbing) {
                HandleClimbing();
                return;
            }

            platformCollider.enabled = ignorePlatformFrames < 0 && rb.velocity.y < 0.05;

            if (currentRobot is IEnergyRechargeRobot energyRechargeRobot) EnergyRechargeUpdate(energyRechargeRobot);

            bool grounded = IsGrounded();
            animator.SetBool(Air,coyoteFrames < 0 && !grounded);
            if (grounded)
            {
                coyoteFrames = JumpStats.coyoteFrames;
            }
            else
            {
                if (coyoteFrames < 0) animator.Play("Air");
                
                if ((DevMode.Instance.flight || RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.Flight) > 0) && playerScript.PlayerStatisticCollection != null)
                {
                    playerScript.PlayerStatisticCollection.ContinuousValues[PlayerStatistic.Flight_Time] += Time.fixedDeltaTime;
                }
            }
            
            currentTileMovementType = IsOnGround() ? GetTileMovementModifier() : TileMovementType.None;
            if (currentTileMovementType == TileMovementType.Slippery)
            {
                slipperyFrames = MovementStats.iceNoAirFrictionFrames;
            }
            
            if (!DevMode.Instance.flight)
            {
                CalculateFallTime();
                ClampFallSpeed();
                liveYUpdates--;
                

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
            if (currentTileMovementType == TileMovementType.Slippery && CollisionStateActive(CollisionState.OnSlope)) return RigidbodyConstraints2D.FreezeRotation;

            if (liveYUpdates > 0) return FREEZE_Z;
            
            if (CollisionStateActive(CollisionState.OnSlope) && !ControlUtils.GetControlKey(PlayerControl.MoveLeft) && !ControlUtils.GetControlKey(PlayerControl.MoveRight))
            {
                return FREEZE_Y;
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

        public void SetIFrames(int frames)
        {
            iFrames = frames;
        }

        private PlayerPickUp GetPlayerPick()
        {
            return GetComponentInChildren<PlayerPickUp>();
        }

        public void TemporarilyPausePlayer()
        {
            PlayerPickUp playerPickup = GetPlayerPick();
            playerPickup.CanPickUp = false;
            immuneToNextFall = true;
            iFrames = 50;
            freezeY = true;
            StartCoroutine(UnPausePlayer());
        }

        private IEnumerator UnPausePlayer()
        {
            yield return new WaitForSeconds(0.1f);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            PlayerPickUp playerPickup = GetComponentInChildren<PlayerPickUp>();
            playerPickup.CanPickUp = true;
            freezeY = false;
            collisionStates.Clear(); // Might do weird things
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
            
            
            
            const float DAMAGE_RATE = 4;
            const float MIN_DAMAGE = 1f;

            float damage = DAMAGE_RATE * fallTime * fallTime;
    
            fallTime = 0f;
            if (damage < MIN_DAMAGE) return;
            
            Damage(damage);
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
            nanoBotParticles.Play();
            float maxHealth = GetMaxHealth();
            if (robotData.Health > maxHealth) robotData.Health = maxHealth;
        }

        public void NanoBotHeal()
        {
            float maxHealth = GetMaxHealth();
            if (robotData.Health >= maxHealth) return;
            robotData.Health += maxHealth * 0.0025f;
            robotData.nanoBotTime -= Time.fixedDeltaTime;
            nanoBotParticles.Play();
            if (robotData.Health > maxHealth) robotData.Health = maxHealth;
        }

        public void RefreshNanoBots()
        {
            nanoBotParticles.Play();
            robotData.nanoBotTime = SelfRobotUpgradeInfo.NANO_BOT_TIME_PER_UPGRADE * RobotUpgradeUtils.GetDiscreteValue(RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.NanoBots);
        }

        public bool Damage(float amount)
        {
            if (DevMode.Instance.noHit || iFrames > 0) return false;
            iFrames = 15;
            liveYUpdates = 3;
            robotData.Health -= amount;
            timeSinceDamaged = 0;
            if (robotData.Health > 0) return true;
            
            Die();
            return false;
        }

        public void Respawn()
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            robotData.Health = GetMaxHealth();
            DimensionManager.Instance.SetPlayerSystem(playerScript,0,new Vector2Int(0,0));
        }

        public void Die()
        {
            robotData.Health = 0;
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
            if (climbing || !climbKeyInput || GetClimbable() == null) return;
            
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
            Vector2Int cellPosition = Global.getCellPositionFromWorld(tileCenter);
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            
            return partition?.GetTileItem(positionInPartition, TileMapLayer.Base);
        }
        private TileMovementType GetMovementTypeAtWorldPosition(Vector2 position)
        {
            TileItem tileItem = GetTileItemBelow(position);
            return tileItem?.tileOptions.movementModifier ?? TileMovementType.None;
        }
        private IClimableTileEntity GetClimbable() {
            int objectLayer = (1 << LayerMask.NameToLayer("Object"));
            RaycastHit2D objHit = Physics2D.BoxCast(transform.position,new Vector2(0.5f,0.1f),0,Vector2.zero,Mathf.Infinity,objectLayer);
            
            WorldTileGridMap worldTileGridMap = objHit.collider?.GetComponent<WorldTileGridMap>();
            if (ReferenceEquals(worldTileGridMap, null)) return null;
            
            TileItem tileItem = worldTileGridMap.getTileItem(Global.getCellPositionFromWorld(transform.position));
            return tileItem?.tileEntity as IClimableTileEntity;
        }

        private void HandleClimbing() {
            IClimableTileEntity climableTileEntity = GetClimbable();
            bool exitKeyCode = ControlUtils.GetControlKey(PlayerControl.MoveLeft) || ControlUtils.GetControlKey(PlayerControl.MoveRight) || ControlUtils.GetControlKey(PlayerControl.Jump);
            if (climableTileEntity == null || exitKeyCode)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                climbing = false;
                rb.gravityScale = defaultGravityScale;
                return;
            }
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

        private class FluidCollisionInformation
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

        private class JumpEvent
        {
            private float holdTime;
            public void IterateTime()
            {
                holdTime += Time.deltaTime;
            }
            
            public float GetGravityModifier(float initialGravityPercent, float maxGravityTime)
            {
                return Mathf.Lerp(initialGravityPercent,1,1/maxGravityTime*holdTime);
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
