using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI.RecipeList
{
    public class RecipeListHeader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mHeaderText;
        [SerializeField] private Button mToggleButton;
        private int mode;

        public void Display(RecipeLookUpList recipeLookUpList, string headerName, int mode)
        {
            this.mode = mode;
            this.mHeaderText.text = headerName;
        }
        
    }
}
