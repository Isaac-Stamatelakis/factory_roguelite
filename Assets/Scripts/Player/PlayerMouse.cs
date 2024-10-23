using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Chunks;
using Chunks.Systems;
using TileMaps.Layer;
using TileMaps;
using TileMaps.Place;
using Chunks.Partitions;
using TileMaps.Type;
using Conduits.Systems;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Conduits;
using Dimensions;
using Items;
using TileEntityModule;
using Entities;
using PlayerModule.IO;

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
        private Transform playerTransform;

        // Start is called before the first frame update
        void Start()
        {
            devMode = GetComponent<DevMode>();
            playerInventory = GetComponent<PlayerInventory>();
            grabbedItem = GameObject.Find("GrabbedItem");
            UILayer = 1 << LayerMask.NameToLayer("UI");
            eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
            playerTransform = GameObject.Find("Player").transform;
        }

        // Update is called once per frame
        void Update()
        {   
            handleInventoryControls();
            if (eventSystem.IsPointerOverGameObject()) {
            return;
            }
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            Vector2 systemOffset = new Vector2(closedChunkSystem.DimPositionOffset.x/2f,closedChunkSystem.DimPositionOffset.y/2f);
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButton(0)) {
                handleLeftClick(mousePosition,systemOffset);
            }
            if (Input.GetMouseButton(1)) {
                handleRightClick(mousePosition,systemOffset);
            }
            
        }

        private ILoadedChunk getChunk(Vector2 mousePosition) {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (closedChunkSystem == null) {
                return null;
            }
            Vector2Int chunkPosition = Global.getChunkFromWorld(mousePosition);
            return closedChunkSystem.getChunk(chunkPosition);
        }

        private void handleLeftClick(Vector2 mousePosition, Vector2 offset) {
            if (devMode.spawnItem) {
                ILoadedChunk chunk = getChunk(mousePosition+offset);
                if (chunk != null) {
                        ItemEntityHelper.spawnItemEntity(
                        mousePosition+offset,
                        ItemSlotFactory.createNewItemSlot(
                            ItemRegistry.getInstance().getItemObject(devMode.spawnItemID),
                            1
                        ),
                        chunk.getEntityContainer()
                    );
                }
                return;
            }
            bool drop = handleDrop(mousePosition,offset);
            if (drop) {
                return;
            }
            breakMouseHover(mousePosition);
        }
        private void handleRightClick(Vector2 mousePosition,Vector2 offset) {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(playerInventory.getSelectedId());

            bool somethingClicked = false;
            if (Input.GetMouseButtonDown(1)) {
                if (itemObject is ConduitItem) {
                    somethingClicked = handlePortClick(mousePosition+offset);
                } else {
                    somethingClicked = handleTileEntityClick(mousePosition,offset);
                }
            }
            if (!somethingClicked) {
                handlePlace(mousePosition,offset,DimensionManager.Instance.getPlayerSystem(playerTransform));
            }
        }

        private bool handlePortClick(Vector2 mousePosition) {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }
            
            string id = playerInventory.getSelectedId();
            ConduitItem conduitItem = ItemRegistry.getInstance().GetConduitItem(id);
            if (conduitItem == null) {
                return false;
            }
            ConduitType conduitType = conduitItem.getConduitType();
            IConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.getManager(conduitType);
            if (conduitSystemManager == null) {
                Debug.LogError("Attempted to click port of null conduit system manager");
                return false;
            }
            if (conduitSystemManager is PortConduitSystemManager portConduitSystemManager) {
                Vector2Int cellPosition = Global.getCellPositionFromWorld(mousePosition);
                IConduit conduit = portConduitSystemManager.getConduitWithPort(cellPosition);
                if (conduit == null) {
                    return false;
                }
                if (conduit is not IPortConduit portConduit) {
                    return false;
                }
                EntityPortType portType = portConduitSystemManager.getPortTypeAtPosition(cellPosition.x,cellPosition.y);
                GameObject ui = ConduitPortUIFactory.getUI(portConduit,conduitType,portType);
                GlobalUIContainer.getInstance().getUiController().setGUI(ui);
                return true;
            }
            return false;
            
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
                    ITileMap tileMap = DimensionManager.Instance.getPlayerSystem(playerTransform).getTileMap(tileMapType);
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
                if (hitableTileMap == null) {
                    return false;
                }
                if (hitableTileMap is TileGridMap tileGridMap) {
                    Vector2Int cellPosition = Global.getCellPositionFromWorld(position);
                    ITileEntityInstance tileEntity = tileGridMap.getTileEntityAtPosition(cellPosition);
                    if (tileEntity is ILeftClickableTileEntity leftClickableTileEntity) {
                        leftClickableTileEntity.onLeftClick();
                        if (!leftClickableTileEntity.canBreak()) {
                            return false;
                        }
                    }
                }
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

        private bool handleTileEntityClick(Vector2 mousePosition,Vector2 offset) {
            int layers = TileMapLayer.Base.toRaycastLayers();
            GameObject tilemapObject = raycastTileMap(mousePosition,layers);
            if (tilemapObject != null) {
                Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
                Vector2Int mouseCellPosition = new Vector2Int(Mathf.FloorToInt((mousePosition.x+offset.x)*2), Mathf.FloorToInt((mousePosition.y+offset.y)*2));
                Vector2Int? tilePosition = FindTileAtLocation.find(mouseCellPosition,tilemap);
                if (tilePosition == null) {
                    return false;
                }
                Vector2Int nonNullPosition = (Vector2Int) tilePosition;
                Vector2 worldPositionTile = new Vector2(nonNullPosition.x/2f,nonNullPosition.y/2f);
                ILoadedChunk chunk = getChunk(worldPositionTile);
                if (chunk == null) {
                    return false;
                }
                Vector2Int partitionPosition = Global.getPartitionFromWorld(worldPositionTile);
                Vector2Int partitionPositionInChunk = partitionPosition -chunk.getPosition()*Global.PartitionsPerChunk;
                Vector2Int tilePositionInPartition = nonNullPosition-partitionPosition*Global.ChunkPartitionSize;
                IChunkPartition chunkPartition = chunk.getPartition(partitionPositionInChunk);
                if (chunkPartition.clickTileEntity(tilePositionInPartition)) {
                    return true;
                }
            }
            
            return false;
            
        }

        private bool handlePlace(Vector2 mousePosition, Vector2 offset, ClosedChunkSystem closedChunkSystem) {
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
            placed = PlaceTile.PlaceFromWorldPosition(itemObject,mousePosition,closedChunkSystem);
            if (placed && !devMode.noPlaceCost) {
                playerInventory.deiterateInventoryAmount();
            }
            return placed;
        }

        
        private bool handleDrop(Vector2 mousePosition,Vector2 offset) {
            if (eventSystem.IsPointerOverGameObject()) {
                return false;
            }
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            if (grabbedItemProperties.ItemSlot == null) {
                return false;
            }
            ILoadedChunk chunk = getChunk(mousePosition+offset);
            if (chunk == null) {
                return false;
            }
            Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
            if (
                grabbedItemProperties.ItemSlot != null && 
                grabbedItemProperties.ItemSlot.itemObject != null && 
                grabbedItemProperties.ItemSlot.itemObject.id != null
            ) {
                ItemEntityHelper.spawnItemEntityWithVelocity(
                    new Vector2(transform.position.x,transform.position.y) + spriteCenter,
                    grabbedItemProperties.ItemSlot,
                    chunk.getEntityContainer(),
                    calculateItemVelocity(mousePosition)
                );
            }
            grabbedItemProperties.setItemSlot(null);
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

