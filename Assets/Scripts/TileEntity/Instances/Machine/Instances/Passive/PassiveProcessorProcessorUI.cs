using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using System;
using Recipe.Viewer;
using RecipeModule;

namespace TileEntity.Instances.Machines {
    public class PassiveProcessorProcessorUI : MonoBehaviour, ITileEntityUI<PassiveProcessorInstance>, IRecipeProcessorUI
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public ArrowProgressController arrowProgressController;
        [SerializeField] public Image panel;
        private GameObject slotPrefab;
        
        public void DisplayTileEntityInstance(PassiveProcessorInstance tileEntityInstance)
        {
            
        }

        public void DisplayRecipe(DisplayableRecipe recipes)
        {
            
        }
    }
}
