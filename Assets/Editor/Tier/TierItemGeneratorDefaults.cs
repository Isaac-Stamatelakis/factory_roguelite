using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Items;
using Recipe.Processor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="Tier Info",menuName="Editor/TierDefault")]
public class TierItemGeneratorDefaults : ScriptableObject
{
    public TierTiles Tiles;
    public RecipeInputs RecipeInputs;
    public RecipeProcessors RecipeProcessors;
    public UIReferences UIReferences;
    public ConduitLayouts ConduitPortLayouts;
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
}
[System.Serializable]
public class RecipeInputs
{
    public ItemObject DefaultLightItem;
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