using System.Collections.Generic;
using ChunkModule.IO;


public interface IChunkPartitionData {

}

public class WorldTileData {
    public WorldTileData(List<EntityData> entityData, SerializedBaseTileData baseData, SerializedBackgroundTileData backgroundData) {
        this.entityData = entityData;
        this.baseData = baseData;
        this.backgroundData = backgroundData;
    }
    public List<EntityData> entityData;
    public SerializedBaseTileData baseData;
    public SerializedBackgroundTileData backgroundData;
}

public class WorldTileConduitData : WorldTileData
{
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
    public WorldTileConduitData(
        List<EntityData> entityData, 
        SerializedBaseTileData baseData, 
        SerializedBackgroundTileData backgroundData,
        SeralizedChunkConduitData itemConduitData,
        SeralizedChunkConduitData fluidConduitData,
        SeralizedChunkConduitData energyConduitData,
        SeralizedChunkConduitData signalConduitData
        ) : base(entityData, baseData, backgroundData)
    {
        this.itemConduitData = itemConduitData;
        this.fluidConduitData = fluidConduitData;
        this.energyConduitData = energyConduitData;
        this.signalConduitData = signalConduitData;
    }
}
[System.Serializable]
public class SerializedTileData : IChunkPartitionData
{
    public List<EntityData> entityData;
    public SerializedBaseTileData baseData;
    public SerializedBackgroundTileData backgroundData;
}

[System.Serializable]
public class SerializedTileConduitData : SerializedTileData {
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
}