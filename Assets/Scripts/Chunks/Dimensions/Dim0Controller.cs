using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldDataModule;

public class Dim0Controller : DimController
{
    public override void Start() {
        base.Start();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Dim0System";
        ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
        closedChunkSystems.Add(mainArea);
        IntervalVector bounds = WorldCreation.getDim0Bounds();
        mainArea.initalize(
            transform,
            coveredArea: bounds,
            dim: 0
        );

    }
}
