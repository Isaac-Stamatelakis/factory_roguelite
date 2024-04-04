using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags.Matrix;
using ItemModule;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemClickReciever {
        public void itemLeftClick(IMatrixTerminalItemSlotClickListener listener);
        public void itemRightClick(IMatrixTerminalItemSlotClickListener listener);
        public void itemMiddleClick(IMatrixTerminalItemSlotClickListener listener);
    } 

    public interface IMatrixRecipeClickReciever {
        public void rightClickRecipe(int n);
    }
    public class MatrixTerminalInventoryUI : MonoBehaviour, IMatrixTerminalItemClickReciever, IMatrixRecipeClickReciever
    {
        private Transform itemContainer;
        private Dictionary<MatrixDrive, List<MatrixDriveInventory>> driveInventories;
        public void init(Dictionary<MatrixDrive, List<MatrixDriveInventory>> driveInventories, List<EncodedRecipeItem> recipes, Transform itemContainer) {
            this.driveInventories = driveInventories;
            this.itemContainer = itemContainer;
            GlobalHelper.deleteAllChildren(itemContainer);
            int totalIndex = 0;
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in driveInventories) {
                for (int driveIndex = 0; driveIndex < kvp.Value.Count; driveIndex++) {
                    List<ItemSlot> matrixDriveInventory = kvp.Value[driveIndex].inventories;
                    for (int i = 0; i < matrixDriveInventory.Count; i++) {
                        ItemSlot itemSlot = matrixDriveInventory[i];
                        MatrixTerminalItemSlotUI itemSlotUI = MatrixTerminalItemSlotUI.newInstance(kvp.Key,i,driveIndex,this);
                        itemSlotUI.name = "slot" + totalIndex;
                        itemSlotUI.transform.SetParent(itemContainer.transform);
                        totalIndex++;
                        if (itemSlot == null || itemSlot.itemObject == null) {
                            continue;
                        }
                        ItemSlotUIFactory.load(itemSlot,itemSlotUI.transform);
                    }
                }
            }

            foreach (EncodedRecipeItem encodedRecipeItem in recipes) {
                
            }
        }

        public void itemLeftClick(IMatrixTerminalItemSlotClickListener listener)
        {
            MatrixDrive matrixDrive = listener.getMatrixDrive();
            int n = listener.getIndex();
            int driveIndex = listener.getDriveIndex();
            GameObject slot = listener.getGameObject();
            List<MatrixDriveInventory> matrixDriveInventoryList = driveInventories[matrixDrive];
            MatrixDriveInventory matrixDriveInventory = matrixDriveInventoryList[driveIndex];
            ItemSlot itemSlot = matrixDriveInventory.inventories[n];
            ItemState itemState = ItemState.Solid;
            if (itemSlot != null && itemSlot.itemObject != null && itemSlot.itemObject is IStateItem stateItem) {
                itemState = stateItem.getItemState();
            }
            if (itemState == ItemState.Solid) {
                List<ItemSlot> inventory = matrixDriveInventoryList[driveIndex].inventories;
                /*
                GameObject grabbedItem = GameObject.Find("GrabbedItem");
                if (grabbedItem == null) {
                    Debug.LogError("Inventory GrabbedItem is null");
                }
                GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
                ItemSlot inventorySlot = inventory[n];
                ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
                if (ItemSlotHelper.areEqual(grabbedSlot,inventorySlot)) {
                    // Merge
                    int sum = inventorySlot.amount + grabbedSlot.amount;
                    if (sum > Global.MaxSize) {
                        grabbedSlot.amount = sum-Global.MaxSize;
                        inventorySlot.amount = Global.MaxSize;
                    } else { // Overflow
                        inventorySlot.amount = sum;
                        grabbedItemProperties.itemSlot = null;
                    }
                } else {    
                    // Swap
                    inventory[n] = grabbedItemProperties.itemSlot;
                    grabbedItemProperties.itemSlot = inventorySlot;
                }
                grabbedItemProperties.updateSprite();
                */
                SolidItemInventoryHelper.leftClick(inventory,n);
                ItemSlotUIFactory.unload(slot.transform);
                ItemSlotUIFactory.load(inventory[n],slot.transform);
            } else if (itemState == ItemState.Fluid) {

            }
            
        }

        public void itemMiddleClick(IMatrixTerminalItemSlotClickListener listener)
        {
            
        }

        public void itemRightClick(IMatrixTerminalItemSlotClickListener listener)
        {
            
        }

        public void rightClickRecipe(int n)
        {
            
        }
    }
}

