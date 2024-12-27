using System.Collections;
using System.Collections.Generic;
using Recipe.Viewer;
using TileEntity;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.Machines;
using UnityEngine;

public class ProcessingMachineUI : MonoBehaviour, ITileEntityUI<ProcessingMachineInstance>, IRecipeProcessorUI
{
    [SerializeField] private MachineUI machineUI;
    public void DisplayTileEntityInstance(ProcessingMachineInstance tileEntityInstance)
    {
        throw new System.NotImplementedException();
    }

    public void DisplayRecipe(DisplayableRecipe recipes)
    {
        throw new System.NotImplementedException();
    }
}
