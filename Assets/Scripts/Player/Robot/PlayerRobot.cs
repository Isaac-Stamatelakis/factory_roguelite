using System.Collections;
using System.Collections.Generic;
using Chunks;
using Chunks.Systems;
using Dimensions;
using Item.Slot;
using Items;
using Items.Tags;
using Player.Inventory;
using Player.Robot;
using Player.Tool;
using Player.UI;
using PlayerModule;
using PlayerModule.KeyPress;
using Robot;
using Robot.Tool;
using RobotModule;
using TileEntity;
using TileMaps;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
    public class PlayerRobot : MonoBehaviour
    {
        [SerializeField] private PlayerRobotUI mPlayerRobotUI;
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;

        [SerializeField] private BoxCollider2D feetBoxCollider;
        [SerializeField] private CapsuleCollider2D feetCapsuleCollider;
   
        [SerializeField] private PlayerDeathScreenUI deathScreenUIPrefab;
        private PolygonCollider2D polygonCollider;
        private int noCollisionWithPlatformCounter;
        private bool onGround;
        public bool OnGround { get => onGround; set => onGround = value; }
        public int NoCollisionWithPlatformCounter { get => noCollisionWithPlatformCounter; set => noCollisionWithPlatformCounter = value; }
        private bool climbing;
        [SerializeField] public ItemSlot robotItemSlot;
        private RobotObject currentRobot;
        private RobotItemData robotData;
        public List<IRobotToolInstance> RobotTools;
        private Dictionary<RobotToolType, RobotToolObject> currentRobotToolObjects;
        public List<RobotToolType> ToolTypes => robotData.ToolData.Types;
        private int groundLayers;
        private float fallTime;
        private float defaultGravityScale;
        private bool dead = false;
        private bool immuneToNextFall = false;
        private uint iFrames;
        public bool Dead => dead;

        private const float TERMINAL_VELOCITY = 30f;
        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            groundLayers = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform") | 1 << LayerMask.NameToLayer("SlipperyBlock"));
            defaultGravityScale = rb.gravityScale;
        }

        public void Update()
        {
            mPlayerRobotUI.Display(robotData,currentRobot);
        }

        public void FixedUpdate()
        {
            if (iFrames > 0) iFrames--;
            CanStartClimbing();
            float playerWidth = spriteRenderer.sprite.bounds.extents.x;
            if (climbing) {
                HandleClimbing();
                return;
            }

            if (currentRobot is IEnergyRechargeRobot energyRechargeRobot) EnergyRechargeUpdate(energyRechargeRobot);
            
            Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y);
            
            RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,0.1f),0,Vector2.zero,Mathf.Infinity,groundLayers);
            onGround = !ReferenceEquals(raycastHit.collider, null);
            
            bool directionalInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
            if (!DevMode.Instance.flight)
            {
                CalculateFallTime();
                ClampFallSpeed();
                rb.gravityScale = onGround && !directionalInput ? 0 : defaultGravityScale;
            }
            
            if (PlayerKeyPressUtils.BlockKeyInput) return;
            
            if (DevMode.Instance.flight)
            {
                FlightMovementUpdate(transform);
                return;
            }
            
            
            feetBoxCollider.enabled = !directionalInput;
            feetCapsuleCollider.enabled = onGround && directionalInput;
            
            currentRobot?.handleMovement(transform);
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

        public void SetIFrames(uint frames)
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
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(UnPausePlayer());
        }

        private IEnumerator UnPausePlayer()
        {
            yield return new WaitForSeconds(0.1f);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            PlayerPickUp playerPickup = GetComponentInChildren<PlayerPickUp>();
            playerPickup.CanPickUp = true;
        }

        private void CalculateFallTime()
        {
            if (!onGround)
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
            SpriteRenderer spriteRenderer = playerTransform.GetComponent<SpriteRenderer>();
            Vector3 position = playerTransform.position;
            float speed = DevMode.Instance.FlightSpeed;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                position.x -= speed;
                spriteRenderer.flipX = true;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                position.x += speed;
                spriteRenderer.flipX = false;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                position.y += speed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                position.y -= speed;
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
            
            DimensionManager.Instance.setPlayerSystem(playerScript.transform,0,new Vector2Int(0,0));
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
        private IClimableTileEntity GetClimbable() {
            int objectLayer = (1 << LayerMask.NameToLayer("Object"));
            RaycastHit2D objHit = Physics2D.BoxCast(transform.position,new Vector2(0.5f,0.1f),0,Vector2.zero,Mathf.Infinity,objectLayer);
            
            WorldTileGridMap worldTileGridMap = objHit.collider?.GetComponent<WorldTileGridMap>();
            if (ReferenceEquals(worldTileGridMap, null)) return null;
            
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(transform);
            TileItem tileItem = worldTileGridMap.getTileItem(Global.getCellPositionFromWorld(transform.position)+closedChunkSystem.DimPositionOffset);
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
                velocity.y = climableTileEntity.getSpeed();
            } else if (Input.GetKey(KeyCode.S)) {
                velocity.y = -climableTileEntity.getSpeed();
            } else {
                velocity.y = 0;
            }
            rb.velocity = velocity;
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
                ItemRobotToolData robotToolData = new ItemRobotToolData(new List<RobotToolType>(), new List<RobotToolData>());
                // TODO robotupgrade data
                RobotItemData newItemData = new RobotItemData(robotToolData,null, currentRobot.BaseHealth,0);
                tagData[ItemTag.RobotData] = newItemData;
                ItemTagCollection itemTagCollection = new ItemTagCollection(tagData);
                robotItemSlot.tags = itemTagCollection;

                robotItemSlot.tags = itemTagCollection;
            }
            
            InitializeTools();
            
            
            currentRobot.init(gameObject);
            SetFlightProperties();
            spriteRenderer.sprite = currentRobot.defaultSprite; 
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
        
        protected void rebuildCollider() {
            Sprite sprite = spriteRenderer.sprite;
            polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
            List<Vector2> path = new List<Vector2>();
                for (int i = 0; i < polygonCollider.pathCount; i++) {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                polygonCollider.SetPath(i, path.ToArray());
            }
        }
    }

}
