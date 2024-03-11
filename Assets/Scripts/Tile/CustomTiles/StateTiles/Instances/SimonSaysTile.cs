using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Place;
using TileMapModule.Layer;
using TileMapModule.Type;
using TileEntityModule.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/SimonSays")]
    public class SimonSaysTile : TileBase, IIDTile, ITypeSwitchType, IStateTile
    {
        [SerializeField] public string id;
        [Header("This tile will be colored red/blue/green/yellow")]
        [SerializeField] public Tile tile;
        private Tile[] coloredTiles;
        public Sprite getDefaultSprite()
        {
            return tile.sprite;
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
            if (state == 0) {
                return tile;
            }
            if (coloredTiles == null) {
                initColoredTiles();
            }
            if (state <= coloredTiles.Length) {
                return coloredTiles[state-1];
            }
            return null;
        }

        private void initColoredTiles() {
            coloredTiles = new Tile[4];
            for (int i = 0; i < 4; i++) {
                Tile newTile = ScriptableObject.Instantiate(tile);
                SimonSaysColor color = (SimonSaysColor)(i+1);
                switch (color) {
                    case SimonSaysColor.Red:
                        newTile.color = Color.red;
                        break;
                    case SimonSaysColor.Green:
                        newTile.color = Color.green;
                        break;
                    case SimonSaysColor.Blue:
                        newTile.color = Color.blue;
                        break;
                    case SimonSaysColor.Yellow:
                        newTile.color = Color.yellow;
                        break;

                }
                coloredTiles[i] = newTile;
            }
        }
        public TileMapType getStateType(int state)
        {
            return TileMapType.Block;
        }

        public int getStateAmount()
        {
            return 1+coloredTiles.Length;
        }
    }
}

