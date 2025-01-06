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

        public void Display(RecipeLookUpList recipeLookUpList, string headerName, int mode)
        {
            this.mHeaderText.text = headerName;
            mToggleButton.onClick.AddListener(() =>
            {
                ToggleButtonImage();
                recipeLookUpList.ToggleHeader(mode);
            });
        }

        private void ToggleButtonImage()
        {
            // Might have to change this later
            mToggleButton.transform.Rotate(0f, 0f, 180f);
        }
        
    }
}
