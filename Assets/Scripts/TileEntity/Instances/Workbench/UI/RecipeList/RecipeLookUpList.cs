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
        private Dictionary<int, RecipeListHeader> modeElementDict;
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
            var modeRecipes = recipeProcessor.GetRecipesToDisplayByMode();
            mSearchField.text = this.workBenchData.CurrentSearch;
            mSearchField.onValueChanged.AddListener(FilterResults);

            modeElementDict = new Dictionary<int, RecipeListHeader>();
            foreach (var (mode, recipes) in modeRecipes)
            {
                string modeName = recipeProcessor.GetModeName(mode);
                RecipeListHeader header = Instantiate(headerPrefab, mContentList.transform);
                header.Display(this,modeName,mode, recipes);
                
                modeElementDict[mode] = header;
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
            /*
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
            */
        }

        private void ResetCategories()
        {
            /*
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
            */
        }
        private void FilterResults(string text)
        {
            workBenchData.CurrentSearch = text;
            foreach (var collection in modeElementDict.Values)
            {
                List<RecipeListElement> recipeListElements = collection.RecipeListElements;
                foreach (RecipeListElement recipeListElement in recipeListElements)
                {
                    bool pass = recipeListElement.Filter(text);
                    recipeListElement.gameObject.SetActive(pass);
                }
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
        
        public RecipeObject GetCurrentRecipe()
        {
            return modeElementDict[currentMode].GetRecipe(currentIndex).RecipeData.Recipe;
        }
        public void Select(int mode, int index)
        {
            if (mode == currentMode && index == currentIndex) return;
            if (!modeElementDict.TryGetValue(mode, out var collection)) return;
            DisplayableRecipe recipe = collection.GetRecipe(index);
            if (recipe == null) return;
            currentIndex = index;
            currentMode = mode;
            recipeProcessorUI.DisplayRecipe(recipe);
        }
    }
}
