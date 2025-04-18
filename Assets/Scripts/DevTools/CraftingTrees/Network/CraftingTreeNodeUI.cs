using System;
using Item.Slot;
using Items;
using Items.Transmutable;
using Recipe.Processor;
using UI.NodeNetwork;
#if  UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.Network
{
    public interface IOnSelectActionNodeUI
    {
        public void OnSelect();
    }
    internal class CraftingTreeNodeUI : NodeUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetworkUI>, IOnSelectActionNodeUI
    {
        [SerializeField] private NodeSprites nodeSprites;
        public override void DisplayImage()
        {
            Image image = GetComponent<Image>();
            mItemSlotUI.Display(node.NodeData.GetDisplaySlot());
            image.sprite = node.NodeType switch
            {
                CraftingTreeNodeType.Item => nodeSprites.ItemBackground,
                CraftingTreeNodeType.Transmutation => nodeSprites.TransmutableBackground,
                CraftingTreeNodeType.Processor => nodeSprites.ProcessorBackground,
                _ => throw new ArgumentOutOfRangeException(nameof(node.NodeType), $"Unknown node type: {node.NodeType}")
            };
        }
        

        public override void OpenContent(NodeUIContentOpenMode contentOpenMode)
        {
            bool generated = nodeNetwork.NodeNetwork.HasGeneratedRecipes();
            if (contentOpenMode == NodeUIContentOpenMode.Click && Input.GetKey(KeyCode.LeftShift) && !generated)
            {
                nodeNetwork.SelectNode(this);
                return;
            }

            OpenSideView(contentOpenMode == NodeUIContentOpenMode.KeyPress && !generated);

        }

        private void OpenSideView(bool openInstantly)
        {
            nodeNetwork.CraftingTreeGeneratorUI?.NodeEditorUI?.Initialize(node,nodeNetwork.NodeNetwork,nodeNetwork,openSearchInstantly: openInstantly);
        }

        [System.Serializable]
        private class NodeSprites
        {
            public Sprite ItemBackground;
            public Sprite TransmutableBackground;
            public Sprite ProcessorBackground;
        }

        public void OnSelect()
        {
            OpenSideView(false);
        }
    }
}
