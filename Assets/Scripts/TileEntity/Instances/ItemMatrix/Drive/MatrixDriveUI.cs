using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using Items.Inventory;
using Items.Tags;


namespace TileEntity.Instances.Matrix {
    public class MatrixDriveUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup inventoryList;
        [SerializeField] private InventoryUI tagRestrictedInventoryUI;
        private MatrixDriveInstance matrixDrive;
        public void init(int rows, int columns, List<ItemSlot> inventory, MatrixDriveInstance matrixDrive) {
            this.matrixDrive = matrixDrive;
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    GameObject slot = GlobalHelper.instantiateFromResourcePath("UI/Matrix/Drive/MatrixDriveInventorySlot");
                    slot.name = "slot"+  (y*columns+x);
                    slot.transform.SetParent(inventoryList.transform,false);
                }
            }
            inventoryList.constraintCount=columns;
            tagRestrictedInventoryUI.DisplayInventory(inventory);
            tagRestrictedInventoryUI.AddListener(matrixDrive);
        }

        public static MatrixDriveUI createInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Drive/MatrixDriveUI").GetComponent<MatrixDriveUI>();
        }

        public void inventoryUpdate()
        {
            
        }
    }
}

