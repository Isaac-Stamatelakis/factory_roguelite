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

    public virtual Recipe getMatchingRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs) {
        // Validations
        if (itemListAllNull(inputs)) {
            // No inputs
            return null;
        }
        int firstAvaiableOutputIndex = 0; string firstAvaiableOutputID = null;
        foreach (ItemSlot itemSlot in outputs) {
            if (itemSlot.itemObject == null) {
                break;
            } else {
                if (itemSlot.amount < Global.MaxSize) {
                    firstAvaiableOutputID = itemSlot.itemObject.id;
                }
            }
            firstAvaiableOutputIndex ++;
        }
        if (firstAvaiableOutputIndex == outputs.Count && firstAvaiableOutputID == null) {
            // No output slots free
            return null;
        }
        return getValidRecipe(inputs,outputs,firstAvaiableOutputIndex);
    }
    protected virtual Recipe getValidRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs, int firstAvaiableOutputIndex) {
        foreach (Recipe recipe in recipes) {
            if (recipe.match(inputs,outputs,firstAvaiableOutputIndex)) {
                return recipe;
            }
        }
        return null;
    }
    private bool itemListAllNull(List<ItemSlot> items) {
        foreach (ItemSlot itemSlot in items) {
            if (itemSlot.itemObject == null) {
                return false;
            }
        }
        return true;
    }

}
