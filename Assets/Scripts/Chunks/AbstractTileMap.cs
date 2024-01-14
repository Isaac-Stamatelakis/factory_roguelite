using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


/**
Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
**/
public abstract class AbstractTileMap : MonoBehaviour
{
    protected Tilemap tilemap;
    public Tilemap mTileMap {get{return tilemap;}}
    protected TilemapRenderer tilemapRenderer;
    protected TilemapCollider2D tilemapCollider;
    protected Dictionary<Vector2Int, ChunkData> dimensionChunkData;
    protected DevMode devMode;


    public virtual void Awake() {
        tilemap = gameObject.AddComponent<Tilemap>();
        dimensionChunkData = new Dictionary<Vector2Int, ChunkData>();
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
    
    public IEnumerator load(ChunkData chunkData, Vector2Int chunkPosition) {
        dimensionChunkData[chunkPosition] = chunkData;
        Coroutine b = StartCoroutine(slowPlaceTiles(chunkPosition));
        yield return b;
        
    }

    public void placeTileAtLocation(int x, int y, int id) {
        addTile(id, getChunkPosition(new Vector2Int(x,y)), getTilePosition(new Vector2Int(x,y)));
        setTile(x,y, getIdDataInChunk(new Vector2Int(x,y)));
        
    }
    public void hitTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        IdData idData = getIdDataInChunk(hitTilePosition);
        if (idData == null) {
            return;
        }
        if (hitHardness(idData)) {
            spawnItemEntity(idData,hitTilePosition,position);
            breakTile(hitTilePosition);
        }
    }

    public void deleteTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        breakTile(hitTilePosition);
        
    }
    protected void addTile(int id, Vector2Int chunkPosition, Vector2Int tilePosition) {
        dimensionChunkData[chunkPosition].data[tilePosition.x][tilePosition.y] = initTileData(id);
    }
    public IdData getIdDataInChunk(Vector2Int realTilePosition) {
        Vector2Int tilePosition = getTilePosition(realTilePosition);
        return dimensionChunkData[getChunkPosition(realTilePosition)].data[tilePosition.x][tilePosition.y];
    }
    protected virtual void setTile(int x, int y,IdData idData) {
        
    }

    protected virtual IdData initTileData(int id) {
        if (id > 0) {
            return IdDataMap.getInstance().GetIdData(id);
        }
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
    protected virtual bool hitHardness(IdData idData) {
        return false;
    }
    protected virtual void breakTile(Vector2Int position) {
        Vector2Int chunkPosition = getChunk(position);
        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        Vector2Int tilePositon = getTilePosition(position);
        dimensionChunkData[chunkPosition].data[tilePositon.x][tilePositon.y] = null;
    }
    protected virtual void spawnItemEntity(IdData idData, Vector2Int hitTilePosition, Vector2 worldPosition) {
        GameObject chunk = ChunkHelper.snapChunk(hitTilePosition.x,hitTilePosition.y);
        Transform entityContainer = Global.findChild(chunk.transform, "Entities").transform;       

        float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
        float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
        
        ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),idData.id,1,entityContainer);
    }
    protected IEnumerator slowPlaceTiles(Vector2Int chunkPosition) {
        ChunkData chunkData = dimensionChunkData[chunkPosition];
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

    public List<List<int>> getTileIds(Vector2Int chunkPosition) {
        ChunkData chunkData = dimensionChunkData[chunkPosition];
        List<List<int>> nestedIds = new List<List<int>>();
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<int> idList = new List<int>();
            for (int yIter = 0; yIter < 16; yIter ++) {
                
                if (chunkData.data[xIter][yIter] == null) {
                    idList.Add(-1);
                } else {
                    idList.Add(chunkData.data[xIter][yIter].id);
                }
                
            }
            nestedIds.Add(idList);
        }
        return nestedIds;
    }
}


















