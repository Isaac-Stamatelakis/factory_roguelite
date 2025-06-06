using System.Collections.Generic;
using Chunks.IO;
using Entities;


public interface IChunkPartitionData {

}

[System.Serializable]
public class SeralizedWorldData : IChunkPartitionData
{
    
    public SerializedBaseTileData baseData;
    public SerializedBackgroundTileData backgroundData;
    public SeralizedFluidTileData fluidData;
    public List<SeralizedEntityData> entityData;

    public SeralizedWorldData(
        SerializedBaseTileData baseTileData, 
        SerializedBackgroundTileData backgroundTileData, 
        List<SeralizedEntityData> entityData, 
        SeralizedFluidTileData fluidTileData
    ) {
        this.baseData = baseTileData;
        this.backgroundData = backgroundTileData;
        this.entityData = entityData;
        this.fluidData = fluidTileData;
    }
}

[System.Serializable]
public class WorldTileConduitData : SeralizedWorldData {
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
    public SeralizedChunkConduitData matrixConduitData;

    public WorldTileConduitData(
        SerializedBaseTileData baseTileData, 
        SerializedBackgroundTileData backgroundTileData, 
        List<SeralizedEntityData> entityData, 
        SeralizedFluidTileData fluidTileData,
        SeralizedChunkConduitData itemConduitData,
        SeralizedChunkConduitData fluidConduitData,
        SeralizedChunkConduitData energyConduitData,
        SeralizedChunkConduitData signalConduitData,
        SeralizedChunkConduitData matrixConduitData
    ) : base(baseTileData, backgroundTileData, entityData, fluidTileData)
    {
        this.itemConduitData = itemConduitData;
        this.fluidConduitData = fluidConduitData;
        this.energyConduitData = energyConduitData;
        this.signalConduitData = signalConduitData;
        this.matrixConduitData = matrixConduitData;
    }

    
}

public struct PartitionFluidData {
    public string[,] ids;
    public string[,] baseIds;
    public float[,] fill;

    public PartitionFluidData(string[,] ids, string[,] baseIds, float[,] fill)
    {
        this.ids = ids;
        this.baseIds = baseIds;
        this.fill = fill;
    }
}