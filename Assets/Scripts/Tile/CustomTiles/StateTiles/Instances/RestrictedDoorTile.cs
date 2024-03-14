using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Place;
using TileMapModule.Layer;
using TileMapModule.Type;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~Door Tile",menuName="Tile/State/Door")]
    public class RestrictedDoorTile : TileBase, IRestrictedTile, IIDTile, ITypeSwitchType, IStateTile
    {
        public string id;
        public Tile left;
        public Tile leftOpen;
        public Tile right;
        public Tile rightOpen;
        public Sprite getDefaultSprite()
        {
            return left.sprite;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
        }

        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }
        public int getStateAtPosition(Vector2 position, VerticalMousePosition verticalMousePosition, HorizontalMousePosition horizontalMousePosition) {
            
            if (horizontalMousePosition == HorizontalMousePosition.Left) {
                return 0;
            }
            return 1;
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
    }
}

