using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.Network;
using Recipe.Objects;
using Recipe.Processor;
using RecipeModule;
using TileEntity;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.TreeEditor
{
    
    internal class CraftingTreeSettingEditorUI : MonoBehaviour
    {
        [SerializeField] private Button mItemButton;
        [SerializeField] private Button mTransmutationButton;
        [SerializeField] private Button mProcessorButton;
        [SerializeField] private Color highlightColor;
        [SerializeField] private TMP_Dropdown mTierDropDown;
        [SerializeField] private TMP_Dropdown mTransmutationEfficiencyDropDown;
        [SerializeField] private TextMeshProUGUI mEnergyBalanceText;
        [SerializeField] private Button mGenerateButton;
        private CraftingTreeNodeType generateNodeType;
        private Button currentHighlightButton;
        private CraftingTreeGenerator craftingTreeGenerator;
        private CraftingTreeNodeNetwork network;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchCraftingType(mItemButton, CraftingTreeNodeType.Item);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchCraftingType(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchCraftingType(mProcessorButton, CraftingTreeNodeType.Processor);
            }
        }

        private void SwitchCraftingType(Button button, CraftingTreeNodeType type)
        {
            if (generateNodeType == type) return;
            generateNodeType = type;
            if (currentHighlightButton)
            {
                currentHighlightButton.GetComponent<Image>().color = button.GetComponent<Image>().color;
            }
            currentHighlightButton = button;
            currentHighlightButton.GetComponent<Image>().color = highlightColor;
            craftingTreeGenerator.SetType(type);
        }

        public void Initialize(CraftingTreeNodeNetwork craftingTreeNodeNetwork, CraftingTreeGenerator treeGenerator)
        {
            this.network = craftingTreeNodeNetwork;
            craftingTreeGenerator = treeGenerator;
            void InitializeNodeButton(Button button, CraftingTreeNodeType type)
            {
                if (generateNodeType == type)
                {
                    button.GetComponent<Image>().color = highlightColor;
                    currentHighlightButton = button;
                    treeGenerator.SetType(type);
                }
                button.onClick.AddListener(() =>
                {
                    SwitchCraftingType(button, type);
                });
            }

            InitializeNodeButton(mItemButton, CraftingTreeNodeType.Item);
            InitializeNodeButton(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            InitializeNodeButton(mProcessorButton, CraftingTreeNodeType.Processor);
            mTierDropDown.options = GlobalHelper.EnumToDropDown<Tier>();
            mTransmutationEfficiencyDropDown.options = GlobalHelper.EnumToDropDown<TransmutationEfficency>();
            mGenerateButton.onClick.AddListener(GenerateRecipes);
        }

        private void GenerateRecipes()
        {
#if UNITY_EDITOR
            Dictionary<int, CraftingTreeGeneratorNode> nodeDictionary = new Dictionary<int, CraftingTreeGeneratorNode>();
            foreach (var node in network.Nodes)
            {
                nodeDictionary[node.GetId()] = node;
            }
            
            foreach (var node in network.Nodes)
            {
                if (node == null || node.NodeType != CraftingTreeNodeType.Processor) continue;
                ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                string assetPath = AssetDatabase.GUIDToAssetPath(processorNodeData.ProcessorGuid);
                RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(assetPath);
                if (recipeProcessor == null) continue;
                string recipePath = AssetDatabase.GUIDToAssetPath(processorNodeData.RecipeGuid);
                RecipeObject recipeObject = AssetDatabase.LoadAssetAtPath<RecipeObject>(recipePath);
                const int MODE = 0; // TODO LET USER ASSIGN MODE
                List<RecipeObject> recipes = recipeProcessor.RecipeCollections[MODE].RecipeCollection.Recipes;
                if (recipeProcessor.RecipeCollections.Count < MODE)
                {
                    Debug.LogWarning($"Recipe Processor {recipeProcessor.name} does not have a mode '{MODE}'");
                    continue;
                }
                if (recipeObject && !RecipeUtils.CurrentValid(recipeObject, recipeProcessor.RecipeType))
                {
                    if (recipes.Contains(recipeObject))
                    {
                        recipes.Remove(recipeObject);
                    }
                    GameObject.Destroy(recipeObject);
                    recipeObject = null;
                }
                if (!recipeObject)
                {
                    recipeObject = RecipeUtils.GetNewRecipeObject(recipeProcessor.RecipeType, null);
                }
                
                List<int> inputIds = node.NetworkData.InputIds;
                foreach (int inputId in inputIds)
                {
                    var inputNode = nodeDictionary.GetValueOrDefault(inputId);
                    if (inputNode == null || inputNode.NodeType != CraftingTreeNodeType.Item) continue;
                    
                }

            }
#endif
        }
    }
}
