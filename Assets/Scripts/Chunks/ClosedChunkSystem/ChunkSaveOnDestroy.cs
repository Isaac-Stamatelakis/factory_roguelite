using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSaveOnDestroy : MonoBehaviour
{
    /*
    public IEnumerator saveChunks(Dictionary<Vector2Int, IChunk> chunks, Dictionary<TileMapType, ITileMap> tileGridMaps) {
        yield return StartCoroutine(save(chunks,tileGridMaps));
    }

    protected IEnumerator save(Dictionary<Vector2Int, IChunk> chunks, Dictionary<TileMapType, ITileMap> tileGridMaps) {
        foreach (IChunk chunk in chunks.Values) {
            foreach (List<IChunkPartition> chunkPartitionList in chunk.getChunkPartitions()) {
                foreach (IChunkPartition partition in chunkPartitionList) {
                    if (partition.getLoaded()) {
                        yield return partition.unload(tileGridMaps);
                    }
                }
            }
            ChunkIO.writeChunk(chunk);
        }
    }
    */
}
