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

        public void OnStatusChange(bool generated)
        {
            mBlocker.gameObject.SetActive(generated);
        }

        public void Hide()
        {
            GlobalHelper.DeleteAllChildren(nodeContentContainer);
            emptyContent.gameObject.SetActive(true);
            baseNodeContent.SetActive(false);
            nodeContentContainer.gameObject.SetActive(false);
        }
        public void Initialize(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeNodeNetworkUI nodeNetworkUI, bool openSearchInstantly)
        {
            baseNodeContent.SetActive(true);
            emptyContent.gameObject.SetActive(false);
            nodeContentContainer.gameObject.SetActive(true);
            mDeleteButton.onClick.RemoveAllListeners();
            gameObject.SetActive(true);
            mDeleteButton.onClick.AddListener(DeletePress);
            mTitleText.text = GlobalHelper.AddSpaces(node.NodeType.ToString());
            mOutputText.text = $"Outputs:{GetOutputs()}";
            mInputText.text = $"Input:{node.NetworkData.InputIds.Count}";
            GlobalHelper.DeleteAllChildren(nodeContentContainer);
            
            CraftingNodeItemEditorUI craftingTreeNodeEditorUI = GameObject.Instantiate(mItemEditorPrefab, nodeContentContainer, false);
            craftingTreeNodeEditorUI.Display(node,nodeNetworkUI.CraftingTreeGeneratorUI,nodeNetworkUI,openSearchInstantly);
            
            InitializeInputFields();
            mBlocker.gameObject.SetActive(nodeNetwork.HasGeneratedRecipes());
            return;

            

            void InitializeInputFields()
            {
                switch (node.NodeType)
                {
                    case CraftingTreeNodeType.Item:
                        ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                        FormattedInputFieldUI amountInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                        amountInput.DisplayUInt("Amount",itemNodeData.SerializedItemSlot?.amount ?? 1, (value) =>
                        {
                            if (itemNodeData.SerializedItemSlot == null) return;
                            itemNodeData.SerializedItemSlot.amount = value;
                            nodeNetworkUI.RefreshNode(node);
                        },min:1,max:uint.MaxValue);
                        CanvasController.Instance.AddTypingListener(amountInput.InputField);
                        
                        if (node.NetworkData.InputIds.Count > 0)
                        {
                            FormattedInputFieldUI chanceInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            chanceInput.DisplayFloat("Chance",itemNodeData.Odds, (value) =>
                            {
                                itemNodeData.Odds = value;
                                nodeNetworkUI.RefreshNode(node);
                            },min:0,max:1);
                            CanvasController.Instance.AddTypingListener(chanceInput.InputField);
                        }
                        
                        break;
                    case CraftingTreeNodeType.Transmutation:
                        break;
                    case CraftingTreeNodeType.Processor:
                        ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                        FormattedInputFieldUI modeInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                        modeInput.DisplayUInt("Mode", (uint)processorNodeData.Mode, (value) =>
                        {
                            processorNodeData.Mode = (int)value;
                        });
                        CanvasController.Instance.AddTypingListener(modeInput.InputField);
                        
                        if (processorNodeData.RecipeData == null) return;
                        if (processorNodeData.RecipeData is PassiveRecipeMetaData passiveRecipeMetaData)
                        {
                            FormattedInputFieldUI tickInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            tickInput.DisplayFloat("Time (s)", passiveRecipeMetaData.Seconds, (value) =>
                            {
                                passiveRecipeMetaData.Seconds = value;
                            });
                            CanvasController.Instance.AddTypingListener(tickInput.InputField);
                        }
                        if (processorNodeData.RecipeData is BurnerRecipeMetaData burnerRecipeMetaData)
                        {
                            FormattedInputFieldUI passiveSpeedInput = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            passiveSpeedInput.DisplayFloat("Passive Speed",burnerRecipeMetaData.PassiveSpeed, (value) =>
                            {
                                burnerRecipeMetaData.PassiveSpeed = value;
                            },min:0,max:1);
                            CanvasController.Instance.AddTypingListener(passiveSpeedInput.InputField);
                        }
                        if (processorNodeData.RecipeData is GeneratorItemRecipeMetaData generatorItemRecipeMetaData)
                        {
                            FormattedInputFieldUI energyProduction = GameObject.Instantiate(mFormattedInputFieldPrefab, nodeContentContainer, false);
                            energyProduction.DisplayULong("Energy Per Tick", generatorItemRecipeMetaData.EnergyPerTick, (value) =>
                            {
                                generatorItemRecipeMetaData.EnergyPerTick = value;
                            });
                            CanvasController.Instance.AddTypingListener(energyProduction.InputField);
                        }

                        if (processorNodeData.RecipeData is ItemEnergyRecipeMetaData itemEnergyRecipeMetaData)
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
