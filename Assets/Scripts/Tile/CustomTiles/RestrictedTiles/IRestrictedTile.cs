using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Type;

namespace Tiles {
    /// <summary>
    /// Used for tiles with different sprites depending on state
    /// </summary>
    public interface IRestrictedTile {
        public Sprite getSprite();
        public int getStateAtPosition(Vector2 position, VerticalMousePosition verticalMousePosition,HorizontalMousePosition horizontalMousePosition);
        public TileBase getTileAtState(int state);
        public int getStateAmount();
    }

    public interface ITypeSwitchType {
        public TileMapType getStateType(int state);
    }
}
