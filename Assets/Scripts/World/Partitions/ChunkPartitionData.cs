using System.Collections.Generic;
using ChunkModule.IO;


public interface IChunkPartitionData {

}

public class WorldTileData {
    public WorldTileData(List<EntityData> entityData, SerializedBaseTileData baseData, SerializedBackgroundTileData backgroundData, SeralizedFluidTileData fluidTileData) {
        this.entityData = entityData;
        this.baseData = baseData;
        this.backgroundData = backgroundData;
        this.fluidData = fluidTileData;
    }
    public List<EntityData> entityData;
    public SerializedBaseTileData baseData;
    public SerializedBackgroundTileData backgroundData;
    public SeralizedFluidTileData fluidData;
}

public class WorldTileConduitData : WorldTileData
{
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
    public SeralizedChunkConduitData matrixConduitData;
    public WorldTileConduitData(
        List<EntityData> entityData, 
        SerializedBaseTileData baseData, 
        SerializedBackgroundTileData backgroundData,
        SeralizedFluidTileData fluidTileData,
        SeralizedChunkConduitData itemConduitData,
        SeralizedChunkConduitData fluidConduitData,
        SeralizedChunkConduitData energyConduitData,
        SeralizedChunkConduitData signalConduitData,
        SeralizedChunkConduitData matrixConduitData
        ) : base(entityData, baseData, backgroundData,fluidTileData)
    {
        this.itemConduitData = itemConduitData;
        this.fluidConduitData = fluidConduitData;
        this.energyConduitData = energyConduitData;
        this.signalConduitData = signalConduitData;
        this.matrixConduitData = matrixConduitData;
    }
}
[System.Serializable]
public class SerializedTileData : IChunkPartitionData
{
    public List<EntityData> entityData;
    public SerializedBaseTileData baseData;
    public SerializedBackgroundTileData backgroundData;
    public SeralizedFluidTileData fluidData;
}

[System.Serializable]
public class SerializedTileConduitData : SerializedTileData {
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
    public SeralizedChunkConduitData matrixConduitData;
}