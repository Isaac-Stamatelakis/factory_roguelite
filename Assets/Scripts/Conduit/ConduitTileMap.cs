using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class ConduitTileMap : AbstractTileMap<ConduitItem,ConduitData>
{
    protected override void setTile(int x, int y, ConduitData conduitData)
    {
        if (conduitData == null || conduitData.itemObject == null) {
            return;
        }
        RuleTile ruleTile = conduitData.itemObject.ruleTile;
        tilemap.SetTile(new Vector3Int(x,y,0),ruleTile);
    }

    protected override bool hitHardness(ConduitData conduitData)
    {
        /*
        conduitData.hardness--;
        return conduitData.hardness == 0;
        */
        return true;
    }
    protected override ConduitData initTileData(ConduitItem conduitItem)
    {
        return null;
    }
    public List<List<ConduitOptions>> getConduitOptions(Vector2Int chunkPosition) {
        ChunkData<ConduitData> chunkData = dimensionChunkData[chunkPosition];
        List<List<ConduitOptions>> nestedConduitOptions = new List<List<ConduitOptions>>();
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<ConduitOptions> conduitOptionsList = new List<ConduitOptions>();
            for (int yIter = 0; yIter < 16; yIter ++) {
                ConduitData conduitData = chunkData.data[xIter][yIter];
                if (conduitData == null) {
                    conduitOptionsList.Add(null);
                    continue;
                }
                conduitOptionsList.Add(conduitData.conduitOptions);
            }
            nestedConduitOptions.Add(conduitOptionsList);
        }
        
        return nestedConduitOptions;
    }

    
}

