using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using Items.Transmutable;
using UnityEngine;
using RecipeModule;
using PlayerModule;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.Workbench;
using TileEntity.Instances.Workbench.UI;
using TileEntity.Instances.WorkBenchs;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TileEntity.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour, ITileEntityUI<WorkBenchInstance>, IRecipeProcessorUI
    {
        [SerializeField] private WorkbenchProcessorUI mWorkbenchProcessorUI;
        [FormerlySerializedAs("recipeLookUpList")] [SerializeField] private RecipeLookUpList mRecipeLookUpList;
        [SerializeField] private VerticalLayoutGroup mInventoryUIGroup;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private InventoryUI mPlayerInventoryUI;

        private WorkBenchInstance workBenchInstance;
        public void TryCraftItem()
        {
            RecipeObject recipeObject = mRecipeLookUpList.GetCurrentRecipe();
            var sourceInventoryUI = workBenchInstance.Inventory == null
                ? PlayerManager.Instance.GetPlayer().PlayerInventory.InventoryUI
                : mInventoryUI;
            List<ItemSlot> sourceInventory = workBenchInstance.Inventory ?? PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory;
            if (recipeObject is ItemRecipeObject itemRecipeObject)
            {
                var outputs = ItemSlotFactory.FromEditorObjects(itemRecipeObject.Outputs);
                if (!ItemSlotUtils.CanInsertIntoInventory(sourceInventory, outputs, Global.MAX_SIZE)) return;
                ItemRecipeObjectInstance itemRecipeObjectInstance = new ItemRecipeObjectInstance(itemRecipeObject);
                var itemRecipe = RecipeUtils.TryCraftRecipe<ItemRecipe>(itemRecipeObjectInstance, sourceInventory, null, RecipeType.Item);
                if (itemRecipe == null) return;
                ItemSlotUtils.InsertInventoryIntoInventory(sourceInventory,itemRecipe.SolidOutputs, Global.MAX_SIZE);
            }

            if (recipeObject is TransmutableRecipeObject transmutableRecipeObject)
            {
                TryCraftTransmutableRecipe(transmutableRecipeObject, sourceInventory);
            }
            
            sourceInventoryUI.RefreshSlots();
        }

        private void TryCraftTransmutableRecipe(TransmutableRecipeObject transmutableRecipeObject, List<ItemSlot> sourceInventory)
        {
            foreach (ItemSlot itemSlot in sourceInventory)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                if (itemSlot.itemObject is not TransmutableItemObject transmutableItemObject) continue;
                if (transmutableItemObject.getState() != transmutableRecipeObject.InputState) continue;
                var output = TransmutableItemUtils.TransmuteOutput(transmutableItemObject.getMaterial(), transmutableRecipeObject);
                if (!ItemSlotUtils.CanInsertIntoInventory(sourceInventory, output, Global.MAX_SIZE)) continue;
                
                var itemRecipe = RecipeFactory.GetTransmutationRecipe(
                    workBenchInstance.TileEntityObject.WorkBenchRecipeProcessor.RecipeType,
                    transmutableItemObject.getMaterial(),
                    transmutableRecipeObject.OutputState,
                    output
                );
                
                var result = RecipeUtils.TryCraftTransmutableRecipe<ItemRecipe>(
                    transmutableRecipeObject,
                    itemSlot,
                    transmutableItemObject.getMaterial(),
                    RecipeType.Item
                );
                if (result != null)
                {
                    ItemSlotUtils.InsertInventoryIntoInventory(sourceInventory,itemRecipe.SolidOutputs, Global.MAX_SIZE);
                    return;
                }
            }
        }

        private List<ItemSlot> GetSourceInventory()
        {
            return workBenchInstance.Inventory ?? PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory;
        }
        public void TryCraftItemRecipe()
        {
            
        }

        public void TryCraftTransmutationRecipe()
        {
            
        }

        public void DisplayTileEntityInstance(WorkBenchInstance tileEntityInstance)
        {
            workBenchInstance = tileEntityInstance;
            mWorkbenchProcessorUI.Initialize(this);
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            mPlayerInventoryUI.DisplayInventory(playerInventory.Inventory);
            RecipeProcessorInstance recipeProcessorInstance = RecipeRegistry.GetProcessorInstance(tileEntityInstance.TileEntityObject.WorkBenchRecipeProcessor);
            mRecipeLookUpList.Initialize(recipeProcessorInstance,this, tileEntityInstance.WorkBenchData); // Note this displays the first recipe
            mInventoryUIGroup.gameObject.SetActive(tileEntityInstance.Inventory!=null);
            if (!ReferenceEquals(mInventoryUI, null))
            {
                mInventoryUI.DisplayInventory(tileEntityInstance.Inventory);
                mInventoryUI.SetConnection(mPlayerInventoryUI);
                mPlayerInventoryUI.SetConnection(mInventoryUI);
            }
        
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            mWorkbenchProcessorUI.DisplayForCraft(recipe);
        }
    }
}

