using System.Collections;
using System.Collections.Generic;
using Chunks.Systems;
using Dimensions;
using Item.Slot;
using Items;
using Items.Tags;
using Player.Tool;
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
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerModule.PlayerPlatformDetector playerFeet;
        private PolygonCollider2D polygonCollider;
        private int noCollisionWithPlatformCounter;
        private bool onGround;
        public bool OnGround { get => onGround; set => onGround = value; }
        public int NoCollisionWithPlatformCounter { get => noCollisionWithPlatformCounter; set => noCollisionWithPlatformCounter = value; }
        private bool climbing;
        [SerializeField] public ItemSlot robotItemSlot;
        private RobotObject currentRobot;
        private RobotItemData robotItemData;
        public List<IRobotToolInstance> RobotTools;
        private Dictionary<RobotToolType, RobotToolObject> currentRobotToolObjects;
        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void FixedUpdate() {
            canStartClimbing();
            float playerWidth = spriteRenderer.sprite.bounds.extents.x;
            if (climbing) {
                handleClimbing();
                return;
            }
            Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y);
            int layers = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform") | 1 << LayerMask.NameToLayer("SlipperyBlock"));
            RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,0.1f),0,Vector2.zero,Mathf.Infinity,layers);
            onGround = raycastHit.collider != null;            
            if (!CanvasController.Instance.IsActive) {
                currentRobot.handleMovement(transform);
            }
        }
        
        private void canStartClimbing() {
            if (rb.bodyType == RigidbodyType2D.Static) {
                return;
            }
            bool climbKeyInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
            if (climbing || !climbKeyInput) {
                return;
            }
            if (getClimbable() == null) {
                return;
            }
            climbing = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0;
            Vector3 position = transform.position;
            float x = position.x;
            x = Mathf.Floor(x*2)/2f+0.25f;
            position.x = x;
            transform.position = position;
        }
        private IClimableTileEntity getClimbable() {
            int objectLayer = (1 << LayerMask.NameToLayer("Object"));
            RaycastHit2D objHit = Physics2D.BoxCast(transform.position,new Vector2(0.5f,0.1f),0,Vector2.zero,Mathf.Infinity,objectLayer);
            if (objHit.collider == null) {
                return null;
            }
            TileGridMap tileGridMap = objHit.collider.GetComponent<TileGridMap>();
            if (tileGridMap == null) {
                return null;
            }
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(transform);
            TileItem tileItem = tileGridMap.getTileItem(Global.getCellPositionFromWorld(transform.position)+closedChunkSystem.DimPositionOffset);
            if (tileItem == null || tileItem.tileEntity == null) {
                return null;
            }
            if (tileItem.tileEntity is not IClimableTileEntity climableTileEntity) {
                return null;
            }
            return climableTileEntity;
        }

        private void handleClimbing() {
            IClimableTileEntity climableTileEntity = getClimbable();
            bool exitKeyCode = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space);
            if (climableTileEntity == null || exitKeyCode) {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                climbing = false;
                rb.gravityScale = 3;
                return;
            }
            Vector2 velocity = rb.velocity;
            rb.velocity = velocity;
            if (Input.GetKey(KeyCode.W)) {
                velocity.y = climableTileEntity.getSpeed();
            } else if (Input.GetKey(KeyCode.S)) {
                velocity.y = -climableTileEntity.getSpeed();
            } else {
                velocity.y = 0;
            }
            rb.velocity = velocity;
        }
        
        public void setRobot(ItemSlot robotItemSlot)
        {
            
            if (ItemSlotUtils.IsItemSlotNull(this.robotItemSlot) || robotItemSlot.itemObject is not RobotItem robotItem) {
                Debug.LogWarning("Tried to set invalid robot");
                robotItemSlot = RobotDataFactory.GetDefaultRobot(false);
                robotItem = robotItemSlot.itemObject as RobotItem;
            }
            this.currentRobot = robotItem.robot;
            this.robotItemSlot = robotItemSlot;
            InitializeTools(robotItemSlot);
            
            
            currentRobot.init(gameObject);
            spriteRenderer.sprite = currentRobot.defaultSprite; 
        }

        private void InitializeTools(ItemSlot robotItemSlot)
        {
            ItemTagCollection tags = this.robotItemSlot.tags;
            if (!tags.Dict.ContainsKey(ItemTag.RobotData))
            {
                Debug.LogWarning("Tried to initialize robot with invalid tool data");
            }

            if (!tags.Dict.ContainsKey(ItemTag.RobotData))
            {
                Debug.LogWarning("Tried to initialize robot with invalid tool data");
                
            }
            this.robotItemData = tags.Dict[ItemTag.RobotData] as RobotItemData;
            ItemRobotToolData itemRobotToolData = robotItemData.ToolData;
            currentRobotToolObjects = RobotToolFactory.GetDictFromCollection(currentRobot.ToolCollection);
            RobotTools = new List<IRobotToolInstance>();
            for (int i = 0; i < itemRobotToolData.Tools.Count; i++)
            {
                var type = itemRobotToolData.Types[i];
                var data = itemRobotToolData.Tools[i];
                RobotTools.Add(RobotToolFactory.GetInstance(type,currentRobotToolObjects[type],data));
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
