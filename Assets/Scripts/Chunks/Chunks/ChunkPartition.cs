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
    public void save(Dictionary<TileMapType, ITileMap> tileGridMaps);
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
        return  parent.getPosition()*Global.PartitionsPerChunk + position;
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

    public abstract void save(Dictionary<TileMapType, ITileMap> tileGridMaps);

    public void setLoaded(bool val)
    {
        tileLoaded = val;
    }

    public void setScheduleForUnloading(bool val)
    {
        scheduledForUnloading = val;
    }

    public IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps) {
        save(tileGridMaps);
        Vector2Int realPosition = getRealPosition();
        foreach (ITileMap tileMap in tileGridMaps.Values) {
            yield return tileMap.removePartition(realPosition);
        }
    }
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
                    place(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else if (angle <= 225) { // left
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                for (int x = Global.ChunkPartitionSize-1; x >=0 ; x --) {
                    place(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else if (angle <= 315) { // down
            for (int y = Global.ChunkPartitionSize-1; y >= 0; y --) {
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    place(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        } else { // right
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    place(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public override void save(Dictionary<TileMapType, ITileMap> tileGridMaps)
    {
        Vector2Int position = getRealPosition();
        SerializedTileData data = (SerializedTileData) getData();
        // Clear data
        for (int x = 0; x < Global.ChunkPartitionSize; x++) {
            for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                data.baseData.ids[x][y] = null;
                data.baseData.sTileOptions[x][y] = null;
                data.backgroundData.ids[x][y] = null;
                data.backgroundData.sTileOptions[x][y] = null;
            }
        }
        
        // Iterate through tilemaps
        foreach (ITileMap tileMap in tileGridMaps.Values) {
            TileMapType tileMapType = tileMap.getType();
            if (!TileMapTypeFactory.typeIsTile(tileMapType)) { // type is valid tile type
                continue;
            }
            // get layer to serialze in (base or background)
            TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMapType);
            IPlacedItemObject[,] tileItemdata = tileMap.getPartitionData(position);
            if (tileItemdata == null) {
                continue;
            }
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    if (tileItemdata[x,y] != null) {
                        TileData tileData = (TileData) tileItemdata[x,y];
                        TileItem tileItem = (TileItem) tileData.getItemObject();
                        switch (layer) {
                            case TileMapLayer.Base:
                                data.baseData.ids[x][y] = tileItem.id;
                                data.baseData.sTileOptions[x][y] = TileOptionFactory.serializeOptions(tileItem.getOptions());
                                break;
                            case TileMapLayer.Background:
                                data.backgroundData.ids[x][y] = tileItem.id;
                                data.backgroundData.sTileOptions[x][y] = TileOptionFactory.serializeOptions(tileItem.getOptions());
                                break;
                        }
                    }
                }
            }
        }
    }

    protected void place(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition) {
        string baseId = data.baseData.ids[x][y];
        string backgroundID = data.backgroundData.ids[x][y];
        Dictionary<string,object> baseOptions = data.baseData.sTileOptions[x][y];
        Dictionary<string,object> backgroundOptions = data.backgroundData.sTileOptions[x][y];
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
        if (backgroundID != null) {
            TileItem tileItem = itemRegistry.getTileItem(backgroundID);
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