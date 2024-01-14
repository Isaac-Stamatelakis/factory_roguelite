using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileClosedChunkSystem : ClosedChunkSystem
{
    public override void Awake()
    {
        base.Awake();
        initTileMapContainer("TileBlocks", LayerMask.NameToLayer("TileBlock"),Global.TileBlockZ);
        initTileMapContainer("TileBackgrounds",LayerMask.NameToLayer("TileBackground"),Global.TileBackGroundZ);
        initTileMapContainer("TileObjects",LayerMask.NameToLayer("TileObject"),Global.TileObjectZ);
    }

    protected void initTileMapContainer(string containerName, int layer, float z) {
        GameObject container = new GameObject();
        container.transform.SetParent(gameObject.transform);
        container.name = containerName;
        container.layer = layer;
        container.transform.localPosition = new Vector3(0,0,z);
        Grid grid = container.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1f);
        container.AddComponent<TileGridMap>();
        Global.setStatic(container);
    }
}
