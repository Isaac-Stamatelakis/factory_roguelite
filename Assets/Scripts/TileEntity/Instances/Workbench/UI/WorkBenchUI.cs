using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using PlayerModule;
using TileEntityModule.Instances.Workbench;
using UnityEngine.UI;

namespace TileEntityModule.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup recipeList;
        [SerializeField] private WorkbenchRecipeElement recipeElementPrefab;
    }
}

