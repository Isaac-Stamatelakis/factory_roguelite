using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items;

namespace Tiles {
    /// <summary>
    /// Options which can only be modified in editor
    /// </summary>
    [System.Serializable]
    public class StaticTileOptions {
        
    }

    [System.Serializable]
    public class TilePlacementOptions
    {
        public bool requireTileBelow = false;
        public bool requireTileAbove = false;
        public bool requireTileSide = false;
    }
    
    public class BaseTileData {
        public int rotation;
        public int state;
        public bool mirror;
        public BaseTileData(int rotation, int state, bool mirror) {
            this.rotation = rotation;
            this.state = state;
            this.mirror = mirror;
        }
    }
    [System.Serializable]
    public class TileOptions {
        public bool hitable = true;
        public bool rotatable = false;
        public bool hasStates = false;
        public int hardness = 8;
        public List<DropOption> dropOptions;
        public TilePlacementOptions placementOptions;
    } 
    [System.Serializable]
    public class DropOption {
        public ItemObject itemObject;
        public int weight;
        public int lowerAmount;
        public int upperAmount;
    }
}