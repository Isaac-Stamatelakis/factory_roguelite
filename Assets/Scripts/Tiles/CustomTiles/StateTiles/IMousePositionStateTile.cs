using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Type;

namespace Tiles {
    /// <summary>
    /// Used for tiles with different sprites depending on state
    /// </summary>
    public interface IMousePositionStateTile {
        public int GetStateAtPosition(Vector2 position);
    }

    public interface ITypeSwitchType {
        public TileMapType getStateType(int state);
    }

    public interface IStateTileSingle : IStateTile{
        public TileBase GetTileAtState(int state);
        
    }
    public interface IStateTileMultiple : IStateTile
    {
        public void GetTiles(int state, TileBase[] container);
    }

    public interface IStateTile
    {
        public TileBase GetDefaultTile();
        public int GetStateAmount();   
    }
}

