using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="R~New Machine Recipe",menuName="Crafting/Recipe/Machine")]
public class MachineRecipe : Recipe
{
    public int energy;
    public int mode;
    public virtual bool match(List<ItemSlot> givenInputs, List<ItemSlot> givenOutputs, int firstAvaiableOutputIndex, int processorMode, int processorEnergy) {
        if (mode != processorEnergy) {
            return false;
        }
        if (energy == 0) {
            return false;
        }
        bool success = match(givenInputs,givenOutputs,firstAvaiableOutputIndex);
        if (success) {
            return processorEnergy >= energy;
        }
        return false;
    }
}
