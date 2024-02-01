using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileClosedChunkSystem : ClosedChunkSystem
{
    public override void Awake()
    {
        base.Awake();
        initTileMapContainer("TileBlocks", LayerMask.NameToLayer("TileBlock"),Global.TileBlockZ,TileMapType.Block);
        initTileMapContainer("TileBackgrounds",LayerMask.NameToLayer("TileBackground"),Global.TileBackGroundZ,TileMapType.Background);
        initTileMapContainer("TileObjects",LayerMask.NameToLayer("TileObject"),Global.TileObjectZ,TileMapType.Object);
    }

    protected void initTileMapContainer(string containerName, int layer, float z, TileMapType tileType) {
        GameObject container = new GameObject();
        container.transform.SetParent(gameObject.transform);
        container.name = containerName;
        container.layer = layer;
        container.transform.localPosition = new Vector3(0,0,z);
        Grid grid = container.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1f);
        TileGridMap tileGridMap = container.AddComponent<TileGridMap>();
        tileGridMap.type = tileType;
        tileGridMaps[tileType] = tileGridMap;
    }
}
