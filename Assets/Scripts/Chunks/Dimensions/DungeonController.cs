using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonController : DimController
{
    public override void Awake() {
        base.Awake();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Cave";
        TileClosedChunkSystem area = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
        closedChunkSystems.Add(area);
        Cave cave = new Cave();
        // r = 2, n = 13, d = 0.58, i = 5, floating islands
        // r = 2, n = 14, d = 0.58, i = 5, very good
        // r = 2, n = 15, d = 0.65, i = 5, more disconnected than first area
        // r = 3, n = 31, d = 0.65, i = 5, similar to first area, more long pathways
        // r = 3, n = 31, d = 0.65, i = 10 large disconnected areas
        // r = 3, n = 29, d = 0.58, i = 5, similar to first area more disonneted, skinnier
        // r = 4, n = 48, d = 0.60, i = 5 similar to first area more connected, wider
        // r = 4, n = 48, d = 0.60, i = 20, very wide areas

        cave.areas.Add(new CaveArea(
            new Vector2Int(-2,2),
            new Vector2Int(-2,2),
            2,
            14,
            0.58F,
            5
        ));
        /*
        cave.areas.Add(new CaveArea(
            new Vector2Int(-20,20),
            new Vector2Int(-20,20),
            4,
            48,
            0.6F,
            10
        ));
        */
        CaveGenerator caveGenerator = new CaveGenerator(cave);
        caveGenerator.generate();
        IntervalVector coveredArea = cave.getCoveredArea();
        area.initalize(coveredArea,-1);

    }
}

