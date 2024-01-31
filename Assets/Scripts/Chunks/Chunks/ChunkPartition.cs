using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChunkPartition {
    public ChunkPartitionData getData();
    public Pos2D getRealPosition();
    public bool getLoaded();
    public void setLoaded(bool val);
    public float distanceFrom(Pos2D target);
    public bool getScheduledForUnloading();
    public void setScheduleForUnloading(bool val);
    public IEnumerator load(Dictionary<TileMapType, TileGridMap> tileGridMaps);
    public IEnumerator unload(Dictionary<TileMapType, TileGridMap> tileGridMaps);
}
public abstract class ChunkPartition<T> : IChunkPartition where T : ChunkPartitionData
{
    protected bool fullLoaded = false;
    protected bool scheduledForUnloading = false;
    protected Pos2D position;
    protected T data;
    protected Chunk parent;

    public ChunkPartition(T data,Pos2D position, Chunk parent) {
        this.data = data;
        this.position = position;
        this.parent = parent;
    }

    public float distanceFrom(Pos2D target)
    {
        Pos2D realPosition = getRealPosition();
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

    public Pos2D getRealPosition()
    {
        return new Pos2D(parent.ChunkPosition.x *Global.PartitionsPerChunk + position.x, parent.ChunkPosition.y *Global.PartitionsPerChunk + position.y);
    }

    public bool getScheduledForUnloading()
    {
        return scheduledForUnloading;
    }

    public virtual IEnumerator load(Dictionary<TileMapType, TileGridMap> tileGridMaps) {
        foreach (TileGridMap tileGridMap in tileGridMaps.Values) {
            Pos2D realPartitionPosition = getRealPosition();
            if (!tileGridMap.containsPartition(realPartitionPosition)) {
                tileGridMap.initPartition(realPartitionPosition);
            }
        }
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

    public IEnumerator unload(Dictionary<TileMapType, TileGridMap> tileGridMaps)
    {
        throw new System.NotImplementedException();
    }
}

public class TileChunkPartition<T> : ChunkPartition<SerializedTileData> where T : SerializedTileData
{
    public TileChunkPartition(SerializedTileData data, Pos2D position, Chunk parent) : base(data, position, parent)
    {

    }

    public override IEnumerator load(Dictionary<TileMapType, TileGridMap> tileGridMaps)
    {
        base.load(tileGridMaps);
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                string baseId = data.baseData.ids[x][y];
                Dictionary<string,object> baseOptions = data.baseData.sTileOptions[x][y];
                if (baseId == null) {
                    TileItem tileItem = itemRegistry.getTileItem(baseId);
                    Dictionary<TileItemOption,object> options = tileItem.getOptions();
                    if (tileItem != null) {
                        TileData tileData = new TileData(
                            tileItem,
                            options
                        );
                        TileGridMap tileGridMap = tileGridMaps[tileItem.tileType];
                    }
                }
                
                
            }
            yield return new WaitForEndOfFrame();
        }
    }
}

public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData> where T : SerializedTileConduitData
{
    public ConduitChunkPartition(SerializedTileConduitData data, Pos2D position, Chunk parent) : base(data, position, parent)
    {
    }

    public override IEnumerator load(Dictionary<TileMapType, TileGridMap> tileGridMaps)
    {
        return base.load(tileGridMaps);
    }
}