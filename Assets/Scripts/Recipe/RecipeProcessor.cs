using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Processes recipes
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
    private Dictionary<int, List<Recipe>> recipesOfMode = new Dictionary<int, List<Recipe>>();
    public virtual IRecipe getMatchingRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs, int mode) {
        // Validations
        if (!itemsNotAllNull(inputs)) {
            return null;
        }
        return getValidRecipe(inputs,outputs,mode);
    }
    protected virtual IRecipe getValidRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs, int mode) {
        if (!recipesOfMode.ContainsKey(mode)) {
            return null;
        }
        foreach (Recipe recipe in recipesOfMode[mode]) {
            if (recipe.match(inputs,outputs)) {
                return (IRecipe) recipe;
            }
        }
        return null;
    }
    private bool itemsNotAllNull(List<ItemSlot> items) {
        foreach (ItemSlot itemSlot in items) {
            if (itemSlot != null && itemSlot.itemObject != null) {
                return true;
            }
        }
        return false;
    }
}
