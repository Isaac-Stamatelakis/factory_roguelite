using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using Items.Inventory;
using Items.Tags.Matrix;
using PlayerModule;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminalUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private GridLayoutGroup itemContainer;
        [SerializeField] private ScrollRect itemContainerScroll;
        [SerializeField] private MatrixTerminalInventoryUI inventoryUI;
        [SerializeField] private GridLayoutGroup playerInventoryContainer;

        public void init(MatrixTerminalInstance matrixTerminal) {
            inventoryUI.init(matrixTerminal.Controller,itemContainer.transform,this);
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            playerInventory.cloneInventoryUI(playerInventoryContainer.transform);
            playerInventory.hideUI();

        }

        public static MatrixTerminalUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalUI").GetComponent<MatrixTerminalUI>();
        }

        public void OnDestroy() {
            PlayerContainer.getInstance().getInventory().showUI(); 
        }
    }
}

