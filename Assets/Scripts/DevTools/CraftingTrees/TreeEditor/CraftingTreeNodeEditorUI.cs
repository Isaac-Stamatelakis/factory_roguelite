using System;
using DevTools.CraftingTrees.Network;
using DevTools.CraftingTrees.TreeEditor.NodeEditors;
using Recipe;
using Recipe.Processor;
using TMPro;
using TMPro.Examples;
using UI;
using UI.GeneralUIElements;
using UI.NodeNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.TreeEditor
{
    internal class CraftingTreeNodeEditorUI : MonoBehaviour, ITreeGenerationListener
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TextMeshProUGUI mInputText;
        [SerializeField] private TextMeshProUGUI mOutputText;
        [SerializeField] private Button mDeleteButton;
        [SerializeField] private Transform nodeContentContainer;
        [SerializeField] private Transform emptyContent;
        [SerializeField] private GameObject baseNodeContent;
        [SerializeField] private CraftingNodeItemEditorUI mItemEditorPrefab;
        [SerializeField] private FormattedInputFieldUI mFormattedInputFieldPrefab;
        [SerializeField] private GameObject mBlocker;

        public void OnStatusChange(bool generationStatus)
        {
            mBlocker.gameObject.SetActive(generationStatus);
        }
        public void Initialize(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeNodeNetworkUI nodeNetworkUI, bool openSearchInstantly)
        {
            baseNodeContent.SetActive(true);
            emptyContent.gameObject.SetActive(false);
            mDeleteButton.onClick.RemoveAllListeners();
            gameObject.SetActive(true);
            mDeleteButton.onClick.AddListener(DeletePress);
            mTitleText.text = GlobalHelper.AddSpaces(node.NodeType.ToString());
            mOutputText.text = $"Outputs:{GetOutputs()}";
            mInputText.text = $"Input:{node.NetworkData.InputIds.Count}";
            GlobalHelper.DeleteAllChildren(nodeContentContainer);
            
            CraftingNodeItemEditorUI craftingTreeNodeEditorUI = GameObject.Instantiate(mItemEditorPrefab, nodeContentContainer, false);
            craftingTreeNodeEditorUI.Display(node,nodeNetworkUI.CraftingTreeGeneratorUI,openSearchInstantly);
            
            InitializeInputFields();
            mBlocker.gameObject.SetActive(nodeNetwork.HasGeneratedRecipes());
            return;

            

            void InitializeInputFields()
            {
                switch (node.NodeType)
                {
                    case CraftingTreeNodeType.Item:
                        if (node.NetworkData.InputIds.Count > 0)
                        {
                            ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                            FormattedInputFieldUI chanceInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            chanceInput.DisplayFloat("Chance",itemNodeData.Odds, (value) =>
                            {
                                itemNodeData.Odds = value;
                            },min:0,max:1);
                            CanvasController.Instance.AddTypingListener(chanceInput.InputField);
                        }
                        
                        break;
                    case CraftingTreeNodeType.Transmutation:
                        break;
                    case CraftingTreeNodeType.Processor:
                        ProcessorNodeData recipeMetaData = (ProcessorNodeData)node.NodeData;
                        if (recipeMetaData.RecipeData == null) return;
                        if (recipeMetaData.RecipeData is PassiveRecipeMetaData passiveRecipeMetaData)
                        {
                            FormattedInputFieldUI tickInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            tickInput.DisplayUInt("Ticks", (uint)passiveRecipeMetaData.Ticks, (value) =>
                            {
                                passiveRecipeMetaData.Ticks = (int)value;
                            });
                            CanvasController.Instance.AddTypingListener(tickInput.InputField);
                        }
                        if (recipeMetaData.RecipeData is BurnerRecipeMetaData burnerRecipeMetaData)
                        {
                            FormattedInputFieldUI passiveSpeedInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            passiveSpeedInput.DisplayFloat("Passive Speed",burnerRecipeMetaData.PassiveSpeed, (value) =>
                            {
                                burnerRecipeMetaData.PassiveSpeed = value;
                            },min:0,max:1);
                            CanvasController.Instance.AddTypingListener(passiveSpeedInput.InputField);
                        }
                        if (recipeMetaData.RecipeData is GeneratorItemRecipeMetaData generatorItemRecipeMetaData)
                        {
                            FormattedInputFieldUI energyProduction = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            energyProduction.DisplayULong("Energy Per Tick", generatorItemRecipeMetaData.EnergyPerTick, (value) =>
                            {
                                generatorItemRecipeMetaData.EnergyPerTick = value;
                            });
                            CanvasController.Instance.AddTypingListener(energyProduction.InputField);
                        }

                        if (recipeMetaData.RecipeData is ItemEnergyRecipeMetaData itemEnergyRecipeMetaData)
                        {
                            FormattedInputFieldUI totalEnergyInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            totalEnergyInput.DisplayULong("Total Energy Cost", itemEnergyRecipeMetaData.TotalInputEnergy, (value) =>
                            {
                                itemEnergyRecipeMetaData.TotalInputEnergy = value;
                            });
                            CanvasController.Instance.AddTypingListener(totalEnergyInput.InputField);
                            
                            FormattedInputFieldUI minEnergyPerTick = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            minEnergyPerTick.DisplayULong("Min Energy Per Tick", itemEnergyRecipeMetaData.MinimumEnergyPerTick, (value) =>
                            {
                                itemEnergyRecipeMetaData.MinimumEnergyPerTick = value;
                            });
                            CanvasController.Instance.AddTypingListener(minEnergyPerTick.InputField);
                        }
                      
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            void DeletePress()
            {
                nodeNetworkUI.DeleteNode(node);
                baseNodeContent.SetActive(false);
                emptyContent.gameObject.SetActive(true);
                GlobalHelper.DeleteAllChildren(nodeContentContainer);
            }

            int GetOutputs()
            {
                int count = 0;
                int currentId = node.GetId();
                foreach (CraftingTreeGeneratorNode otherNode in nodeNetwork.Nodes)
                {
                    if (otherNode.GetId() == currentId) continue;
                    if (otherNode.NetworkData.InputIds.Contains(currentId)) count++;
                }

                return count;
            }
        }
    }
}
