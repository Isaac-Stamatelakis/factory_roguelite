using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Place;
using TileMapModule.Layer;
using TileMapModule.Type;
using TileEntityModule.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/NatureHammer")]
    public class NatureTile : TileBase, IIDTile, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        [SerializeField] public Tile[] natureSlants;
        [SerializeField] public Tile[] natureSlabs;
        
        public Sprite getDefaultSprite()
        {
            return baseTile.sprite;
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

        public int getRandomNatureSlabState() {
            int ran = Random.Range(0,natureSlabs.Length);
            return 3 + natureSlants.Length + ran;
        }

        public int getRandomSlantState() {
            int ran = Random.Range(0,natureSlants.Length);
            return 3 + ran;
        }

        public TileBase getTileAtState(int state)
        {
            if (state == 0) {
                return baseTile;
            } else if (state == 1) {
                return cleanSlab;
            } else if (state == 2) {
                return cleanSlant;
            }
            int tempState = state-3;
            if (tempState < natureSlants.Length) {
                return natureSlants[tempState];
            }
            tempState -= natureSlants.Length;
            if (tempState < natureSlabs.Length) {
                return natureSlabs[tempState];
            }
            return null;
        }
        public int getStateAmount()
        {
            return 3+natureSlants.Length+natureSlabs.Length;
        }
    }
}
