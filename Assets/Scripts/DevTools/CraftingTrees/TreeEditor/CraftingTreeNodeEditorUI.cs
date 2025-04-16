using DevTools.CraftingTrees.Network;
using TMPro;
using TMPro.Examples;
using UnityEngine;
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

        public void Initialize(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeGeneratorUI craftingTreeGeneratorUI)
        {
            mDeleteButton.onClick.AddListener(DeletePress);
            mTitleText.text = GlobalHelper.AddSpaces(node.NodeType.ToString());
            mOutputText.text = $"Outputs:{node.NetworkData.InputIds.Count}";
            mInputText.text = $"Input:?";
            return;

            void DeletePress()
            {
                int nodeId = node.GetId();
                for (var index = 0; index < nodeNetwork.Nodes.Count; index++)
                {
                    var otherNode = nodeNetwork.Nodes[index];
                    if (nodeId == otherNode.GetId())
                    {
                        nodeNetwork.Nodes.RemoveAt(index);
                        craftingTreeGeneratorUI.Rebuild();
                        GameObject.Destroy(gameObject);
                        break;
                    }
                }
            }
        }
    }
}
