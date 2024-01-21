using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds two numbers together.
/// </summary>
[CreateAssetMenu(fileName ="New Recipe Processor",menuName="Crafting/Processor")]
public class RecipeProcessor : ScriptableObject
{
    public string id;
    [Header("Max number of items this can take as an input")]
    public int maxInputs;
    [Header("Max number of items this can output")]
    public int maxOutputs;
    [Header("Recipes this completes")]
    public List<Recipe> recipes;
}
