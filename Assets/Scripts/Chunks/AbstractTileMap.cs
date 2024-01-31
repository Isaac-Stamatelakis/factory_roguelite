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
    public TileMapType type;
    protected Tilemap tilemap;
    public Tilemap mTileMap {get{return tilemap;}}
    protected TilemapRenderer tilemapRenderer;
    protected TilemapCollider2D tilemapCollider;
    protected Dictionary<Pos2D, PlacedItem[,]> partitions;
    protected DevMode devMode;


    public virtual void Awake() {
        tilemap = gameObject.AddComponent<Tilemap>();
        partitions = new Dictionary<Pos2D, PlacedItem[,]>();
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
    
    public void initPartition(Pos2D partitionPosition) {
        partitions[partitionPosition] = new PlacedItem[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
    }

    public void removePartition(Pos2D partitionPosition) {
        if (this.containsPartition(partitionPosition)) {
            partitions.Remove(partitionPosition);
        }
    }

    public bool containsPartition(Pos2D partitionPosition) {
        return this.partitions.ContainsKey(partitionPosition);
    }

    public void placeTileAtLocation(int x, int y, Item itemObject) {
        addTile(itemObject, getChunkPosition(new Pos2D(x,y)), getTilePosition(new Vector2(x,y)));
        setTile(x,y, getIdDataInChunk(new Pos2D(x,y)));
        
    }
    public void hitTile(Vector2 position) {
        Pos2D hitTilePosition = getHitTilePosition(position);
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
        Pos2D hitTilePosition = getHitTilePosition(position);
        breakTile(hitTilePosition);
        
    }
    protected void addTile(Item itemObject, Pos2D partitionPosition, Pos2D tilePosition) {
        partitions[partitionPosition][tilePosition.x,tilePosition.y] = initTileData(itemObject);
    }
    public PlacedItem getIdDataInChunk(Pos2D realTilePosition) {
        Pos2D tilePosition = getTilePositionInPartition(realTilePosition);
        return (PlacedItem) partitions[getPartitionPosition(realTilePosition)][tilePosition.x,tilePosition.y];
    }
    protected virtual void setTile(int x, int y,PlacedItem placedItem) {
        
    }
    protected abstract PlacedItem initTileData(Item itemObject);
    protected Pos2D getChunkPosition(Pos2D position) {
        return new Pos2D(Mathf.FloorToInt(position.x/(Global.ChunkSize/2)), Mathf.FloorToInt(position.y/(Global.ChunkSize/2)));
    }
    protected Pos2D getPartitionPosition(Pos2D position) {
        return new Pos2D(Mathf.FloorToInt(position.x/(Global.ChunkPartitionSize/2)), Mathf.FloorToInt(position.y/(Global.ChunkPartitionSize/2)));
    }
    protected Pos2D getTilePositionInPartition(Pos2D position) {
        return new Pos2D(Global.modInt(position.x,Global.ChunkPartitionSize),Global.modInt(position.y,Global.ChunkPartitionSize));
    }

    protected abstract Pos2D getHitTilePosition(Vector2 position);

    protected Pos2D getTilePosition(Vector2 position) {
        Vector3Int vect = tilemap.WorldToCell(position);
        return new Pos2D(vect.x,vect.y);
    }
    protected virtual bool hitHardness(PlacedItem placedItem) {
        return false;
    }
    protected virtual void breakTile(Pos2D position) {
        Pos2D chunkPartition = getPartitionPosition(position);
        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        Pos2D tilePositon = getTilePositionInPartition(position);
        partitions[chunkPartition][tilePositon.x,tilePositon.y] = null;
    }
    protected virtual void spawnItemEntity(Item itemObject, Pos2D hitTilePosition, Vector2 worldPosition) {
        GameObject chunk = ChunkHelper.snapChunk(hitTilePosition.x,hitTilePosition.y);
        Transform entityContainer = Global.findChild(chunk.transform, "Entities").transform;       

        float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
        float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
        ItemSlot itemSlot = new ItemSlot(itemObject: itemObject, amount: 1, nbt : new Dictionary<ItemSlotOption, object>());
        ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,entityContainer);
    }
    protected IEnumerator slowPlaceTiles(Pos2D chunkPosition,int sectionAmount, double angle) {
        /*
        ChunkData<PlacedItem> chunkData = partitions[chunkPosition];
        double deg = Mathf.Rad2Deg*angle;
        if (45 <= deg && deg < 135) { // moving up
            for (int xIter=0; xIter < Global.PartitionsPerChunk; xIter ++) {
                for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
                    setTile(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter + Global.PartitionsPerChunk*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (xIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        } else if (135 <= deg && deg < 215) { // moving left
            for (int yIter=0; yIter < Global.PartitionsPerChunk; yIter ++) {
                for (int xIter = 0; xIter < Global.PartitionsPerChunk; xIter ++) {
                    setTile(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter + Global.PartitionsPerChunk*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (yIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        } else if (225 <= deg && deg < 315) { // moving down
           for (int yIter=Global.PartitionsPerChunk-1; yIter >= 0; yIter --) {
                for (int xIter = 0; xIter < Global.PartitionsPerChunk; xIter ++) {
                    setTile(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter + Global.PartitionsPerChunk*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (yIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }

        } else {
            for (int xIter=Global.PartitionsPerChunk-1; xIter >= 0; xIter --) {
                for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
                    setTile(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter + Global.PartitionsPerChunk*chunkPosition.y,chunkData.data[xIter][yIter]);
                }
                if (xIter % sectionAmount == 0) {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        */
        yield return null;
    }
    protected void instantlyPlaceTiles(Vector2Int chunkPosition) {
        /*
        ChunkData<PlacedItem> chunkData = partitions[chunkPosition];
        for (int xIter=0; xIter < Global.PartitionsPerChunk; xIter ++) {
            for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
                setTile(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter + Global.PartitionsPerChunk*chunkPosition.y,chunkData.data[xIter][yIter]);
            }
        }
        */
    }

    protected Pos2D getChunk(Pos2D tileMapPosition) {
        return new Pos2D(Mathf.FloorToInt(tileMapPosition.x/Global.PartitionsPerChunk),Mathf.FloorToInt(tileMapPosition.y/Global.PartitionsPerChunk));
    }

    public IEnumerator removeChunk(Pos2D chunkPosition) {
        partitions.Remove(chunkPosition);
        for (int xIter=0; xIter < Global.PartitionsPerChunk; xIter ++) {
            for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
                
                tilemap.SetTile(new Vector3Int(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter+Global.PartitionsPerChunk*chunkPosition.y,0),null);
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void instantlyRemoveChunk(Vector2Int chunkPosition) {
        /*
        partitions.Remove(chunkPosition);
        for (int xIter=0; xIter < Global.PartitionsPerChunk; xIter ++) {
            for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
                tilemap.SetTile(new Vector3Int(xIter + Global.PartitionsPerChunk*chunkPosition.x,yIter+Global.PartitionsPerChunk*chunkPosition.y,0),null);
            }
        }
        */
    }

    public List<List<string>> getTileIds(Pos2D chunkPosition) {
        /*
        ChunkData<PlacedItem> chunkData = partitions[chunkPosition];
        List<List<string>> nestedIds = new List<List<string>>();
        for (int xIter = 0; xIter < Global.PartitionsPerChunk; xIter ++) {
            List<string> idList = new List<string>();
            for (int yIter = 0; yIter < Global.PartitionsPerChunk; yIter ++) {
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
        */
        return null;
    }
}


















