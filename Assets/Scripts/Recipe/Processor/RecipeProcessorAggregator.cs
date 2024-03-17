using System.Collections;
using System.Collections.Generic;
using ItemModule.Inventory;
using TileEntityModule;
using UnityEngine;

namespace RecipeModule.Processors {
    public abstract class RecipeProcessorAggregator<Layout> : RecipeProcessor, IRegisterableProcessor where Layout : InventoryLayout
    {
        [SerializeField] protected GameObject uiPrefab;
        [SerializeField] protected Layout layout;
        public InventoryLayout getInventoryLayout()
        {
            return layout;
        }

        public GameObject getUIPrefab()
        {
            return uiPrefab;
        }
    }
}

