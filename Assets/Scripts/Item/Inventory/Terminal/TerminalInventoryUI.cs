using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;
using ItemModule.Tags.Matrix;
using ItemModule;

public class TerminalInventoryUI : MonoBehaviour
{
    public void init(Dictionary<MatrixDrive, List<MatrixDriveInventory>> driveInventories, List<EncodedRecipeItem> recipes, Transform itemContainer) {
        GlobalHelper.deleteAllChildren(itemContainer);
        int i = 0;
        foreach (List<MatrixDriveInventory> driveInventoryList in driveInventories.Values) {
            foreach (MatrixDriveInventory matrixDriveInventory in driveInventoryList) {
                foreach (ItemSlot itemSlot in matrixDriveInventory.inventories) {
                    GameObject slotUI = null;
                    ItemState itemState = ItemState.Solid;
                    if (itemSlot != null && itemSlot.itemObject != null && itemSlot.itemObject is IStateItem stateItem) {
                        itemState = stateItem.getItemState();
                    }
                    switch (itemState) {
                        case ItemState.Solid:
                            slotUI = GameObject.Instantiate(Resources.Load<GameObject>(InventoryHelper.SolidSlotPrefabPath));
                            break;
                        case ItemState.Fluid:
                            slotUI = GameObject.Instantiate(Resources.Load<GameObject>(InventoryHelper.FluidSlotPrefabPath));
                            break;
                    }
                    if (slotUI == null) {
                        Debug.LogError("Tried to init inventory with null slot " + name);
                        continue;
                    }
                    slotUI.name = "slot" + i;
                    slotUI.transform.SetParent(itemContainer.transform);
                    i++;
                }
            }  
        }

        foreach (EncodedRecipeItem encodedRecipeItem in recipes) {
            
        }
    }
}
