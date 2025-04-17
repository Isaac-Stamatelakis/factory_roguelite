using System;
using DevTools.CraftingTrees.Network;
using DevTools.CraftingTrees.TreeEditor.NodeEditors;
using TMPro;
using TMPro.Examples;
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

        public void Initialize(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeNodeNetworkUI nodeNetworkUI)
        {
            gameObject.SetActive(true);
            mDeleteButton.onClick.AddListener(DeletePress);
            mTitleText.text = GlobalHelper.AddSpaces(node.NodeType.ToString());
            mOutputText.text = $"Outputs:{node.NetworkData.InputIds.Count}";
            mInputText.text = $"Input:?";
            GlobalHelper.DeleteAllChildren(nodeContentContainer);
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                    CraftingNodeItemEditorUI craftingTreeNodeEditorUI = GameObject.Instantiate(mItemEditorPrefab, nodeContentContainer, false);
                    craftingTreeNodeEditorUI.Display((ItemNodeData)node.NodeData,nodeNetworkUI.CraftingTreeGeneratorUI);
                    break;
                case CraftingTreeNodeType.Transmutation:
                    break;
                case CraftingTreeNodeType.Processor:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return;

            void DeletePress()
            {
                int nodeId = node.GetId();
                for (var index = 0; index < nodeNetwork.Nodes.Count; index++)
                {
                    var otherNode = nodeNetwork.Nodes[index];
                    if (nodeId != otherNode.GetId()) continue;
                    nodeNetwork.Nodes.RemoveAt(index);
                    nodeNetworkUI.CraftingTreeGeneratorUI.Rebuild();
                    gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}
