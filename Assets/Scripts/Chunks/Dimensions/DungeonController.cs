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
        cave.areas.Add(new CaveArea(
            new Vector2Int(-20,20),
            new Vector2Int(-20,10),
            2,
            14,
            0.58F,
            5
        ));
        // r = 2, n = 13, d = 0.58, i = 5, floating islands
        cave.areas.Add(new CaveArea(
            new Vector2Int(-20,20),
            new Vector2Int(10,30),
            2,
            13,
            0.58F,
            5
        ));
        
        IntervalVector coveredArea = cave.getCoveredArea();
        area.initalize(
            coveredArea,
            -1,
            new DungeonChunkList(cave,-1,area)
        );

    }
}

