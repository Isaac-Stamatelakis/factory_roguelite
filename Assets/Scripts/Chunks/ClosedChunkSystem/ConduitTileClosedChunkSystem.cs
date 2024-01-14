using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConduitTileClosedChunkSystem : TileClosedChunkSystem
{
    public override void Awake()
    {
        initConduitMapContainer("EnergyConduits",LayerMask.NameToLayer("EnergyConduit"),Global.EnergyConduitZ);
        initConduitMapContainer("ItemConduits",LayerMask.NameToLayer("ItemConduit"),Global.ItemConduitZ);
        initConduitMapContainer("FluidConduits",LayerMask.NameToLayer("FluidConduit"),Global.FluidConduitZ);
        initConduitMapContainer("SignalConduits",LayerMask.NameToLayer("SignalConduit"),Global.SignalConduitZ);
        base.Awake();
    }

    protected void initConduitMapContainer(string containerName, int layer, float z) {
        GameObject container = new GameObject();
        container.transform.SetParent(gameObject.transform);
        container.name = containerName;
        container.layer = layer;
        container.transform.localPosition = new Vector3(0,0,z);
        Grid grid = container.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1f);
        container.AddComponent<ConduitTileMap>();
        Global.setStatic(container);
    }


}
