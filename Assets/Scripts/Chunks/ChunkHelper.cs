using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHelper
{
    private static int chunkLayer = 1 << LayerMask.NameToLayer("Chunk");
    public static GameObject snapChunk(float x, float y) {
        RaycastHit2D hitChunk = Physics2D.Raycast(new Vector2(x,y), Vector2.zero, Mathf.Infinity, chunkLayer);
        if (hitChunk.collider == null) {
            return null;
        }
        return hitChunk.collider.gameObject;

    }
}
