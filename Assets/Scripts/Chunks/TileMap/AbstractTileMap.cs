using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


public interface HitableTileMap {
    public void hitTile(Vector2 position);
    public void deleteTile(Vector2 position);
}

public interface ITileMap {
    public void initPartition(Vector2Int partitionPosition);
    public IEnumerator removePartition(Vector2Int partitionPosition);
    public bool containsPartition(Vector2Int partitionPosition);
    public void placeTileAtLocation(int x, int y, IPlacedItemObject itemObject);
    public void placeTileAtLocation(Vector2Int partition, Vector2Int partitionPosition, IPlacedItemObject itemObject);
    public IPlacedItemObject[,] getPartitionData(Vector2Int partition);
    public TileMapType getType();
}
/**
Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
**/

public abstract class AbstractTileMap<G,T> : MonoBehaviour, HitableTileMap, ITileMap where G : ItemObject where T : IPlacedItemObject
{
    public TileMapType type;
    protected Tilemap tilemap;
    public Tilemap mTileMap {get{return tilemap;}}
    protected TilemapRenderer tilemapRenderer;
    protected TilemapCollider2D tilemapCollider;
    protected Dictionary<Vector2Int, T[,]> partitions;
    protected DevMode devMode;


    public virtual void Awake() {
        tilemap = gameObject.AddComponent<Tilemap>();
        partitions = new Dictionary<Vector2Int, T[,]>();
        tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();

        
        //tilemapRenderer.detectChunkCullingBounds = TilemapRenderer.DetectChunkCullingBounds.Manual;
        //tilemapRenderer.chunkCullingBounds = new Vector3(16,16,0);


        tilemapRenderer.material = Resources.Load<Material>("Material/ShadedMaterial");
        tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.maximumTileChangeCount=100000000;
        
    }
    public virtual void Start() {
        devMode = GameObject.Find("Player").GetComponent<DevMode>();
    }
    
    public abstract void initPartition(UnityEngine.Vector2Int partitionPosition);
    public IEnumerator removePartition(Vector2Int partitionPosition) {
        if (!containsPartition(partitionPosition)) {
            yield return null;
        }
        partitions.Remove(partitionPosition);
        int partitionX = partitionPosition.x*Global.ChunkPartitionSize;
        int partitionY = partitionPosition.y*Global.ChunkPartitionSize;
        for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
            for (int y =0; y < Global.ChunkPartitionSize; y ++) {
                removeTile(partitionX+x,partitionY+y);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    protected void removeTile(int x, int y) {
        tilemap.SetTile(new Vector3Int(x,y,0),null);
    }
    public bool containsPartition(UnityEngine.Vector2Int partitionPosition) {
        return this.partitions.ContainsKey(partitionPosition);
    }

    public void placeTileAtLocation(int x, int y, IPlacedItemObject placedItem) {
        addTile(placedItem, getPartitionPosition(new UnityEngine.Vector2Int(x, y)), getTilePositionInPartition(new Vector2Int(x, y)));
        setTile(x, y, getIdDataInChunk(new UnityEngine.Vector2Int(x, y)));
    }

    public void placeTileAtLocation(Vector2Int partitionPosition, Vector2Int tilePartitionPosition, IPlacedItemObject placedItem)
    {
        addTile(placedItem, partitionPosition,tilePartitionPosition);
        setTile(
            partitionPosition.x *Global.ChunkPartitionSize + tilePartitionPosition.x, 
            partitionPosition.y *Global.ChunkPartitionSize + tilePartitionPosition.y, 
            (T) placedItem
        );
    }
    public void hitTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        T placedData = getIdDataInChunk(hitTilePosition);
        if (placedData == null) {
            return;
        }
        if (hitHardness(placedData)) {
            spawnItemEntity((G) placedData.getItemObject(),hitTilePosition,position);
            breakTile(hitTilePosition);
        }
    }

    public void deleteTile(Vector2 position) {
        Vector2Int hitTilePosition = getHitTilePosition(position);
        breakTile(hitTilePosition);
        
    }
    protected void addTile(IPlacedItemObject placedItem, Vector2Int partitionPosition, Vector2Int tilePosition) {
        if (partitions.ContainsKey(partitionPosition)) {
            partitions[partitionPosition][tilePosition.x,tilePosition.y] = (T) placedItem;
        }
        
    }
    public T getIdDataInChunk(Vector2Int realTilePosition) {
        Vector2Int tilePosition = getTilePositionInPartition(realTilePosition);
        return partitions[getPartitionPosition(realTilePosition)][tilePosition.x,tilePosition.y];
    }
    protected abstract void setTile(int x, int y,T placedItem);
    protected Vector2Int getChunkPosition(Vector2Int position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.ChunkSize/2)), Mathf.FloorToInt(position.y/(Global.ChunkSize/2)));
    }
    protected Vector2Int getPartitionPosition(Vector2Int position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.ChunkPartitionSize)), Mathf.FloorToInt(position.y/(Global.ChunkPartitionSize)));
    }
    protected Vector2Int getTilePositionInPartition(Vector2Int position) {
        return new Vector2Int(Global.modInt(position.x,Global.ChunkPartitionSize),Global.modInt(position.y,Global.ChunkPartitionSize));
    }

    protected abstract Vector2Int getHitTilePosition(Vector2 position);

    protected Vector2Int getTilePosition(Vector2 position) {
        Vector3Int vect = tilemap.WorldToCell(position);
        return new Vector2Int(vect.x,vect.y);
    }
    protected virtual bool hitHardness(T placedItem) {
        return false;
    }
    protected virtual void breakTile(Vector2Int position) {
        Vector2Int chunkPartition = getPartitionPosition(position);
        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        Vector2Int tilePositon = getTilePositionInPartition(position);
        partitions[chunkPartition][tilePositon.x,tilePositon.y] = null;
    }
    protected virtual void spawnItemEntity(G itemObject, Vector2Int hitTilePosition, Vector2 worldPosition) {
        GameObject chunk = ChunkHelper.snapChunk(hitTilePosition.x,hitTilePosition.y);
        Transform entityContainer = Global.findChild(chunk.transform, "Entities").transform;       

        float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
        float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
        ItemSlot itemSlot = new ItemSlot(itemObject: itemObject, amount: 1, nbt : new Dictionary<ItemSlotOption, object>());
        ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,entityContainer);
    }


    protected Vector2Int getChunk(Vector2Int tileMapPosition) {
        return new Vector2Int(Mathf.FloorToInt(tileMapPosition.x/Global.PartitionsPerChunk),Mathf.FloorToInt(tileMapPosition.y/Global.PartitionsPerChunk));
    }

    public List<List<string>> getTileIds(Vector2Int chunkPosition) {
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

    public IPlacedItemObject[,] getPartitionData(Vector2Int partition)
    {
        if (partitions.ContainsKey(partition)) {
            return partitions[partition];
        }
        return null;
    }

    public TileMapType getType()
    {
        return type;
    }
}


















