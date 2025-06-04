using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items;
using Items.Transmutable;
using Robot.Tool.Instances.Drill;
using TileEntity;
using Tiles.Options.Colors;
using Tiles.Options.Overlay;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

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
        public bool BreakWhenBroken = false;
        public bool AtleastOne = true;
        public bool Below = false;
        public bool Above = false;
        public bool Side = false;
        public bool BackGround = false;
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
    public class TileOptions
    {
        public bool placeableInCave = true;
        public bool hitable = true;
        public bool rotatable = false;
        public int hardness = 8;
        public bool placeBreakable = false;
        public bool fluidBreakable = false;
        public Tier requiredToolTier;
        public TileAudioType AudioType;
        public TileMovementType movementModifier  = TileMovementType.None;
        public List<DropOption> dropOptions;
        public TilePlacementOptions placementRequirements;
        public TransmutableItemMaterial TransmutableColorOverride;
        public TileColorOptionObject TileColor;
        [FormerlySerializedAs("Overlay")] public TileOverlayData overlayData;
        public TileParticleOptions ParticleGradient;

        public Color GetTileColor()
        {
            if (TransmutableColorOverride)
            {
                return TransmutableColorOverride.color;
            }

            if (TileColor)
            {
                return TileColor.GetColor();
            }
            return Color.white;
        }
    }

    [System.Serializable]
    public class TileParticleOptions
    {
        public Color FirstGradientColor;
        public Color SecondGradientColor;
    }
    [System.Serializable]
    public class DropOption {
        public ItemObject itemObject;
        public int weight;
        public int lowerAmount;
        public int upperAmount;
    }
}