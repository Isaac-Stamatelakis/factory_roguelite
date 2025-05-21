using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="New Nature Tile",menuName="Tile/State/NatureHammer")]
    public class NatureTile : HammerTile
    {
        [SerializeField] public Tile[] natureSlants;
        [SerializeField] public Tile[] natureSlabs;
        
        
        public int GetRandomSlantState() {
            int ran = Random.Range(0,natureSlants.Length);
            return 4 + ran;
        }

        public override TileBase GetTileAtState(int state)
        {
            switch (state)
            {
                case (int)HammerTileState.Solid:
                    return baseTile;
                case (int)HammerTileState.Slab:
                    return cleanSlab;
                case (int)HammerTileState.Slant:
                    return cleanSlant;
                case (int)HammerTileState.Stair:
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

        public override HammerTileState? GetHammerTileState(int state)
        {
            if (state < 4)
            {
                return (HammerTileState)state;
            }
            state -= 4;
            if (state < natureSlants.Length) return HammerTileState.Slant;
            state -= natureSlabs.Length;
            if (state < natureSlabs.Length) return HammerTileState.Slab;
            return null;
        }
        
        public override int GetStateAmount()
        {
            return 4+natureSlants.Length+natureSlabs.Length;
        }
    }
}

