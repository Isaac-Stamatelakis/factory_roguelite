using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


/**
Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
**/
public abstract class AbstractTileMap<Item,PlacedItem> : MonoBehaviour where Item: ItemObject where PlacedItem : PlacedItemObject<Item>
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
        tilemapRenderer.material = Resources.Load<Material>("Material/ShadedMaterial");
        tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
        
        CompositeCollider2D compositeCollider2D = gameObject.AddComponent<CompositeCollider2D>();
        compositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;

        Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType=RigidbodyType2D.Static;
        tilemapCollider.usedByComposite=true;
        
    }
    public virtual void Start() {
        devMode = GameObject.Find("Player").GetComponent<DevMode>();
    }
    
    public IEnumerator load(ChunkData<PlacedItem> chunkData, Vector2Int chunkPosition) {
        dimensionChunkData[chunkPosition] = chunkData;
        Coroutine b = StartCoroutine(slowPlaceTiles(chunkPosition));
        yield return b;
        
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

    protected virtual PlacedItem initTileData(Item itemObject) {
        /*
        if (id > 0) {
            return IdDataMap.getInstance().GetIdData(id);
        }
        */
        return null;
    }
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
    protected IEnumerator slowPlaceTiles(Vector2Int chunkPosition) {
        ChunkData<PlacedItem> chunkData = dimensionChunkData[chunkPosition];
        for (int xIter=0; xIter < Global.ChunkSize; xIter ++) {
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                setTile(xIter + Global.ChunkSize*chunkPosition.x,yIter + Global.ChunkSize*chunkPosition.y,chunkData.data[xIter][yIter]);
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    protected Vector2Int getChunk(Vector2Int tileMapPosition) {
        return new Vector2Int(Mathf.FloorToInt(tileMapPosition.x/16f),Mathf.FloorToInt(tileMapPosition.y/16f));
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
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<string> idList = new List<string>();
            for (int yIter = 0; yIter < 16; yIter ++) {
                
                if (chunkData.data[xIter][yIter] == null) {
                    idList.Add(null);
                } else {
                    idList.Add(chunkData.data[xIter][yIter].itemObject.id);
                }
                
            }
            nestedIds.Add(idList);
        }
        return nestedIds;
    }
}


















