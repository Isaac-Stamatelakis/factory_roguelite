using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dim0Controller : DimController
{
    public override void Awake() {
        base.Awake();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Dim0System";
        ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
        closedChunkSystems.Add(mainArea);
        mainArea.initalize(
            coveredArea: new IntervalVector(new Interval<int>(-8,8), new Interval<int>(-4,8)),
            dim: 0
        );

    }
}
