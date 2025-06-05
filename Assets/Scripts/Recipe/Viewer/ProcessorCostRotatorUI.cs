using System;
using TileEntity.Instances.Machine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recipe.Viewer
{
    public class ProcessorCostRotatorUI : MonoBehaviour
    {
        private int counter;
        private int index;
        
        private TransmutationDisplayableRecipe transmutationRecipe;
        private Action<ItemDisplayableRecipe> displayRecipeCostAction;

        public void Initialize(TransmutationDisplayableRecipe transmutationDisplayableRecipe, Action<ItemDisplayableRecipe> displayRecipeCostAction)
        {
            counter = 0;
            index = transmutationDisplayableRecipe.InitialDisplayIndex;
            transmutationRecipe = transmutationDisplayableRecipe;
            this.displayRecipeCostAction = displayRecipeCostAction;
            Display();
        }
        public void FixedUpdate()
        {
            if (Keyboard.current.shiftKey.isPressed) return;
            if (transmutationRecipe == null || displayRecipeCostAction == null) return;
            counter++;
            if (counter % TileEntityInventoryUI.TRANSMUTATION_ROTATE_RATE != 0) return;
            index++;
            index %= transmutationRecipe.Inputs.Count;
            Display();
            
        }

        private void Display()
        {
            ItemDisplayableRecipe itemDisplayableRecipe = transmutationRecipe.ToItemDisplayableRecipe(index);
            displayRecipeCostAction?.Invoke(itemDisplayableRecipe);
        }
    }
}
