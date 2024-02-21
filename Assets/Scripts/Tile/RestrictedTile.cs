using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles {
    public interface IRestrictedTile {
        public Sprite getSprite();
    }
    [CreateAssetMenu(fileName ="T~Torch Tile",menuName="Tile/Torch")]
    public class RestrictedTile : TileBase, IRestrictedTile, IIDTile
    {
        private int state;
        public string id;
        public Sprite onBlock;
        public Sprite onLeft;
        public Sprite onRight;
        public Sprite onBackground;

        public int State { get => state; set => state = value; }

        public Sprite getSprite()
        {
            return onBlock;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            switch (State) {
                case 0:
                    tileData.sprite = onBlock;
                    break;
                case 1:
                    tileData.sprite = onLeft;
                    break;
                case 2:
                    tileData.sprite = onRight;
                    break;
                case 3:
                    tileData.sprite = onBackground;
                    break;
                default:
                    tileData.sprite = onBlock;
                    break;
            }
        }

        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }
    }
}

