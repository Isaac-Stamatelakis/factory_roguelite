using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminalUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private GridLayoutGroup itemContainer;
        [SerializeField] private ScrollRect itemContainerScroll;
        [SerializeField] private SolidItemInventory inventoryUI;

        public void init(MatrixTerminal matrixTerminal) {
            List<ItemSlot> matrixInventory = matrixTerminal.Controller.getInventory();
            GlobalHelper.deleteAllChildren(itemContainer.transform);
            for (int i = 0; i < matrixInventory.Count; i++) {
                GameObject slotUI = null;
                ItemSlot itemSlot = matrixInventory[i];
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
               
            }
            inventoryUI.initalize(matrixInventory);
        }

        public static MatrixTerminalUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalUI").GetComponent<MatrixTerminalUI>();
        }
    }
}

