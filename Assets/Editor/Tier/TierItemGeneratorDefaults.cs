using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using EditorScripts.Tier.Generators;
using Items;
using Items.Transmutable;
using Recipe.Processor;
using Tiles.Options.Overlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

namespace Tier.Generators.Defaults
{

    [CreateAssetMenu(fileName = "Tier Info", menuName = "Editor/TierDefault")]
    public class TierItemGeneratorDefaults : ScriptableObject
    {
        public TierTiles Tiles;
        public RecipeInputs RecipeInputs;
        public RecipeProcessors RecipeProcessors;
        public UIReferences UIReferences;
        public ConduitLayouts ConduitPortLayouts;
        public ItemSprites ItemSprites;
    }


    [System.Serializable]
    public class UIReferences
    {
        public AssetReference ChestUI;
    }

    [System.Serializable]
    public class TierTiles
    {
        public Tile Ladder;
        public Tile Chest;
        public Tile Crate;
        public Tile SingleTank;
        public TileBase Platform;
        public TileBase TorchRod;
        public TileOverlayData TorchSource;
        public TileBase MachineFrame;
    }

    [System.Serializable]
    public class RecipeInputs
    {
        public TransmutableItemMaterial DefaultLightMaterial;
    }

    [System.Serializable]
    public class RecipeProcessors
    {
        public RecipeProcessor WorkBenchProcessor;
        public RecipeProcessor ConstructorProcessor;
    }

    [System.Serializable]
    public class ConduitLayouts
    {
        public ConduitPortLayout ItemInOut;
    }

    [System.Serializable]
    public class ItemSprites
    {
        public RobotArmSprites RobotArmSprites;
        public Sprite MotorSprite;
    }
}