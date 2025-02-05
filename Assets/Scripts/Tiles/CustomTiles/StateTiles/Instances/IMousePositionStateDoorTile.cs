using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;

namespace Tiles {
    public interface IStateLayerTile
    {
        public TileType GetTileType(int state);
    }
    [CreateAssetMenu(fileName ="T~Door Tile",menuName="Tile/State/Door")]
    public class IMousePositionStateDoorTile : TileBase, IMousePositionStateTile, IIDTile, ITypeSwitchType, IStateTile, IStateLayerTile
    {
        public string id;
        public Tile left;
        public Tile leftOpen;
        public Tile right;
        public Tile rightOpen;
        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }
        public int GetStateAtPosition(Vector2 position)
        {
            int mousePosition = MousePositionUtils.GetMousePlacement(position);
            return MousePositionUtils.MouseBiasDirection(mousePosition, MousePlacement.Left) ? 0 : 1;
        }

        public TileBase getTileAtState(int state)
        {
            switch (state) {
                case 0:
                    return left;
                case 1:
                    return right;
                case 2:
                    return leftOpen;
                case 3:
                    return rightOpen;
                default:
                    return null;
            }
        }

        public TileBase GetDefaultTile()
        {
            return left;
        }

        public TileMapType getStateType(int state)
        {
            switch (state) {
                case 0:
                    return TileMapType.Block;
                case 1:
                    return TileMapType.Block;
                case 2:
                    return TileMapType.Object;
                case 3:
                    return TileMapType.Object;
            }
            Debug.LogWarning("Got statetype for invalid state " + state + " for door " + name);
            return TileMapType.Block;
        }

        public int getStateAmount()
        {
            return 4;
        }

        public TileType GetTileType(int state)
        {
            return state < 2 ? TileType.Block : TileType.Object;
        }
    }
}

