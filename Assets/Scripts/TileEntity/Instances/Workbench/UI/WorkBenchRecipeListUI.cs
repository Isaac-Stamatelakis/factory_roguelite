using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench
{
    public class WorkBenchRecipeListUI : MonoBehaviour
    {
        [SerializeField] private WorkbenchRecipeElement workbenchRecipeElementPrefab;
        [SerializeField] private VerticalLayoutGroup recipeList;
        [SerializeField] private TMP_InputField recipeSearchField;
    }
}
