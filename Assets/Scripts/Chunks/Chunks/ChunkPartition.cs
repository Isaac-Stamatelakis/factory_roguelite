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
    public IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps);
    public IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps);
    public bool inRange(Vector2Int target, int xRange, int yRange);
}
public abstract class ChunkPartition<T> : IChunkPartition where T : ChunkPartitionData
{
    protected bool fullLoaded = false;
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
        return fullLoaded;
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

    public virtual IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps) {
        foreach (TileGridMap tileGridMap in tileGridMaps.Values) {
            UnityEngine.Vector2Int realPartitionPosition = getRealPosition();
            if (!tileGridMap.containsPartition(realPartitionPosition)) {
                tileGridMap.initPartition(realPartitionPosition);
            }
        }
        this.fullLoaded = true;
        yield return null;
    }

    public void setLoaded(bool val)
    {
        fullLoaded = val;
    }

    public void setScheduleForUnloading(bool val)
    {
        scheduledForUnloading = val;
    }

    public IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps)
    {
        throw new System.NotImplementedException();
    }
}

public class TileChunkPartition<T> : ChunkPartition<SerializedTileData> where T : SerializedTileData
{
    public TileChunkPartition(SerializedTileData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
    {

    }

    public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps)
    {
        yield return base.load(tileGridMaps);
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        Vector2Int realPosition = getRealPosition();
        for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
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
            yield return new WaitForEndOfFrame();
        }
    }
}

public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData> where T : SerializedTileConduitData
{
    public ConduitChunkPartition(SerializedTileConduitData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
    {
    }

    public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps)
    {
        return base.load(tileGridMaps);
    }
}