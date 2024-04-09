using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;
using ItemModule.Inventory;
using ItemModule.Tags.Matrix;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminalUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private GridLayoutGroup itemContainer;
        [SerializeField] private ScrollRect itemContainerScroll;
        [SerializeField] private MatrixTerminalInventoryUI inventoryUI;

        public void init(MatrixTerminal matrixTerminal) {
            inventoryUI.init(matrixTerminal.Controller,itemContainer.transform);
        }

        public static MatrixTerminalUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalUI").GetComponent<MatrixTerminalUI>();
        }
    }
}

