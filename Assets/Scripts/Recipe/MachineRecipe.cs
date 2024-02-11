using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="R~New Machine Recipe",menuName="Crafting/Recipe/Machine")]
public class MachineRecipe : Recipe,IMachineRecipe
{
    [Header("Energy required to complete recipe")]
    public int energy;
    public int energyPerTick;

    public int getEnergyPerTick()
    {
        throw new System.NotImplementedException();
    }

    public int getRequiredEnergy()
    {
        return energy;
    }
}
