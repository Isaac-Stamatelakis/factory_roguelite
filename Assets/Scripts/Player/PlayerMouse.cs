using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ChunkModule;
using ChunkModule.ClosedChunkSystemModule;
using TileMapModule.Layer;
using TileMapModule;
using TileMapModule.Place;
using ChunkModule.PartitionModule;
using TileMapModule.Type;
using ConduitModule.ConduitSystemModule;
using ConduitModule.Ports;
using GUIModule;
using UnityEngine.Tilemaps;
using ConduitModule;
using DimensionModule;

namespace PlayerModule.Mouse {
    /// <summary>
    /// Handles all player mouse interactions
    /// </summary>
    public class PlayerMouse : MonoBehaviour
    {
        private int interactionableLayer;
        private bool isHoldingMouse = false;
        private float updateInterval = 0.25f;
        private float timeSinceLastUpdate = 0f;
        private GameObject testObject;
        private PlayerInventory playerInventory;
        private DevMode devMode;
        private GameObject grabbedItem;
        private LayerMask UILayer;
        private EventSystem eventSystem;

        // Start is called before the first frame update
        void Start()
        {
            devMode = GetComponent<DevMode>();
            playerInventory = GetComponent<PlayerInventory>();
            grabbedItem = GameObject.Find("GrabbedItem");
            UILayer = 1 << LayerMask.NameToLayer("UI");
            eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        }

        // Update is called once per frame
        void Update()
        {   
            handleInventoryControls();
            if (eventSystem.IsPointerOverGameObject()) {
            return;
            }
            if (Input.GetMouseButton(0)) {
                handleRightClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            if (Input.GetMouseButton(1)) {
                handleLeftClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            
        }

        private IChunk getChunk(Vector2 mousePosition) {
            ClosedChunkSystem closedChunkSystem = GetClosedChunkSystem(mousePosition);
            if (closedChunkSystem == null) {
                return null;
            }
            Vector2Int chunkPosition = Global.getChunkFromWorld(mousePosition);
            return closedChunkSystem.getChunk(chunkPosition);
        }

        private ClosedChunkSystem GetClosedChunkSystem(Vector2 mousePosition) {
            DimensionManager dimensionManager = DimensionManagerContainer.getInstance().getManager();
            return dimensionManager.CurrentDimension.getPlayerSystem();
        }

        private void handleRightClick(Vector2 mousePosition) {
            handleDrop(mousePosition);
            if (devMode.spawnItem) {
                IChunk chunk = getChunk(mousePosition);
                if (chunk != null) {
                        ItemEntityHelper.spawnItemEntity(
                        mousePosition,
                        new ItemSlot(itemObject:ItemRegistry.getInstance().getItemObject(devMode.spawnItemID),1,new Dictionary<string, object>()),
                        chunk.getEntityContainer()
                    );
                }
                return;
            }
            breakMouseHover(mousePosition);
        }
        private void handleLeftClick(Vector2 mousePosition) {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(playerInventory.getSelectedId());

            bool somethingClicked = false;
            if (Input.GetMouseButtonDown(1)) {
                if (itemObject is ConduitItem) {
                    somethingClicked = handlePortClick(mousePosition);
                } else {
                    somethingClicked = handleTileEntityClick(mousePosition);
                }
            }

            if (!somethingClicked) {
                handlePlace(mousePosition,GetClosedChunkSystem(mousePosition));
            }
        }

        private bool handlePortClick(Vector2 mousePosition) {
            ClosedChunkSystem closedChunkSystem = GetClosedChunkSystem(mousePosition);
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }
            ConduitType conduitType = devMode.breakType.toConduit();
            ConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.getManager(conduitType);
            if (conduitSystemManager == null) {
                Debug.LogError("Attempted to click port of null conduit system manager");
                return false;
            }
            IConduit conduit = conduitSystemManager.getConduitWithPort(Global.getCellPositionFromWorld(mousePosition));
            if (conduit == null) {
                return false;
            }
            GameObject ui = ConduitPortUIFactory.getUI(conduit,conduitType);
            GlobalUIContainer.getInstance().getUiController().setGUI(ui);
            return true;
        }
        private void breakMouseHover(Vector2 mousePosition) {
            if (Input.GetMouseButtonDown(0)) {
                isHoldingMouse = true;
            }

            if (Input.GetMouseButton(0) && isHoldingMouse) {
                if (devMode.noBreakCooldown) {
                    handleBreak(mousePosition);
                    return;
                }
                timeSinceLastUpdate += Time.deltaTime;
                if (timeSinceLastUpdate >= updateInterval) {
                    timeSinceLastUpdate = 0f;
                    handleBreak(mousePosition);
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                isHoldingMouse = false;
            }
        }
        
        public void handleBreak(Vector2 mousePosition) {
            TileMapLayer tileMapLayer = devMode.breakType;
            if (tileMapLayer.raycastable()) {
                int layer = tileMapLayer.toRaycastLayers();
                raycastHitBlock(mousePosition,layer);  
            } else {
                foreach (TileMapType tileMapType in tileMapLayer.getTileMapTypes()) {
                    ITileMap tileMap = GetClosedChunkSystem(mousePosition).getTileMap(tileMapType);
                    if (tileMap is IHitableTileMap) {
                        IHitableTileMap hitableTileMap = ((IHitableTileMap) tileMap);
                        if (devMode.instantBreak) {
                            hitableTileMap.deleteTile(mousePosition);
                        } else {
                            hitableTileMap.hitTile(mousePosition);
                        }
                        
                    }
                }
            }
            
        }


        private bool raycastHitBlock(Vector2 position, int layer) {
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
            if (hit.collider != null) {
                GameObject container = hit.collider.gameObject;
                IHitableTileMap hitableTileMap = container.GetComponent<IHitableTileMap>();
                if (devMode.instantBreak) {
                    hitableTileMap.deleteTile(position);
                } else {
                    hitableTileMap.hitTile(position);
                }
                return true;            
            }
            return false;
        }

        private GameObject raycastTileMap(Vector2 position, int layer) {
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
            if (hit.collider == null) {
                return null;
            }
            return hit.collider.gameObject;
        }

        private bool handleTileEntityClick(Vector2 mousePosition) {
            int layers = TileMapLayer.Base.toRaycastLayers();
            GameObject tilemapObject = raycastTileMap(mousePosition,layers);
            if (tilemapObject != null) {
                Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
                
                Vector2Int mouseCellPosition = new Vector2Int(Mathf.FloorToInt(mousePosition.x*2), Mathf.FloorToInt(mousePosition.y*2));
                Vector2Int tilePosition = FindTileAtLocation.find(mouseCellPosition,tilemap);
                Vector2 worldPositionTile = new Vector2(tilePosition.x/2f,tilePosition.y/2f);
                IChunk chunk = getChunk(worldPositionTile);
                Vector2Int partitionPosition = Global.getPartitionFromWorld(worldPositionTile);
                Vector2Int partitionPositionInChunk = partitionPosition -chunk.getPosition()*Global.PartitionsPerChunk;
                Vector2Int tilePositionInPartition = tilePosition-partitionPosition*Global.ChunkPartitionSize;
                IChunkPartition chunkPartition = chunk.getPartition(partitionPositionInChunk);
                if (chunkPartition.clickTileEntity(tilePositionInPartition)) {
                    return true;
                }
            }
            
            return false;
            
        }

        private bool handlePlace(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem) {
            if (closedChunkSystem == null) {
                return false;
            }
            string id;
            if (devMode.placeSelectedID) {
                id = devMode.placeID;
            } else {
                id = playerInventory.getSelectedId();
            }
            if (id == null) {
                return false;
            }
        
            bool placed = false;
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(id);
            placed = PlaceTile.Place(itemObject,mousePosition,closedChunkSystem);
            
            if (placed && !devMode.noPlaceCost) {
                playerInventory.deiterateInventoryAmount();
            }
            return placed;
        }

        
        private bool handleDrop(Vector2 mousePosition) {
            if (eventSystem.IsPointerOverGameObject()) {
                return false;
            }
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            if (grabbedItemProperties.itemSlot == null) {
                return false;
            }
            IChunk chunk = getChunk(mousePosition);
            if (chunk == null) {
                return false;
            }
            Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
            if (
                grabbedItemProperties.itemSlot != null && 
                grabbedItemProperties.itemSlot.itemObject != null && 
                grabbedItemProperties.itemSlot.itemObject.id != null
            ) {
                ItemEntityHelper.spawnItemEntityWithVelocity(
                    new Vector2(transform.position.x,transform.position.y) + spriteCenter,
                    grabbedItemProperties.itemSlot,
                    chunk.getEntityContainer(),
                    calculateItemVelocity(mousePosition)
                );
            }
            grabbedItemProperties.itemSlot = null;
            grabbedItemProperties.updateSprite();
            return true;
        }
        

        private void handleInventoryControls() {
            if (Input.mouseScrollDelta.y != 0) {
                float y = Input.mouseScrollDelta.y;
                if (y < 0) {
                    playerInventory.iterateSelectedTile(1);
                } else {
                    playerInventory.iterateSelectedTile(-1);
                }
            }
        }

        private Vector2 calculateItemVelocity(Vector3 mouseposition) {
            float maxVelocity=5f;
            float distanceMultiplierX = 1/2f;
            float distanceMultiplierY = 2f;
            float xDif = mouseposition.x-transform.position.x;
            float yDif = mouseposition.y-transform.position.y;
            
            if (xDif < 0) {
                xDif = Mathf.Max(xDif*distanceMultiplierX,-maxVelocity);
            } else {
                xDif = Mathf.Min(xDif*distanceMultiplierX,maxVelocity);
            }
            if (yDif < 0) {
                yDif = Mathf.Max(yDif*distanceMultiplierY,-maxVelocity);
            } else {
                yDif = Mathf.Min(yDif*distanceMultiplierY,maxVelocity);
            }
            
            return new Vector2(xDif, yDif);
        }
    }
}

