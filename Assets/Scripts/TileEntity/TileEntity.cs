using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileEntity : ScriptableObject
{
    protected Vector3Int tilePosition;
    protected IChunk chunk;
    public void initalize(Vector2Int tilePosition, IChunk chunk) {
        this.chunk = chunk;
        this.tilePosition = (Vector3Int) tilePosition;
    }

    public Vector3 getRealPosition() {
        return (((Vector3Int)chunk.getPosition()*Global.ChunkSize+tilePosition))/2;
    }
}



