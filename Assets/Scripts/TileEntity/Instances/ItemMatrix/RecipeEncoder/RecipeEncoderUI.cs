using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items;
using Items.Tags;
using PlayerModule;

namespace TileEntityModule.Instances.Matrix {
    public class RecipeEncoderUI : MonoBehaviour, IPlayerInventoryIntegratedUI
    {
        [SerializeField] private Button clearButton;
        [SerializeField] private Button encodeButton;
        [SerializeField] private GridLayoutGroup recipeInputs;
        [SerializeField] private GridLayoutGroup recipeOutputs;
        [SerializeField] private GridLayoutGroup patternInput;
        [SerializeField] private GridLayoutGroup patternOutput;
        [SerializeField] private GridLayoutGroup playerInventoryContainer;
        private MatrixRecipeEncoder recipeEncoder;

        public void init(MatrixRecipeEncoder recipeEncoder) {
            this.recipeEncoder = recipeEncoder;
            clearButton.onClick.AddListener(() => {
                for (int i = 0; i < recipeEncoder.RecipeInputs.Count; i++) {
                    recipeEncoder.RecipeInputs[i] = null;
                }
                for (int i = 0; i < recipeEncoder.RecipeOutputs.Count; i++) {
                    recipeEncoder.RecipeOutputs[i] = null;
                }
            });

            encodeButton.onClick.AddListener(encodeButtonClick);

            GlobalHelper.deleteAllChildren(recipeInputs.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.RecipeInputs,recipeInputs.transform);
            RecipeEncoderInventoryUI recipeInputUI = recipeInputs.gameObject.AddComponent<RecipeEncoderInventoryUI>();
            recipeInputUI.initalize(recipeEncoder.RecipeInputs);
            
            GlobalHelper.deleteAllChildren(recipeOutputs.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.RecipeOutputs,recipeOutputs.transform);
            RecipeEncoderInventoryUI recipeOutputUI = recipeOutputs.gameObject.AddComponent<RecipeEncoderInventoryUI>();
            recipeOutputUI.initalize(recipeEncoder.RecipeOutputs);

            GlobalHelper.deleteAllChildren(patternInput.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.BlankRecipes,patternInput.transform);
            TagRestrictedInventoryUI patternInputsInventoryUI = patternInput.gameObject.AddComponent<TagRestrictedInventoryUI>();
            patternInputsInventoryUI.initalize(recipeEncoder.BlankRecipes,ItemTag.EncodedRecipe);

            GlobalHelper.deleteAllChildren(patternOutput.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.EncodedRecipes,patternOutput.transform);
            TagRestrictedInventoryUI patternOutputsInventoryUI = patternOutput.gameObject.AddComponent<TagRestrictedInventoryUI>();
            patternOutputsInventoryUI.initalize(recipeEncoder.EncodedRecipes,ItemTag.EncodedRecipe);
            patternInputsInventoryUI.AllowInputs=false;

            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            playerInventory.cloneInventoryUI(playerInventoryContainer.transform);
            playerInventory.hideUI();

        }

        public void OnDestroy() {
            PlayerContainer.getInstance().getInventory().showUI(); 
        }

        private void encodeButtonClick() {
            if (ItemSlotHelper.inventoryAllNull(recipeEncoder.RecipeInputs)) {
                return;
            }
            if (ItemSlotHelper.inventoryAllNull(recipeEncoder.RecipeOutputs)) {
                return;
            }
            for (int i = 0; i < recipeEncoder.BlankRecipes.Count; i++) {
                ItemSlot itemSlot = recipeEncoder.BlankRecipes[i];
                if (itemSlot != null && itemSlot.itemObject != null && itemSlot.tags != null && !itemSlot.tags.Dict.ContainsKey(ItemTag.EncodedRecipe)) {
                    continue;
                }
                ItemSlot spliced = ItemSlotFactory.copy(itemSlot);
                List<ItemSlot> copyInputs = ItemSlotFactory.copyList(recipeEncoder.RecipeInputs);
                List<ItemSlot> copyOutputs = ItemSlotFactory.copyList(recipeEncoder.RecipeOutputs);
                spliced.tags.Dict[ItemTag.EncodedRecipe] = new EncodedRecipe(
                    copyInputs,
                    copyOutputs
                );
                spliced.amount = 1;
                if (!ItemSlotHelper.canInsertIntoInventory(recipeEncoder.EncodedRecipes,spliced,Global.MaxSize)) {
                    continue;
                }
                ItemSlotHelper.insertIntoInventory(recipeEncoder.EncodedRecipes,spliced,Global.MaxSize);
                itemSlot.amount--;
                if (itemSlot.amount <= 0) {
                    itemSlot.itemObject = null;
                }
            }
        }
    }
}

