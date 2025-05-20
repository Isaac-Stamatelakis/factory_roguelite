using System.Collections;
using System.Collections.Generic;
using Items;
using Recipe.Processor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="Tier Info",menuName="Editor/TierDefault")]
public class TierItemGeneratorDefaults : ScriptableObject
{
    public TierTiles Tiles;
    public RecipeInputs RecipeInputs;
    public RecipeProcessors RecipeProcessors;
}

[System.Serializable]
public class TierTiles
{
    public Tile Ladder;
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