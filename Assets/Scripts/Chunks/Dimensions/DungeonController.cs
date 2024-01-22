using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonController : DimController
{
    [SerializeField]
    Vector2Int xChunkRange;
    [SerializeField]
    Vector2Int yChunkRange;
    public override void Awake() {
        base.Awake();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Cave";
        TileClosedChunkSystem area = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
        closedChunkSystems.Add(area);
        area.initalize(
            new IntervalVector(new Interval<int>(xChunkRange.x,xChunkRange.y), new Interval<int>(yChunkRange.x,yChunkRange.x)),
            -1,
            new DungeonChunkList(xChunkRange.y,xChunkRange.x,yChunkRange.y,yChunkRange.x,-1,area)
        );

    }
}

