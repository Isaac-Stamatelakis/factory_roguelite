using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IWorkbenchRecipe {
        public bool match(List<ItemSlot> inputs, List<ItemSlot> playerInventory);
    }
    public class WorkbenchRecipe : SingleOutputRecipe, IWorkbenchRecipe
    {
        public bool match(List<ItemSlot> inputs, List<ItemSlot> playerInventory)
        {
            return true;
        }
    }

}
