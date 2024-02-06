using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMouse : MonoBehaviour
{
    private int interactionableLayer;
    private bool isHoldingMouse = false;
    private float updateInterval = 0.25f;
    private float timeSinceLastUpdate = 0f;
    private GameObject testObject;
    private PlayerInventory playerInventory;
    private ClosedChunkSystem[] closedChunkSystems;
    private DevMode devMode;
    private GameObject grabbedItem;
    private LayerMask UILayer;

    // Start is called before the first frame update
    void Start()
    {
        devMode = GetComponent<DevMode>();
        playerInventory = GetComponent<PlayerInventory>();
        grabbedItem = GameObject.Find("GrabbedItem");
        UILayer = 1 << LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        handleLeftClick(mousePosition);
        if (devMode.spawnItem) {
            if (Input.GetMouseButton(0)) {
                
                IChunk chunk = getChunk(mousePosition);
                if (chunk != null) {
                        ItemEntityHelper.spawnItemEntity(
                        mousePosition,
                        new ItemSlot(itemObject:ItemRegistry.getInstance().getItemObject(devMode.spawnItemID),1,new Dictionary<ItemSlotOption, object>()),
                        chunk.getEntityContainer()
                    );
                }
                
            }
        } else {
            breakMouseHover(mousePosition);
        }
        handleInventoryControls();
        if (Input.GetMouseButton(0)) {
            handleInventoryClick(mousePosition);
        }
    }

    private IChunk getChunk(Vector2 mousePosition) {
        ClosedChunkSystem closedChunkSystem = GetClosedChunkSystem(mousePosition);
        if (closedChunkSystem == null) {
            return null;
        }
        Vector2Int chunkPosition = Global.getChunk(mousePosition);
        return closedChunkSystem.getChunk(chunkPosition);
    }

    private ClosedChunkSystem GetClosedChunkSystem(Vector2 mousePosition) {
        if (closedChunkSystems == null) {
            closedChunkSystems = GameObject.Find("DimController").GetComponentsInChildren<ClosedChunkSystem>();
        }
        Vector2Int chunkPosition = Global.getChunk(mousePosition);
        foreach (ClosedChunkSystem closedChunkSystem in closedChunkSystems) {
            if (closedChunkSystem.containsChunk(chunkPosition)) {
                return closedChunkSystem;
            }
        }
        return null;
    }
    private void handleLeftClick(Vector2 mousePosition) {
        bool tileEntityClicked = false;
        if (Input.GetMouseButtonDown(1)) {
            tileEntityClicked = handleTileEntityClick(mousePosition);
        }
        if (Input.GetMouseButton(1)) {
            if (!tileEntityClicked) {
                handlePlace(mousePosition,GetClosedChunkSystem(mousePosition));
            }
        }
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
        List<TileMapType> layerTypes = TileMapTypeFactory.getTileTypesInLayer(tileMapLayer);
        foreach (TileMapType tileMapType in layerTypes) {
            if (raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer(tileMapType.ToString()))) {
                break;
            }
            
        }
    }

    private bool raycastHitBlock(Vector2 position, int layer) {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
        if (hit.collider != null) {
            GameObject container = hit.collider.gameObject;
            HitableTileMap hitableTileMap = container.GetComponent<HitableTileMap>();
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
        GameObject tilemap = raycastTileMap(mousePosition,1 << LayerMask.NameToLayer("TileObject"));
        if (tilemap != null) {
            TileGridMap tileGridMap = tilemap.GetComponent<TileGridMap>();
            if (tileGridMap != null) {
                Vector3Int vect = tileGridMap.mTileMap.WorldToCell(mousePosition);
                Vector2Int tilePosition = FindTileAtLocation.find(new Vector2Int(vect.x,vect.y),tileGridMap.mTileMap);
                TileData tileData = (TileData) tileGridMap.getIdDataInChunk(tilePosition);
                if (tileData != null) {
                    IChunk chunk = getChunk(mousePosition);
                    GameObject tileEntity = TileEntityHelper.getTileEntity(
                        chunk.getTileEntityContainer(),
                        tilemap.name,
                        new UnityEngine.Vector2Int(Global.modInt(tilePosition.x,Global.ChunkSize), Global.modInt(tilePosition.y,Global.ChunkSize))
                    );
                    if (tileEntity != null) {
                        OnTileEntityClickController clickController = tileEntity.GetComponent<OnTileEntityClickController>();
                        if (clickController != null) {
                            clickController.activeClick();
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool handlePlace(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem) {
        if (closedChunkSystem == null) {
            return false;
        }
        if (EventSystem.current.IsPointerOverGameObject()) return false;
        string id;
        if (devMode.placeSelectedID) {
            id = devMode.placeID;
        } else {
            id = playerInventory.getSelectedTileId();
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

    
    private bool handleInventoryClick(Vector2 mousePosition) {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            if (grabbedItemProperties.itemSlot != null) {
                IChunk chunk = getChunk(mousePosition);
                if (chunk == null) {
                    return false;
                }
                Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
                if (grabbedItemProperties.itemSlot != null && grabbedItemProperties.itemSlot.itemObject != null && grabbedItemProperties.itemSlot.itemObject.id != null) {
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
        }
        return false;
        /*
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity,1 << LayerMask.NameToLayer("Inventory"));
        if (hit.collider != null) {
            GameObject hitInventory = hit.collider.gameObject;
            InventoryTileMap inventoryTileMap = hitInventory.transform.parent.GetComponent<InventoryTileMap>();
            inventoryTileMap.swapWithGrabbedItem(mousePosition, grabbedItem);
            return true;
        } else {
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            if (grabbedItemProperties.GrabbedItemData != null && grabbedItemProperties.GrabbedItemData.id > 0) {
                GameObject chunkGameObject = ChunkHelper.snapChunk(mousePosition.x,mousePosition.y);
                if (chunkGameObject == null) {
                    return false;
                }
                Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
                ItemEntityHelper.spawnItemEntityWithVelocity(
                    new Vector2(transform.position.x,transform.position.y) + spriteCenter,
                    grabbedItemProperties.GrabbedItemData.id,
                    grabbedItemProperties.GrabbedItemData.amount,
                    Global.findChild(chunkGameObject.transform, "Entities").transform,
                    calculateItemVelocity(mousePosition)
                );
                grabbedItemProperties.GrabbedItemData = null;
                grabbedItemProperties.updateSprite();
                return true;
            }
        }
        */
    }
    

    private void handleInventoryControls() {
        /*
        if (Input.GetMouseButtonDown(0)) {
            if (Input.GetKey(KeyCode.LeftControl)) {
                handleInventorySelect();
            }
        }
        */
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
