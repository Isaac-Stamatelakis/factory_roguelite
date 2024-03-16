using System.Collections;
using System.Collections.Generic;
using ItemModule.Inventory;
using UnityEngine;

namespace RecipeModule.Processors {
    public abstract class RecipeProcessorAggregator : RecipeProcessor, IRegisterableProcessor
    {
        [SerializeField] protected GameObject uiPrefab;
        [SerializeField] protected InventoryLayout layout;
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

