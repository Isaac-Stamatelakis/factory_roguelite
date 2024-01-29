using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A collection of chunks which are stored in the same json file
/// </summary>
public class ChunkCollection : MonoBehaviour
{
    protected JsonData jsonData;
    protected Dictionary<Vector2Int, Chunk> chunks;
    protected int dim;
    public int Dim {get{return dim;}}

    public List<Chunk> getUnloadedChunks() {
        return null;
    }
}
