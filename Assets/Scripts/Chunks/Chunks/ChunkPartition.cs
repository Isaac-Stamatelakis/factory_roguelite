using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChunkPartition {
    public ChunkPartitionData getData();
    public UnityEngine.Vector2Int getRealPosition();
    public bool getLoaded();
    public void setLoaded(bool val);
    public float distanceFrom(UnityEngine.Vector2Int target);
    public bool getScheduledForUnloading();
    public void setScheduleForUnloading(bool val);
    public IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle);
    public IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps);
    public bool inRange(Vector2Int target, int xRange, int yRange);
}
public abstract class ChunkPartition<T> : IChunkPartition where T : ChunkPartitionData
{
    protected bool tileLoaded = false;
    protected bool scheduledForUnloading = false;
    protected UnityEngine.Vector2Int position;
    protected T data;
    protected Chunk parent;

    public ChunkPartition(T data, UnityEngine.Vector2Int position, Chunk parent) {
        this.data = data;
        this.position = position;
        this.parent = parent;
    }

    public float distanceFrom(UnityEngine.Vector2Int target)
    {
        UnityEngine.Vector2Int realPosition = getRealPosition();
        return Mathf.Pow(target.x-realPosition.x,2) + Mathf.Pow(target.y-realPosition.y,2);
    }

    public ChunkPartitionData getData()
    {
        return data;
    }

    public bool getLoaded()
    {
        return tileLoaded;
    }

    public UnityEngine.Vector2Int getRealPosition()
    {
        return new UnityEngine.Vector2Int(parent.ChunkPosition.x * Global.PartitionsPerChunk + position.x, parent.ChunkPosition.y * Global.PartitionsPerChunk + position.y);
    }

    public bool getScheduledForUnloading()
    {
        return scheduledForUnloading;
    }

    public bool inRange(Vector2Int target, int xRange, int yRange)
    {
        Vector2Int rPosition = getRealPosition();
        return Mathf.Abs(target.x-rPosition.x) <= xRange && Mathf.Abs(target.y-rPosition.y) <= yRange;
    }
    /// <summary> 
    /// loads chunkpartition into tilegridmaps at given angle
    /// </summary>
    public virtual IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle) {
        this.tileLoaded = true;
        foreach (TileGridMap tileGridMap in tileGridMaps.Values) {
            UnityEngine.Vector2Int realPartitionPosition = getRealPosition();
            if (!tileGridMap.containsPartition(realPartitionPosition)) {
                tileGridMap.initPartition(realPartitionPosition);
            }
        }
        yield return null;
    }

    public void setLoaded(bool val)
    {
        tileLoaded = val;
    }

    public void setScheduleForUnloading(bool val)
    {
        scheduledForUnloading = val;
    }

    public abstract IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps);
}

public class TileChunkPartition<T> : ChunkPartition<SerializedTileData> where T : SerializedTileData
{
    public TileChunkPartition(SerializedTileData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
    {

    }

    public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle)
    {
        yield return base.load(tileGridMaps,angle);
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        Vector2Int realPosition = getRealPosition();
        if (angle > 45 && angle <= 135) { // up
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    iterate(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else if (angle <= 225) { // left
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                for (int x = Global.ChunkPartitionSize-1; x >=0 ; x --) {
                    iterate(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else if (angle <= 315) { // down
            for (int y = Global.ChunkPartitionSize-1; y >= 0; y --) {
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    iterate(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else { // right
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    iterate(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public override IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps)
    {
        Vector2Int position = getRealPosition();
        this.tileLoaded = false;
        foreach (ITileMap tileMap in tileGridMaps.Values) {
            IPlacedItemObject[,] data = tileMap.getPartitionData(position);
            if (data == null) {
                continue;
            }
            yield return tileMap.removePartition(position);
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    if (data[x,y] != null) {
                        TileData tileData = (TileData) data[x,y];
                        TileItem tileItem = (TileItem) tileData.getItemObject();
                    }
                }
            }
            
        }
    }

    protected void iterate(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition) {
        string baseId = data.baseData.ids[x][y];
        Dictionary<string,object> baseOptions = data.baseData.sTileOptions[x][y];
        if (baseId != null) {
            TileItem tileItem = itemRegistry.getTileItem(baseId);
            Dictionary<TileItemOption,object> options = tileItem.getOptions();
            if (tileItem != null) {
                TileData tileData = new TileData(
                    tileItem,
                    options
                );
                ITileMap tileGridMap = tileGridMaps[TileMapTypeFactory.tileToMapType(tileItem.tileType)];
                tileGridMap.placeTileAtLocation(
                    realPosition,
                    new Vector2Int(x,y),
                    tileData
                );
            }
        }
    }
}

public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData> where T : SerializedTileConduitData
{
    public ConduitChunkPartition(SerializedTileConduitData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
    {
    }

    public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle)
    {
        return base.load(tileGridMaps,angle);
    }
}