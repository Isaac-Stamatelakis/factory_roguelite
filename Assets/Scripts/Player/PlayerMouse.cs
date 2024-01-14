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
    private DevMode devMode;
    private GameObject grabbedItem;

    // Start is called before the first frame update
    void Start()
    {
        devMode = GetComponent<DevMode>();
        playerInventory = GetComponent<PlayerInventory>();
        grabbedItem = GameObject.Find("GrabbedItem");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        handleLeftClick(mousePosition);
        if (devMode.spawnItem) {
            if (Input.GetMouseButton(0)) {
                ItemEntityHelper.spawnItemEntity(mousePosition,(int) devMode.spawnItemID,1,
                Global.findChild(ChunkHelper.snapChunk(mousePosition.x,mousePosition.y).transform,"TileEntities").transform);
            }
        } else {
            breakMouseHover(mousePosition);
        }
        handleInventoryControls();
        if (Input.GetMouseButton(0)) {
            handleInventoryClick(mousePosition);
        }
        
        
        
    }

    private void handleLeftClick(Vector2 mousePosition) {
        bool tileEntityClicked = false;
        if (Input.GetMouseButtonDown(1)) {
            tileEntityClicked = handleTileEntityClick(mousePosition);
        }
        if (Input.GetMouseButton(1)) {
            if (!tileEntityClicked) {
                handlePlace(mousePosition);
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
        if (devMode.breakType == 0) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("TileBlock"));
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("TileObject"));
        } else if (devMode.breakType == 1) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("TileBackground"));
        } else if (devMode.breakType == 2) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("EnergyConduit"));
        } else if (devMode.breakType == 3) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("ItemConduit"));
        } else if (devMode.breakType == 4) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("FluidConduit"));
        } else if (devMode.breakType == 5) {
            raycastHitBlock(mousePosition,1 << LayerMask.NameToLayer("SignalConduit"));
        }
        
    }

    private void raycastHitBlock(Vector2 position, int layer) {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
        if (hit.collider != null) {
            GameObject container = hit.collider.gameObject;
            AbstractTileMap[] mapArray = container.GetComponents<AbstractTileMap>();
            if (mapArray == null || mapArray.Length == 0) {
                return;
            }
            
            AbstractTileMap abstractTileMap = mapArray[0];
            if (devMode.instantBreak) {
                abstractTileMap.deleteTile(position);
            } else {
                abstractTileMap.hitTile(position);
            }
            
        }
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
                Vector2Int tilePosition = FindTileAtLocation.find(Global.Vector3IntToVector2Int(tileGridMap.mTileMap.WorldToCell(mousePosition)),tileGridMap.mTileMap);
                TileData tileData = (TileData) tileGridMap.getIdDataInChunk(tilePosition);
                if (tileData != null) {
                    GameObject chunk = ChunkHelper.snapChunk(tilePosition.x/2f,tilePosition.y/2f);
                    GameObject tileEntityContainer = Global.findChild(chunk.transform, "TileEntities");
                    GameObject tileEntity = TileEntityHelper.getTileEntity(
                        tileEntityContainer.transform,
                        tilemap.name,
                        new Vector2Int(Global.modInt(tilePosition.x,16),Global.modInt(tilePosition.y,16))
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

    private bool handlePlace(Vector2 mousePosition) {
        int id;
        if (devMode.placeSelectedID) {
            id = (int)devMode.placeID;
        } else {
            id = playerInventory.getSelectedTileId();
        }
        if (id < 0) {
            return false;
        }
        bool placed = false;
        IdData idData = IdDataMap.getInstance().GetIdData(id);
        placed = PlaceTile.Place(idData,mousePosition);
        
        if (placed && !devMode.noPlaceCost) {
            playerInventory.deiterateInventoryAmount();
        }
        return placed;
    }

    
    private bool handleInventoryClick(Vector2 mousePosition) {
        if (!EventSystem.current.IsPointerOverGameObject()) {
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
