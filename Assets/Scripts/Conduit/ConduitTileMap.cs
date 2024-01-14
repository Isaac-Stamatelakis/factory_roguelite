using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class ConduitTileMap : AbstractTileMap
{
    protected override void setTile(int x, int y, IdData idData)
    {
        if (idData == null) {
            return;
        }
        RuleTile ruleTile = Resources.Load<RuleTile>(((ConduitData) idData).ruleTilePath);
        tilemap.SetTile(new Vector3Int(x,y,0),ruleTile);
    }

    protected override bool hitHardness(IdData idData)
    {
        ConduitData conduitData = (ConduitData) idData;
        conduitData.hardness--;
        return conduitData.hardness == 0;
    }
    protected override IdData initTileData(int id)
    {
        return base.initTileData(id);
    }
    public List<List<ConduitOptions>> getConduitOptions(Vector2Int chunkPosition) {
        ChunkData chunkData = dimensionChunkData[chunkPosition];
        List<List<ConduitOptions>> nestedConduitOptions = new List<List<ConduitOptions>>();
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<ConduitOptions> conduitOptionsList = new List<ConduitOptions>();
            for (int yIter = 0; yIter < 16; yIter ++) {
                IdData idData = chunkData.data[xIter][yIter];
                if (idData == null) {
                    conduitOptionsList.Add(null);
                    continue;
                }
                ConduitData conduitData = (ConduitData) idData;
                conduitOptionsList.Add(conduitData.conduitOptions);
            }
            nestedConduitOptions.Add(conduitOptionsList);
        }
        
        return nestedConduitOptions;
    }

    
}

