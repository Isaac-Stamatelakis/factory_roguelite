using System.Collections;
using System.Collections.Generic;
using TileMapModule.Type;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles {
    [CreateAssetMenu(fileName ="T~Switch Tile",menuName="Tile/Switch")]
    public class SwitchTile : TileBase, IIDTile, ITypeSwitchType, IStateTile
    {
        public string id;
        public Tile first;
        public Tile second;
        public Sprite getDefaultSprite()
        {
            return first.sprite;
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

        public TileBase getTileAtState(int state)
        {
            switch (state) {
                case 0:
                    return first;
                case 1:
                    return second;
            }
            return null;
        }

        public int getStateAmount()
        {
            return 2;
        }

        public TileMapType getStateType(int state)
        {
            switch (state) {
                case 0:
                    return TileMapType.Block;
                case 1:
                    return TileMapType.Object;
            }
            Debug.LogWarning(name + "invalid state ");
            return TileMapType.Block;
        }
    }
}

