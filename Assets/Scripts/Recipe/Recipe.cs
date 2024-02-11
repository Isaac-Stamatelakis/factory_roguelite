using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMachineRecipe : IEnergyRecipe, IRecipe {
    
}
public interface IMatchableRecipe {
    public bool match(List<ItemSlot> givenInputs, List<ItemSlot> givenOutputs, int firstAvaiableOutputIndex);
}

public interface IEnergyRecipe {
    public int getRequiredEnergy();
    public int getEnergyPerTick();
}

public interface IRecipe {
    public List<ItemSlot> getOutputs();
}
[CreateAssetMenu(fileName ="R~New Recipe",menuName="Crafting/Recipe/Standard")]
public class Recipe : ScriptableObject, IMatchableRecipe, IRecipe
{
    public List<ItemSlot> inputs;
    public List<ItemSlot> outputs;
    // Enable loading of items which have been deleted and recreated (such as transmutables)
    [Header("DO NOT EDIT\nPaths for inputs/outputs\nSet automatically")]
    public List<string> inputGUIDs;
    public List<string> outputGUIDs;
    
    public List<string> InputPaths {get{return inputGUIDs;} set{inputGUIDs = value;}}
    public List<string> OutputPaths {get{return outputGUIDs;} set{outputGUIDs = value;}}

    public List<ItemSlot> getOutputs()
    {
        return outputs;
    }

    public virtual bool match(List<ItemSlot> givenInputs, List<ItemSlot> givenOutputs, int firstAvaiableOutputIndex) {
        int requiredOutputSpace = outputs.Count;
        if (requiredOutputSpace >= givenOutputs.Count-firstAvaiableOutputIndex) {
            // No space for recipe
            return false;
        }
        // Quick check O(n)
        if (givenInputs.Count < inputs.Count) {
            return false;
        }
        // Check recipes O(n+m)
        var inputItemAmounts = new Dictionary<string, int>();
        foreach (ItemSlot itemSlot in givenInputs) {
            if (itemSlot.itemObject != null) {
                if (inputItemAmounts.ContainsKey(itemSlot.itemObject.id)) {
                    inputItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
                } else {
                    inputItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
                }
            }
        }
        var recipeItemAmounts = new Dictionary<string, int>();
        foreach (ItemSlot itemSlot in inputs) {
            if (itemSlot.itemObject != null) {
                if (recipeItemAmounts.ContainsKey(itemSlot.itemObject.id)) {
                    recipeItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
                } else {
                    recipeItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
                }
            }
        }
        foreach (string id in inputItemAmounts.Keys) {
            if (recipeItemAmounts.ContainsKey(id)) {
                recipeItemAmounts[id] -= inputItemAmounts[id];
            }
        }
        bool success = true;
        foreach (int amount in recipeItemAmounts.Values) {
            if (amount > 0) {
                success = false;
                break;
            }
        }
        return success;
    }
}   