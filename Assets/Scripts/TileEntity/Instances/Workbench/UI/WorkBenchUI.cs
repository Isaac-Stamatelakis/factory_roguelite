using System.Collections;
using System.Collections.Generic;
using Items.Inventory;
using UnityEngine;
using RecipeModule;
using PlayerModule;
using TileEntity.Instances.Workbench;
using TileEntity.Instances.Workbench.UI;
using TileEntity.Instances.WorkBenchs;
using UnityEngine.UI;

namespace TileEntity.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour, ITileEntityUI<WorkBenchInstance>
    {
        [SerializeField] private WorkbenchProcessorUI mWorkbenchProcessorUI;
        [SerializeField] private RecipeLookUpList recipeLookUpList;
        [SerializeField] private InventoryUI mPlayerInventoryUI;

        public void TryCraftItem()
        {
            
        }

        public void DisplayTileEntityInstance(WorkBenchInstance tileEntityInstance)
        {
            
            mWorkbenchProcessorUI.DisplayRecipe(null);
        }
    }
}

