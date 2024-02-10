using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds two numbers together.
/// </summary>
[CreateAssetMenu(fileName ="RP~New Transmutable Recipe Processor",menuName="Crafting/Processor")]
public class RecipeProcessor : ScriptableObject
{
    public string id;
    [Header("Max number of items this can take as an input")]
    public int maxInputs;
    [Header("Max number of items this can output")]
    public int maxOutputs;
    [Header("Recipes this completes")]
    private List<Recipe> recipes;

    public List<Recipe> Recipes { get => recipes; set => recipes = value; }
}
