using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using PlayerModule;

namespace TileEntityModule.Instances.WorkBenchs {
    public class WorkBenchUI : MonoBehaviour
    {
        public void display(WorkBench workBench) {
            List<ItemSlot> playerInventory = PlayerContainer.getInstance().getInventory().Inventory;
            List<WorkbenchRecipe> workbenchRecipes = workBench.workBenchRecipeProcessor.getCraftableRecipes(0,playerInventory,playerInventory);
            foreach (Recipe recipe in workbenchRecipes) {
                Debug.Log("Hi");
            }
        }
        public static WorkBenchUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("").GetComponent<WorkBenchUI>();
        }
    }
}

