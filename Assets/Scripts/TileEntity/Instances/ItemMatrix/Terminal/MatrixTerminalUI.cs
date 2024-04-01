using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminalUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private GridLayoutGroup itemContainer;
        [SerializeField] private ScrollRect itemContainerScroll;

        public void init(MatrixTerminal matrixTerminal) {
            List<ItemSlot> matrixInventory = matrixTerminal.Controller.getInventory();
        }

        public static MatrixTerminalUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalUI").GetComponent<MatrixTerminalUI>();
        }
    }
}

