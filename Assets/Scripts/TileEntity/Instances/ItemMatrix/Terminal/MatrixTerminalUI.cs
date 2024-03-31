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
    }
}

