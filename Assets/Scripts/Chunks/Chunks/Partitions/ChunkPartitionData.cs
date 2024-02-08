using System.Collections.Generic;
using ChunkModule.IO;


public interface ChunkPartitionData {

}

public class WorldTileData {
    public WorldTileData(List<EntityData> entityData, SeralizedChunkTileData baseData, SeralizedChunkTileData backgroundData) {
        this.entityData = entityData;
        this.baseData = baseData;
        this.backgroundData = backgroundData;
    }
    public List<EntityData> entityData;
    public SeralizedChunkTileData baseData;
    public SeralizedChunkTileData backgroundData;
}
[System.Serializable]
public class SerializedTileData : ChunkPartitionData
{
    public List<EntityData> entityData;
    public SeralizedChunkTileData baseData;
    public SeralizedChunkTileData backgroundData;
}

[System.Serializable]
public class SerializedTileConduitData : SerializedTileData {
    public SeralizedChunkConduitData itemConduitData;
    public SeralizedChunkConduitData fluidConduitData;
    public SeralizedChunkConduitData energyConduitData;
    public SeralizedChunkConduitData signalConduitData;
}