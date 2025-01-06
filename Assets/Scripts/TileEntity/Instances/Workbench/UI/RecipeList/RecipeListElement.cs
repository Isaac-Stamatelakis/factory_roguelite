using Item.Slot;
using Items;
using Recipe.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileEntity.Instances.Workbench.UI.RecipeList
{
    public class RecipeListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ItemSlotUI mItemSlotUI;
        [SerializeField] private TextMeshProUGUI mNameText;
        private RecipeLookUpList recipeLookUpListParent;
        private int mode;
        private int index;

        public void Display(DisplayableRecipe displayableRecipe, RecipeLookUpList recipeLookUpList, int mode, int index)
        {
            if (displayableRecipe is not ItemDisplayableRecipe itemDisplayableRecipe)
            {
                Debug.LogError($"Recipe tried to display non item display recipe: {displayableRecipe.RecipeData.Recipe.name}");
                return;
            }

            if (itemDisplayableRecipe.SolidOutputs.Count == 0)
            {
                Debug.LogError($"Recipe list tried to display empty  recipe: {displayableRecipe.RecipeData.Recipe.name}");
                return;
            }
            
            if (itemDisplayableRecipe.SolidOutputs.Count > 1)
            {
                Debug.LogWarning($"Recipe list recipe has more than one output: {displayableRecipe.RecipeData.Recipe.name}");
            }
            this.mode = mode;
            this.index = index;
            ItemSlot output = itemDisplayableRecipe.SolidOutputs[0];
            mItemSlotUI.Display(output);
            mNameText.text = output.itemObject.name;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            recipeLookUpListParent.Select(mode,index);
        }
    }
}
