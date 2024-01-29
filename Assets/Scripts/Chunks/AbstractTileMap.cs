using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


public interface HitableTileMap {
    public void hitTile(Vector2 position);
    public void deleteTile(Vector2 position);
}
/**
Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
**/
public abstract class AbstractTileMap<Item,PlacedItem> : MonoBehaviour, HitableTileMap where Item: ItemObject where PlacedItem : PlacedItemObject<Item>
{
    protected Tilemap tilemap;
    public Tilemap mTileMap {get{return tilemap;}}
    protected TilemapRenderer tilemapRenderer;
    protected TilemapCollider2D tilemapCollider;
    protected Dictionary<Vector2Int, ChunkData<PlacedItem>> dimensionChunkData;
    protected DevMode devMode;


    public virtual void Awake() {
        tilemap = gameObject.AddComponent<Tilemap>();
        dimensionChunkData = new Dictionary<Vector2Int, ChunkData<PlacedItem>>();
        tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();

        
        tilemapRenderer.detectChunkCullingBounds = TilemapRenderer.DetectChunkCullingBounds.Manual;
        tilemapRenderer.chunkCullingBounds = new Vector3(16,16,0);


        tilemapRenderer.material = Resources.Load<Material>("Material/ShadedMaterial");
        tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.maximumTileChangeCount=100000000;
        
    }
    public virtual void Start() {
        devMode = GameObject.Find("Player").GetComponent<DevMode>();
    }
    
    public IEnumerator load(ChunkData<PlacedItem> chunkData, Vector2Int chunkPosition,int sectionAmount, double angle) {
        dimensionChunkData[chunkPosition] = chunkData;
        
        yield return StartCoroutine(slowPlaceTiles(chunkPosition,sectionAmount,angle));
        
    }

    public void placeTileAtLocation(int x, int y, Item itemObject) {
        addTile(itemObject, getChunkPosition(new Vector2Int(x,y)), getTilePosition(new Vector2Int(x,y)));
        setTile(x,y, getIdDataInChunk(new Vector2Int(x,y)));
        
    }
    public void hitTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        PlacedItem placedData = getIdDataInChunk(hitTilePosition);
        if (placedData == null) {
            return;
        }
        if (hitHardness(placedData)) {
            spawnItemEntity(placedData.itemObject,hitTilePosition,position);
            breakTile(hitTilePosition);
        }
    }

    public void deleteTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        breakTile(hitTilePosition);
        
    }
    protected void addTile(Item itemObject, Vector2Int chunkPosition, Vector2Int tilePosition) {
        dimensionChunkData[chunkPosition].data[tilePosition.x][tilePosition.y] = initTileData(itemObject);
    }
    public PlacedItem getIdDataInChunk(Vector2Int realTilePosition) {
        Vector2Int tilePosition = getTilePosition(realTilePosition);
        return (PlacedItem) dimensionChunkData[getChunkPosition(realTilePosition)].data[tilePosition.x][tilePosition.y];
    }
    protected virtual void setTile(int x, int y,PlacedItem placedItem) {
        
    }
    public bool containsChunk(Vector2Int chunkPosition) {
        return dimensionChunkData.ContainsKey(chunkPosition);
    }

    protected abstract PlacedItem initTileData(Item itemObject);
    protected Vector2Int getChunkPosition(Vector2Int position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/16f), Mathf.FloorToInt(position.y/16f));
    }
    protected Vector2Int getTilePosition(Vector2Int position) {
        return new Vector2Int(Global.modInt(position.x,Global.ChunkSize),Global.modInt(position.y,Global.ChunkSize));
    }

    protected virtual Vector2Int getHitTilePosition(Vector2 position) {
        return Global.Vector3IntToVector2Int(tilemap.WorldToCell(position));
    }
    protected virtual bool hitHardness(PlacedItem placedItem) {
        return false;
    }
    protected virtual void breakTile(Vector2Int position) {
        Vector2Int chunkPosition = getChunk(position);
        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        Vector2Int tilePositon = getTilePosition(position);
        dimensionChunkData[chunkPosition].data[tilePositon.x][tilePositon.y] = null;
    }
    protected virtual void spawnItemEntity(Item itemObject, Vector2Int hitTilePosition, Vector2 worldPosition) {
        GameObject chunk = ChunkHelper.snapChunk(hitTilePosition.x,hitTilePosition.y);
        Transform entityContainer = Global.findChild(chunk.transform, "Entities").transform;       

        float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
        float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
        ItemSlot itemSlot = new ItemSlot(itemObject: itemObject, amount: 1, nbt : new Dictionary<ItemSlotOption, object>());
        ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,entityContainer);
    }
    protected IEnumerator slowPlaceTiles(Vector2Int chunkPosition,int sectionAmount, double angle) {
        ChunkData<PlacedItem> chunkData = dimensionChunkData[chunkPosition];
        double deg = Mathf.Rad2Deg*angle;
        if (45 <= deg && deg < 135) { // moving up
            for (int xIter=0; xIter < Global.ChunkSize; xIter ++) {
                for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                    setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (xIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        } else if (135 <= deg && deg < 215) { // moving left
            for (int yIter=0; yIter < Global.ChunkSize; yIter ++) {
                for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
                    setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (yIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        } else if (225 <= deg && deg < 315) { // moving down
           for (int yIter=Global.ChunkSize-1; yIter >= 0; yIter --) {
                for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
                    setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (yIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }

        } else {
            for (int xIter=Global.ChunkSize-1; xIter >= 0; xIter --) {
                for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                    setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (xIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        
        yield return null;
    }
    protected void instantlyPlaceTiles(Vector2Int chunkPosition) {
        ChunkData<PlacedItem> chunkData = dimensionChunkData[chunkPosition];
        for (int xIter=0; xIter < Global.ChunkSize; xIter ++) {
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
            }
        }
    }

    protected Vector2Int getChunk(Vector2Int tileMapPosition) {
        return new Vector2Int(Mathf.FloorToInt(tileMapPosition.x/Global.ChunkSize),Mathf.FloorToInt(tileMapPosition.y/Global.ChunkSize));
    }

    public IEnumerator removeChunk(Vector2Int chunkPosition) {
        dimensionChunkData.Remove(chunkPosition);
        for (int xIter=0; xIter < Global.ChunkSize; xIter ++) {
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                
                tilemap.SetTile(new Vector3Int(xIter + Global.ChunkSize*chunkPosition.x,yIter+Global.ChunkSize*chunkPosition.y,0),null);
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void instantlyRemoveChunk(Vector2Int chunkPosition) {
        
        dimensionChunkData.Remove(chunkPosition);
        for (int xIter=0; xIter < Global.ChunkSize; xIter ++) {
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                tilemap.SetTile(new Vector3Int(xIter + Global.ChunkSize*chunkPosition.x,yIter+Global.ChunkSize*chunkPosition.y,0),null);
            }
        }
    }

    public List<List<string>> getTileIds(Vector2Int chunkPosition) {
        ChunkData<PlacedItem> chunkData = dimensionChunkData[chunkPosition];
        List<List<string>> nestedIds = new List<List<string>>();
        for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
            List<string> idList = new List<string>();
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                PlacedItem placedItem = chunkData.data[xIter][yIter];
                if (placedItem == null || placedItem.itemObject == null) {
                     idList.Add(null);
                } else {
                    idList.Add(placedItem.itemObject.id);
                }
                
            }
            nestedIds.Add(idList);
        }
        return nestedIds;
    }
}


















