using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemModule.Inventory;
using ItemModule.Tags;


namespace TileEntityModule.Instances.Matrix {
    public class MatrixDriveUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup inventoryList;
        [SerializeField] private TagRestrictedInventoryUI tagRestrictedInventoryUI;

        public void init(int rows, int columns, List<ItemSlot> inventory) {
            for (int x = 0; x < columns; x++) {
                for (int y = 0; y < rows; y++) {
                    GameObject slot = GlobalHelper.instantiateFromResourcePath("UI/Matrix/Drive/MatrixDriveInventorySlot");
                    slot.name = "slot"+  (y*columns+x);
                    slot.transform.SetParent(inventoryList.transform,false);
                }
            }
            inventoryList.constraintCount=columns;
            tagRestrictedInventoryUI.initalize(inventory,ItemTag.StorageDrive);
        }

        public static MatrixDriveUI createInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Drive/MatrixDriveUI").GetComponent<MatrixDriveUI>();
        }
    }
}

