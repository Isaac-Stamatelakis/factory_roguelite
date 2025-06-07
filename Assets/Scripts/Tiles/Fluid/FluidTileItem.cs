using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.ItemObjects.Interfaces;
using Items.Transmutable;
using TileEntity;
using Tiles.Fluid;
using Tiles.Options.Colors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Items {
    public class FluidTileItem : ItemObject, IPlacableItem, IColorableItem
    {
        public const int FLUID_TILE_ARRAY_SIZE = 16;
        public GameStageObject GameStageObject;
        
        [SerializeField] public FluidTile fluidTile;
        [SerializeField] public FluidOptions fluidOptions;
        public override Sprite[] GetSprites()
        {
            return new Sprite[]{GetSprite()};
        }
        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Single;
        }

        public override GameStageObject GetGameStageObject()
        {
            return GameStageObject;
        }

        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            GameStageObject = gameStageObject;
        }

        public override Sprite GetSprite()
        {
            TileBase defaultTile = fluidTile?.GetDefaultTile();
            if (defaultTile is not Tile tile) return null;
            return tile.sprite;
        }

        public TileBase GetTile()
        {
            return fluidTile?.GetDefaultTile();
        }

        public Color Color => fluidOptions.GetFluidColor();
        
        public (Material,Color) GetRendererValues()
        {
            TransmutableItemMaterial transmutableItemMaterial = fluidOptions.MaterialColorOverride;
            Material material = transmutableItemMaterial?.HasShaders ?? false
                ? ItemRegistry.GetInstance().GetTransmutationUIMaterial(transmutableItemMaterial)
                : null;
            
            Color color = Color;
            color.a = fluidOptions.Opacity;
            
            return (material, color);
        }
    }
    [System.Serializable]
    public class FluidOptions {
        [SerializeField] public int viscosity;
        [SerializeField] public bool invertedGravity;
        public bool Lit = false;
        [FormerlySerializedAs("SpeedSlowFactory")] [Range(0,1)] public float SpeedSlowFactor = 0.5f;
        [Range(0,10)] public float DamagePerSecond = 0f;
        public int CollisionDominance = 0;
        public TileItem OnCollisionTile;
        public bool DestroysItems;
        public TileColorOptionObject Color;
        public TransmutableItemMaterial MaterialColorOverride;
        public float Opacity = 1f;

        public Color GetFluidColor()
        {
            Color color = GetDefaultFluidColor();
            color.a = Opacity;
            return color;
        }

        private Color GetDefaultFluidColor()
        {
            if (MaterialColorOverride)
            {
                return MaterialColorOverride.color;
            }

            return Color ? Color.GetColor() : UnityEngine.Color.white;
        }

        public int Viscosity { get => viscosity;}
        public bool InvertedGravity { get => invertedGravity;}
    }
    
}





