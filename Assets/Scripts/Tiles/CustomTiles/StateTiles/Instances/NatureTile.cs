using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/NatureHammer")]
    public class NatureTile : TileBase, IIDTile, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        [SerializeField] public Tile stairs;
        [SerializeField] public Tile[] natureSlants;
        [SerializeField] public Tile[] natureSlabs;
        
        public Sprite getDefaultSprite()
        {
            return baseTile.sprite;
        }
        
        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }

        public int getRandomSlantState() {
            int ran = Random.Range(0,natureSlants.Length);
            return 3 + ran;
        }

        public TileBase getTileAtState(int state)
        {
            switch (state)
            {
                case 0:
                    return baseTile;
                case 1:
                    return cleanSlab;
                case 2:
                    return cleanSlant;
                case 3:
                    return stairs;
            }
            int tempState = state-4;
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
            return 4+natureSlants.Length+natureSlabs.Length;
        }
    }
}

