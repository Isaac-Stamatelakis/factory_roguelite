using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;
using Items.Tags;


namespace TileEntity.Instances.Matrix {
    /*
    public class EncodedRecipeTagInventoryUI : IInventoryListener
    {
        private List<ItemSlot> inventory;
        private MatrixInterfaceInstance matrixInterface;
        public override void initalize(List<ItemSlot> items, ItemTag validTag)
        {
            Debug.LogError("Wrong initalizaiton used. Fix this later");
        }

        public void initalize(List<ItemSlot> items, MatrixInterfaceInstance matrixInterface) {
            this.inventory = items;
            this.matrixInterface = matrixInterface;
            validTag = ItemTag.EncodedRecipe;
            InitalizeSlots();
        }

        public override void leftClick(int n)
        {   
            EncodedRecipe initalRecipe = matrixInterface.getRecipe(inventory[n]);
            base.leftClick(n);
            EncodedRecipe postProcessingRecipe = matrixInterface.getRecipe(inventory[n]);
            recipeChange(initalRecipe,postProcessingRecipe);
            
        }

        private void recipeChange(EncodedRecipe inital, EncodedRecipe post) {
            if (inital == null && post != null) {
                matrixInterface.Controller.Recipes.addRecipe(post,matrixInterface);
            }
            if (inital != null && post == null) {
                matrixInterface.Controller.Recipes.removeRecipe(inital,matrixInterface);
            }
        }

        public override void middleClick(int n)
        {
            base.middleClick(n);
        }

        public override void rightClick(int n)
        {
            EncodedRecipe initalRecipe = matrixInterface.getRecipe(inventory[n]);
            base.rightClick(n);
            EncodedRecipe postProcessingRecipe = matrixInterface.getRecipe(inventory[n]);
            recipeChange(initalRecipe,postProcessingRecipe);
        }
        
    }
    */
}

