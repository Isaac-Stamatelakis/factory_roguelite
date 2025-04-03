using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using Recipe.Viewer;
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
        [SerializeField] private GridLayoutGroup mGridLayoutGroup;
        [SerializeField] private RecipeListElement mRecipeListElement;
        private bool elementsVisible = true;
        private List<DisplayableRecipe> recipes;
        private List<RecipeListElement> recipeListElements;
        public List<RecipeListElement> RecipeListElements => recipeListElements;
        public void Display(RecipeLookUpList recipeLookUpList, string headerName, int mode, List<DisplayableRecipe> recipes)
        {
            GlobalHelper.DeleteAllChildren(mGridLayoutGroup.transform);
            this.mHeaderText.text = headerName;
            mToggleButton.onClick.AddListener(() =>
            {
                elementsVisible = !elementsVisible;
                DisplayToggleButton();
                mGridLayoutGroup.gameObject.SetActive(elementsVisible);
            });
                

            void ClickAction(int index)
            {
                recipeLookUpList.Select(mode, index);
            }
            this.recipes = recipes;
            recipeListElements = new List<RecipeListElement>();
            for (var index = 0; index < recipes.Count; index++)
            {
                var displayableRecipe = recipes[index];
                RecipeListElement recipeListElement = Instantiate(mRecipeListElement, mGridLayoutGroup.transform);
                if (displayableRecipe is ItemDisplayableRecipe itemDisplayableRecipe)
                {
                    ItemSlot output = itemDisplayableRecipe.SolidOutputs[0];
                    recipeListElement.Display(new List<ItemSlot> { output }, displayableRecipe.RecipeData.Recipe, ClickAction, index, output?.itemObject?.name);
                }

                if (displayableRecipe is TransmutationDisplayableRecipe transmutationDisplayableRecipe)
                {
                    recipeListElement.Display(transmutationDisplayableRecipe.Outputs, 
                        displayableRecipe.RecipeData.Recipe, ClickAction,index,transmutationDisplayableRecipe.RecipeData.Recipe.name);
                }
                recipeListElements.Add(recipeListElement);
            }
        }

        public DisplayableRecipe GetRecipe(int index)
        {
            return recipes[index];
        }

        public void ToggleCollection(bool state)
        {
            elementsVisible = state;
            mGridLayoutGroup.gameObject.SetActive(state);
        }

        private void DisplayToggleButton()
        {
            float rotation = elementsVisible ? 0 : 180f;
            mToggleButton.transform.Rotate(0f, 0f, rotation);
        }
        
    }
}
