using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items;
using Items.Tags;
using PlayerModule;

namespace TileEntity.Instances.Matrix {
    public class RecipeEncoderUI : MonoBehaviour, IPlayerInventoryIntegratedUI
    {
        [SerializeField] private Button clearButton;
        [SerializeField] private Button encodeButton;
        [SerializeField] private GridLayoutGroup recipeInputs;
        [SerializeField] private GridLayoutGroup recipeOutputs;
        [SerializeField] private GridLayoutGroup patternInput;
        [SerializeField] private GridLayoutGroup patternOutput;
        [SerializeField] private GridLayoutGroup playerInventoryContainer;
        private MatrixRecipeEncoderInstance recipeEncoder;

        public void init(MatrixRecipeEncoderInstance recipeEncoder) {
            this.recipeEncoder = recipeEncoder;
            clearButton.onClick.AddListener(() => {
                for (int i = 0; i < recipeEncoder.RecipeInputs.Count; i++) {
                    recipeEncoder.RecipeInputs[i] = null;
                }
                for (int i = 0; i < recipeEncoder.RecipeOutputs.Count; i++) {
                    recipeEncoder.RecipeOutputs[i] = null;
                }
            });
            /*
            encodeButton.onClick.AddListener(encodeButtonClick);

            GlobalHelper.deleteAllChildren(recipeInputs.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.RecipeInputs,recipeInputs.transform,ItemDisplayUtils.SolidItemPanelColor);
            RecipeEncoderInventoryUI recipeInputUI = recipeInputs.gameObject.AddComponent<RecipeEncoderInventoryUI>();
            recipeInputUI.initalize(recipeEncoder.RecipeInputs);
            
            GlobalHelper.deleteAllChildren(recipeOutputs.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.RecipeOutputs,recipeOutputs.transform,ItemDisplayUtils.SolidItemPanelColor);
            RecipeEncoderInventoryUI recipeOutputUI = recipeOutputs.gameObject.AddComponent<RecipeEncoderInventoryUI>();
            recipeOutputUI.initalize(recipeEncoder.RecipeOutputs);

            GlobalHelper.deleteAllChildren(patternInput.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.BlankRecipes,patternInput.transform,ItemDisplayUtils.SolidItemPanelColor);
            TagRestrictedInventoryUI patternInputsInventoryUI = patternInput.gameObject.AddComponent<TagRestrictedInventoryUI>();
            patternInputsInventoryUI.initalize(recipeEncoder.BlankRecipes,ItemTag.EncodedRecipe);

            GlobalHelper.deleteAllChildren(patternOutput.transform);
            ItemSlotUIFactory.getSlotsForInventory(recipeEncoder.EncodedRecipes,patternOutput.transform,ItemDisplayUtils.SolidItemPanelColor);
            TagRestrictedInventoryUI patternOutputsInventoryUI = patternOutput.gameObject.AddComponent<TagRestrictedInventoryUI>();
            patternOutputsInventoryUI.initalize(recipeEncoder.EncodedRecipes,ItemTag.EncodedRecipe);
            patternInputsInventoryUI.AllowInputs=false;

            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            playerInventory.cloneInventoryUI(playerInventoryContainer.transform);
            playerInventory.hideUI();
            */

        }

        private void encodeButtonClick() {
            if (ItemSlotUtils.InventoryAllNull(recipeEncoder.RecipeInputs)) {
                return;
            }
            if (ItemSlotUtils.InventoryAllNull(recipeEncoder.RecipeOutputs)) {
                return;
            }
            for (int i = 0; i < recipeEncoder.BlankRecipes.Count; i++) {
                ItemSlot itemSlot = recipeEncoder.BlankRecipes[i];
                if (itemSlot != null && itemSlot.itemObject != null && itemSlot.tags != null && !itemSlot.tags.Dict.ContainsKey(ItemTag.EncodedRecipe)) {
                    continue;
                }
                ItemSlot spliced = ItemSlotFactory.Copy(itemSlot);
                List<ItemSlot> copyInputs = ItemSlotFactory.CopyList(recipeEncoder.RecipeInputs);
                List<ItemSlot> copyOutputs = ItemSlotFactory.CopyList(recipeEncoder.RecipeOutputs);
                spliced.tags.Dict[ItemTag.EncodedRecipe] = new EncodedRecipe(
                    copyInputs,
                    copyOutputs
                );
                spliced.amount = 1;
                if (!ItemSlotUtils.CanInsertIntoInventory(recipeEncoder.EncodedRecipes,spliced,Global.MAX_SIZE)) {
                    continue;
                }
                ItemSlotUtils.InsertIntoInventory(recipeEncoder.EncodedRecipes,spliced,Global.MAX_SIZE);
                itemSlot.amount--;
                if (itemSlot.amount <= 0) {
                    itemSlot.itemObject = null;
                }
            }
        }
    }
}

