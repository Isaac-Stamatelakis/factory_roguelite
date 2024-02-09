using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dim0Controller : DimController
{
    public override void Start() {
        base.Start();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Dim0System";
        ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
        closedChunkSystems.Add(mainArea);
        mainArea.initalize(
            transform,
            coveredArea: new IntervalVector(new Interval<int>(-4,4), new Interval<int>(-2,2)),
            dim: 0
        );

    }
}
