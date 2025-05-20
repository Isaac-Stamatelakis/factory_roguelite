using System.Collections;
using System.Collections.Generic;
using Items;
using Recipe.Processor;
using UnityEngine;

[CreateAssetMenu(fileName ="Tier Info",menuName="Editor/TierDefault")]
public class TierItemGeneratorDefaults : ScriptableObject
{
    public RecipeInputs RecipeInputs;
    public RecipeProcessors RecipeProcessors;
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