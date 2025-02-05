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

    public interface IStateTile {
        public TileBase getTileAtState(int state);
        public TileBase GetDefaultTile();
        public int getStateAmount();   
    }
}

