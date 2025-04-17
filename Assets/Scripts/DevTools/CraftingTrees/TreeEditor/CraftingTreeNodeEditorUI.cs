using System;
using DevTools.CraftingTrees.Network;
using DevTools.CraftingTrees.TreeEditor.NodeEditors;
using TMPro;
using TMPro.Examples;
using UI.GeneralUIElements;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.TreeEditor
{
    internal class CraftingTreeNodeEditorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TextMeshProUGUI mInputText;
        [SerializeField] private TextMeshProUGUI mOutputText;
        [SerializeField] private Button mDeleteButton;
        [SerializeField] private Transform nodeContentContainer;
        
        [SerializeField] private CraftingNodeItemEditorUI mItemEditorPrefab;
        [SerializeField] private FormattedInputFieldUI mFormattedInputFieldPrefab;

        public void Initialize(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeNodeNetworkUI nodeNetworkUI)
        {
            mDeleteButton.onClick.RemoveAllListeners();
            gameObject.SetActive(true);
            mDeleteButton.onClick.AddListener(DeletePress);
            mTitleText.text = GlobalHelper.AddSpaces(node.NodeType.ToString());
            mOutputText.text = $"Outputs:{GetOutputs()}";
            mInputText.text = $"Input:{node.NetworkData.InputIds.Count}";
            GlobalHelper.DeleteAllChildren(nodeContentContainer);
            
            CraftingNodeItemEditorUI craftingTreeNodeEditorUI = GameObject.Instantiate(mItemEditorPrefab, nodeContentContainer, false);
            craftingTreeNodeEditorUI.Display(node,nodeNetworkUI.CraftingTreeGeneratorUI);

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
                    }
                    break;
                case CraftingTreeNodeType.Transmutation:
                    break;
                case CraftingTreeNodeType.Processor:
                    ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                  
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return;

            void DeletePress()
            {
                nodeNetworkUI.DeleteNode(node);
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
