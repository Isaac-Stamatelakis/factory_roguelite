using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using PlayerModule;
using TileEntity.Instances.Workbench;
using UnityEngine.UI;

namespace TileEntity.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup recipeList;
        [SerializeField] private WorkbenchRecipeElement recipeElementPrefab;
    }
}

