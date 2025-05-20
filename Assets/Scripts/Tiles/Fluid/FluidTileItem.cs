using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Items {
    public class FluidTileItem : ItemObject
    {
        public const int FLUID_TILE_ARRAY_SIZE = 16;
        public GameStageObject GameStageObject;
        public override Sprite[] getSprites()
        {
            return new Sprite[]{getSprite()};
        }
        public Tile getTile(int fill) {
            if (fill == 0) {
                return null;
            }
            fill--;
            return fill switch
            {
                0 => tiles[1],
                < 0 => null,
                _ => fill >= tiles.Length ? tiles[^1] : tiles[fill]
            };
        }

        public Tile GetTile(float fill)
        {
            int tileIndex = Mathf.FloorToInt(tiles.Length * fill);
            if (tileIndex == 0) return null;
            tileIndex--;
            return tiles[tileIndex];
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

        public override Sprite getSprite()
        {
            return tiles[^1].sprite;
        }

        [SerializeField] public Tile[] tiles;
        [SerializeField] public FluidOptions fluidOptions;
    }
    [System.Serializable]
    public class FluidOptions {
        [SerializeField] private int viscosity;
        [SerializeField] private bool invertedGravity;
        public bool Lit = false;
        [FormerlySerializedAs("SpeedSlowFactory")] [Range(0,1)] public float SpeedSlowFactor = 0.5f;
        [Range(0,10)] public float DamagePerSecond = 0f;
        public int CollisionDominance = 0;
        public TileItem OnCollisionTile;
        public Color ParticleColor;
        public bool DestroysItems;
        public FluidOptions(int viscosity, bool invertedGravity) {
            this.viscosity = viscosity;
            this.invertedGravity = invertedGravity;
        }

        public int Viscosity { get => viscosity;}
        public bool InvertedGravity { get => invertedGravity;}
    }
    
}





