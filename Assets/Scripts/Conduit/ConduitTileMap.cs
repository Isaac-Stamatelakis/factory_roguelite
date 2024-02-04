using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class ConduitTileMap : AbstractTileMap<ConduitItem, ConduitData>
{
    protected override void setTile(int x, int y, ConduitData conduitData)
    {
        if (conduitData == null || conduitData.getItemObject() == null) {
            return;
        }
        RuleTile ruleTile = ((ConduitItem) conduitData.getItemObject()).ruleTile;
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
    public List<List<ConduitOptions>> getConduitOptions(UnityEngine.Vector2Int chunkPosition) {
        /*
        ChunkData<ConduitData> chunkData = partitions[chunkPosition];
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
        */
        return null;
    }

    protected override Vector2Int getHitTilePosition(Vector2 position)
    {
        throw new NotImplementedException();
    }

    public override void initPartition(Vector2Int partitionPosition)
    {
        partitions[partitionPosition] = new ConduitData[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
    }
}

