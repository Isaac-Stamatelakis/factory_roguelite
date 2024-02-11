using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="RP~New Transmutable Recipe Processor",menuName="Crafting/Transmutation Processor")]
public class TransmutableRecipeProcessor : RecipeProcessor
{
    [Header("Minimum Accepted Tier (Inclusive)")]
    public int minimumTier = -9999;
    [Header("Maximum Accepted Tier (Inclusive)")]
    public int maximumTier = 9999;
    [Header("Valid Input States")]
    public List<TransmutableItemState> inputStates;
    [Header("Output State")]
    public TransmutableItemState outputState;

    protected override Recipe getValidRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs,int firstAvaiableOutputIndex)
    {
        
        // Check recipes
        return base.getValidRecipe(inputs, outputs,firstAvaiableOutputIndex);

    }

}
