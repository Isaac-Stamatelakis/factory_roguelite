using System;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using TileEntity.Instances.WorkBench;
using TileEntity.Instances.Workbench.UI.RecipeList;
using TileEntity.Instances.WorkBenchs;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI
{
    public class RecipeLookUpList : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mContentList;
        [SerializeField] private TMP_InputField mSearchField;
        [SerializeField] private TMP_Dropdown mCollectionDropdown;
        [SerializeField] private Button mResetButton;
        [SerializeField] private RecipeListHeader headerPrefab;
        [SerializeField] private RecipeListElement listElementPrefab;
        [SerializeField] private Color highLightColor;
        private Dictionary<string, List<RecipeListElement>> itemNameElementDict;
        private Dictionary<int, List<DisplayableRecipe>> modeRecipes;
        private Dictionary<int, (RecipeListHeader, List<RecipeListElement>)> modeElementDict;
        private List<int> orderedModes;
        private int currentMode = -1;
        private int currentIndex = -1;
        private Color defaultElementColor;
        private IRecipeProcessorUI recipeProcessorUI;
        private WorkBenchData workBenchData;
       
        public void Initialize(RecipeProcessorInstance recipeProcessor, IRecipeProcessorUI recipeProcessorUI, WorkBenchData workBenchData)
        {
            this.workBenchData = workBenchData;
            GlobalHelper.DeleteAllChildren(mContentList.transform);
            modeRecipes = recipeProcessor.GetRecipesToDisplayByMode();
            mSearchField.text = this.workBenchData.CurrentSearch;
            mSearchField.onValueChanged.AddListener(FilterResults);
            
            modeElementDict = new Dictionary<int, (RecipeListHeader, List<RecipeListElement>)>();
            itemNameElementDict = new Dictionary<string, List<RecipeListElement>>();
            foreach (var (mode, recipes) in modeRecipes)
            {
                List<RecipeListElement> elements = new List<RecipeListElement>();
                string modeName = recipeProcessor.GetModeName(mode);
                RecipeListHeader header = Instantiate(headerPrefab, mContentList.transform);
                header.Display(this,modeName,mode);
                foreach (var displayableRecipe in recipes)
                {
                    int index = elements.Count;
                    InitializeRecipeElement(displayableRecipe, header, mode, index,elements);
                }
                modeElementDict[mode] = (header,elements);
            }

            defaultElementColor = listElementPrefab.GetComponent<Image>().color;
            this.recipeProcessorUI = recipeProcessorUI;
            
            InitializeDropdown(recipeProcessor);
            mResetButton.onClick.AddListener(ResetCategories);
            
            Select(0,0);
            FilterResults(this.workBenchData.CurrentSearch);
        }

        private void InitializeDropdown(RecipeProcessorInstance recipeProcessorInstance)
        {
            orderedModes = modeElementDict.Keys.ToList();
            orderedModes.Sort();
            mCollectionDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (int mode in orderedModes)
            {
                options.Add(new TMP_Dropdown.OptionData(recipeProcessorInstance.GetModeName(mode)));
            }
            mCollectionDropdown.options = options;
            mCollectionDropdown.onValueChanged.AddListener(SelectCategory);
        }

        private void SelectCategory(int index)
        {
            int selectedMode = orderedModes[index];
            foreach (var (mode, elements) in modeElementDict)
            {
                bool setActive = mode == selectedMode;
                var (header, elementList) = elements;
                header.gameObject.SetActive(setActive);
                foreach (RecipeListElement recipeElement in elementList)
                {
                    bool newState = setActive && recipeElement.Filter(workBenchData.CurrentSearch);
                    recipeElement.gameObject.SetActive(newState);
                }
            }
        }

        private void ResetCategories()
        {
            foreach (var (mode, elements) in modeElementDict)
            {
                var (header, elementList) = elements;
                header.gameObject.SetActive(true);
                foreach (RecipeListElement recipeElement in elementList)
                {
                    bool newState = recipeElement.Filter(workBenchData.CurrentSearch);
                    recipeElement.gameObject.SetActive(newState);
                }
            }
        }
        private void FilterResults(string text)
        {
            workBenchData.CurrentSearch = text;
            foreach (var (itemName, elementList) in itemNameElementDict)
            {
                bool passesFilter = itemName.ToLower().Contains(text.ToLower());
                foreach (var element in elementList)
                {
                    if (!element.HeaderActive) continue;
                    element.gameObject.SetActive(passesFilter);
                }
            }

            foreach (var (mode, values) in modeElementDict)
            {
                //var (header, elementList) = values;
                //header.gameObject.SetActive(!HeaderElementsAllInactive(elementList));
            }
        }

        private bool HeaderElementsAllInactive(List<RecipeListElement> elements)
        {
            foreach (var element in elements)
            {
                if (element.gameObject.activeInHierarchy) return false;
            }

            return true;
        } 

        private void InitializeRecipeElement(DisplayableRecipe displayableRecipe, RecipeListHeader header, int mode, int index, List<RecipeListElement> elements)
        {
            RecipeListElement recipeListElement = Instantiate(listElementPrefab, mContentList.transform);
            string itemName = string.Empty;
            if (displayableRecipe is ItemDisplayableRecipe itemDisplayableRecipe)
            {
                ItemSlot output = itemDisplayableRecipe.SolidOutputs[0];
                recipeListElement.Display(new List<ItemSlot>{output}, displayableRecipe.RecipeData.Recipe,this,header,mode,index);
                itemName = output.itemObject.name;
            }

            if (displayableRecipe is TransmutationDisplayableRecipe transmutationDisplayableRecipe)
            {
                recipeListElement.Display(transmutationDisplayableRecipe.Outputs, displayableRecipe.RecipeData.Recipe,this,header,mode,index);
                itemName = transmutationDisplayableRecipe.RecipeData.Recipe.name;
            }

            if (!itemNameElementDict.ContainsKey(itemName))
            {
                itemNameElementDict[itemName] = new List<RecipeListElement>();
            }
            itemNameElementDict[itemName].Add(recipeListElement);
            elements.Add(recipeListElement);
        }
        public RecipeObject GetCurrentRecipe()
        {
            return modeRecipes[currentMode][currentIndex].RecipeData.Recipe;
        }
        public void Select(int mode, int index)
        {
            if (mode == currentMode && index == currentIndex) return;
            if (!modeElementDict.ContainsKey(mode)) return;
            var (header, recipeElements) = modeElementDict[mode];
            if (recipeElements.Count < 0 || index >= recipeElements.Count) return;
            recipeElements[index].SetColor(highLightColor);
            if (currentIndex >= 0 && currentMode >= 0)
            {
                modeElementDict[currentMode].Item2[currentIndex].SetColor(defaultElementColor);
            }
            currentIndex = index;
            currentMode = mode;
            recipeProcessorUI.DisplayRecipe(modeRecipes[mode][index]);
        }

        public void ToggleHeader(int mode)
        {
            var (header, recipeElements) = modeElementDict[mode];
            if (recipeElements.Count == 0) return;
            bool toggleState = header.ElementsVisible;
            foreach (RecipeListElement recipeElement in recipeElements)
            {
                bool newState = toggleState && recipeElement.Filter(workBenchData.CurrentSearch);
                recipeElement.gameObject.SetActive(newState);
            }
        }

        public void OnDestroy()
        {
            
        }
    }
}
