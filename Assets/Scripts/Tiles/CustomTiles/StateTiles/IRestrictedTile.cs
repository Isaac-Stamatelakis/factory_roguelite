using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Type;

namespace Tiles {
    /// <summary>
    /// Used for tiles with different sprites depending on state
    /// </summary>
    public interface IRestrictedTile {
        
        public int getStateAtPosition(Vector2 position, VerticalMousePosition verticalMousePosition,HorizontalMousePosition horizontalMousePosition);
        
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

