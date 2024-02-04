using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConduitTileClosedChunkSystem : TileClosedChunkSystem
{
    public override void Awake()
    {
        initTileMapContainer(TileMapType.ItemConduit);
        initTileMapContainer(TileMapType.FluidConduit);
        initTileMapContainer(TileMapType.EnergyConduit);
        initTileMapContainer(TileMapType.SignalConduit);
        base.Awake();
    }


}
