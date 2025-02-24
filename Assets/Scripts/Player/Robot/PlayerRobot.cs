using System;
using System.Collections;
using System.Collections.Generic;
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
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using RobotModule;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Place;
using Tiles;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
    public enum CollisionState
    {
        OnWallLeft,
        OnWallRight,
        OnGround,
        OnSlope,
        HeadContact,
        OnPlatform
    }
    public class PlayerRobot : MonoBehaviour
    {
        [SerializeField] private PlayerRobotUI mPlayerRobotUI;
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private PlayerDeathScreenUI deathScreenUIPrefab;
        private PolygonCollider2D polygonCollider;
        private bool climbing;
        private bool autoJumping;
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
        private bool dead = false;
        private bool immuneToNextFall = false;
        private int iFrames;
        private TileMovementType currentTileMovementType;
        public bool Dead => dead;
        private CameraBounds cameraBounds;

        private const float TERMINAL_VELOCITY = 30f;
        private int liveYUpdates = 0;
        private int blockLayer;
        private int platformLayer;
        private int baseCollidableLayer;
        private int ignorePlatformFrames;
        private int slipperyFrames;
        private float moveDirTime;
        private int coyoteFrames;
        private int bonusJumps;

        [SerializeField] private DirectionalMovementStats MovementStats;
        [SerializeField] private JumpMovementStats JumpStats;

        public RobotUpgradeLoadOut RobotUpgradeLoadOut;
        
        private JumpEvent jumpEvent;
        private bool freezeY;
        private PlayerTeleportEvent playerTeleportEvent;
        
        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            blockLayer = 1 << LayerMask.NameToLayer("Block");
            platformLayer = 1 << LayerMask.NameToLayer("Platform");
            baseCollidableLayer = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform"));
            defaultGravityScale = rb.gravityScale;
            cameraBounds = Camera.main.GetComponent<CameraBounds>();
        }

        public void Update()
        {
            mPlayerRobotUI.Display(robotData,currentRobot);
            MoveUpdate();
            cameraBounds.UpdateCameraBounds();
        }

        public void AddCollisionState(CollisionState state)
        {
            if (collisionStates.Contains(state)) return;
            if (state is CollisionState.OnGround or CollisionState.OnSlope or CollisionState.OnPlatform)
            {
                liveYUpdates = 3;
            }
            collisionStates.Add(state);
        }

        public void RemoveCollisionState(CollisionState state)
        {
            collisionStates.Remove(state);
        }

        public bool CollisionStateActive(CollisionState state)
        {
            return collisionStates.Contains(state);
        }

        private void MoveUpdate()
        {

            if (!PlayerKeyPressUtils.BlockKeyInput)
            {
                TeleportUpdate();
            }
            if (DevMode.Instance.flight)
            {
                if (PlayerKeyPressUtils.BlockKeyInput) return;
                FlightMovementUpdate(transform);
                return;
            }

            StandardMoveUpdate();
        }

        private void TeleportUpdate()
        {
            if (playerTeleportEvent != null)
            {
                playerTeleportEvent.IterateTime(Time.deltaTime);
                if (playerTeleportEvent.Expired()) playerTeleportEvent = null;
            }
            if (ControlUtils.GetControlKeyDown(ControlConsts.TELEPORT) && playerTeleportEvent == null)
            {
                Camera mainCamera = Camera.main;
                if (!mainCamera) return;
                Vector2 teleportPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                playerTeleportEvent = new PlayerTeleportEvent(transform, teleportPosition, spriteRenderer.sprite.bounds);
                playerTeleportEvent.TryTeleport();
            }
        }

        public bool CanJump()
        {
            return bonusJumps > 0 || IsGrounded();
        }

        public bool IsGrounded()
        {
            return CollisionStateActive(CollisionState.OnPlatform) || CollisionStateActive(CollisionState.OnGround) ||
                   CollisionStateActive(CollisionState.OnSlope);
        }
        private void StandardMoveUpdate()
        {
            bool blockInput = PlayerKeyPressUtils.BlockKeyInput;
            Vector2 velocity = rb.velocity;
            
            bool movedLeft = !CollisionStateActive(CollisionState.OnWallLeft) && !blockInput && DirectionalMovementUpdate(Direction.Left, KeyCode.A, KeyCode.LeftArrow);
            bool movedRight = !CollisionStateActive(CollisionState.OnWallRight) && !blockInput && DirectionalMovementUpdate(Direction.Right, KeyCode.D, KeyCode.RightArrow);

            bool moveUpdate = movedLeft ^ movedRight; // xor
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

            const float MAX_MOVE_DIR = 1;
            
            if (moveDirTime > MAX_MOVE_DIR) moveDirTime = MAX_MOVE_DIR;
            if (moveDirTime < -MAX_MOVE_DIR) moveDirTime = -MAX_MOVE_DIR;
            
            int sign = moveDirTime < 0 ? -1 : 1;
            float wishdir = MovementStats.accelationModifier*moveDirTime * sign;

            float bonusSpeed = RobotUpgradeLoadOut?.SelfLoadOuts?.GetCurrent()?.GetCountinuousValue((int)RobotUpgrade.Speed) ?? 0;
            float speed = MovementStats.speed + bonusSpeed;
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
            if (jumpEvent != null)
            {
                if (collisionStates.Contains(CollisionState.HeadContact))
                {
                    rb.gravityScale = defaultGravityScale;
                    jumpEvent = null;
                } else if (Input.GetKey(KeyCode.Space))
                {
                    rb.gravityScale = jumpEvent.GetGravityModifier(JumpStats.initialGravityPercent,JumpStats.maxGravityTime) * defaultGravityScale;
                    jumpEvent.IterateTime();
                }
                else
                {
                    rb.gravityScale = defaultGravityScale;   
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                jumpEvent = null;
            }
            rb.velocity = velocity;
        }

        private void SpaceBarMovementUpdate(ref Vector2 velocity)
        {
            if (PlayerKeyPressUtils.BlockKeyInput)
            {
                rb.gravityScale = defaultGravityScale;   
                jumpEvent = null;

                return;
            }
            if (CollisionStateActive(CollisionState.OnPlatform) && Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.S))
            {
                ignorePlatformFrames = 3;
            }
            
            if (ignorePlatformFrames <= 0 && (CanJump() || coyoteFrames > 0) && Input.GetKeyDown(KeyCode.Space))
            {
                float bonusJumpHeight = RobotUpgradeUtils.GetContinuousValue(RobotUpgradeLoadOut, (int)RobotUpgrade.JumpHeight); 
                velocity.y = JumpStats.jumpVelocity+bonusJumpHeight;
                coyoteFrames = 0;
                liveYUpdates = 3;
                bonusJumps--;
                fallTime = 0;
                jumpEvent = new JumpEvent();
            }
        }
        
        private float GetFriction()
        {
            if (currentTileMovementType == TileMovementType.Slippery || slipperyFrames > 0) return MovementStats.iceFriction;

            return IsOnGround() ? MovementStats.friction : MovementStats.airFriction;
        }

        private bool DirectionalMovementUpdate(Direction direction, KeyCode firstKeycode, KeyCode secondKeyCode)
        {
            if (!Input.GetKey(firstKeycode) && !Input.GetKey(secondKeyCode)) return false;
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
            IChunkSystem chunkSystem = DimensionManager.Instance.GetPlayerSystem();
            Vector2Int cellPosition = Global.getCellPositionFromWorld(adjacentTilePosition);
            var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(cellPosition);
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
           
            if (climbing) {
                HandleClimbing();
                RemoveCollisionState(CollisionState.OnGround);
                return;
            }

            platformCollider.enabled = ignorePlatformFrames < 0 && rb.velocity.y < 0.05;

            if (currentRobot is IEnergyRechargeRobot energyRechargeRobot) EnergyRechargeUpdate(energyRechargeRobot);

            if (IsGrounded())
            {
                coyoteFrames = JumpStats.coyoteFrames;
                if (bonusJumps <= 0)
                {
                    bonusJumps = RobotUpgradeLoadOut?.SelfLoadOuts?.GetCurrent()?.GetDiscreteValue((int)RobotUpgrade.BonusJump) ?? 0;
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
            
            if (CollisionStateActive(CollisionState.OnSlope) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
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
            if (robotData.Energy >= currentRobot.MaxEnergy) return;
            
            robotData.Energy += energyRechargeRobot.EnergyRechargeRate;
            if (robotData.Energy > currentRobot.MaxEnergy) robotData.Energy = currentRobot.MaxEnergy;
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
            if (!IsOnGround())
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

        private void FlightMovementUpdate(Transform playerTransform)
        {
            Vector3 position = playerTransform.position;
            float movementSpeed = DevMode.Instance.FlightSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                position.x -= movementSpeed;
                spriteRenderer.flipX = true;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                position.x += movementSpeed;
                spriteRenderer.flipX = false;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                position.y += movementSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                position.y -= movementSpeed;
            }
            playerTransform.position = position;
        }

        public void Heal(float amount)
        {
            robotData.Health += amount;
            if (robotData.Health > currentRobot.BaseHealth) robotData.Health = currentRobot.BaseHealth;
        }

        public void Damage(float amount)
        {
            if (DevMode.Instance.noHit || iFrames > 0) return;
            
            robotData.Health -= amount;
            if (robotData.Health > 0 || dead) return;
            
            Die();
        }

        public void Respawn()
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            robotData.Health = currentRobot.BaseHealth;
            PlayerScript playerScript = GetComponent<PlayerScript>();
            
            DimensionManager.Instance.SetPlayerSystem(playerScript,0,new Vector2Int(0,0));
            dead = false;
        }

        public void Die()
        {
            robotData.Health = 0;
            dead = true;
            PlayerPickUp playerPickup = GetPlayerPick();
            playerPickup.CanPickUp = false;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            PlayerDeathScreenUI playerDeathScreenUI = Instantiate(deathScreenUIPrefab);
            PlayerScript playerScript = GetComponent<PlayerScript>();
            playerDeathScreenUI.Initialize(playerScript);
            CanvasController.Instance.DisplayOnParentCanvas(playerDeathScreenUI.gameObject);
            playerScript.PlayerInventory.DropAll();
            
        }
        private void CanStartClimbing() {
            if (rb.bodyType == RigidbodyType2D.Static) {
                return;
            }
            bool climbKeyInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
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
            IChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            Vector2Int cellPosition = Global.getCellPositionFromWorld(tileCenter);
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            
            return partition.GetTileItem(positionInPartition, TileMapLayer.Base);
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
            bool exitKeyCode = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space);
            if (climableTileEntity == null || exitKeyCode)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                climbing = false;
                rb.gravityScale = defaultGravityScale;
                return;
            }
            Vector2 velocity = rb.velocity;
            fallTime = 0;
            if (Input.GetKey(KeyCode.W)) {
                velocity.y = climableTileEntity.GetSpeed();
            } else if (Input.GetKey(KeyCode.S)) {
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
            if (tags?.Dict == null)
            {
                tags = new ItemTagCollection(new Dictionary<ItemTag, object>());
            }
            if (!tags.Dict.ContainsKey(ItemTag.RobotData) || tags.Dict[ItemTag.RobotData] is not RobotItemData)
            {
                Dictionary<ItemTag, object> tagData = new Dictionary<ItemTag, object>();
                ItemRobotToolData robotToolData = new ItemRobotToolData(new List<RobotToolType>(), new List<RobotToolData>(), new List<RobotUpgradeData>());
                // TODO robotupgrade data
                RobotItemData newItemData = new RobotItemData(robotToolData,null, currentRobot.BaseHealth,0);
                tagData[ItemTag.RobotData] = newItemData;
                ItemTagCollection itemTagCollection = new ItemTagCollection(tagData);
                robotItemSlot.tags = itemTagCollection;

                robotItemSlot.tags = itemTagCollection;
            }
            
            InitializeTools();
            
            
            SetFlightProperties();
            spriteRenderer.sprite = currentRobot.defaultSprite; 
        }

        public void SetRobotLoadOut(RobotUpgradeLoadOut loadOut)
        {
            this.RobotUpgradeLoadOut = loadOut;
        }
        private void InitializeTools()
        {
            robotData = (RobotItemData)robotItemSlot.tags.Dict[ItemTag.RobotData];
            ItemRobotToolData itemRobotToolData = robotData.ToolData;
            currentRobotToolObjects = RobotToolFactory.GetDictFromCollection(currentRobot.ToolCollection);
            RobotTools = new List<IRobotToolInstance>();
            for (int i = 0; i < itemRobotToolData.Tools.Count; i++)
            {
                var type = itemRobotToolData.Types[i];
                var data = itemRobotToolData.Tools[i];
                if (!currentRobotToolObjects.TryGetValue(type, out var toolObject)) continue;
                
                RobotTools.Add(RobotToolFactory.GetInstance(type,toolObject,data));
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

}
