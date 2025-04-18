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
    internal class CraftingTreeNodeUI : NodeUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetworkUI>
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
        

        protected override void openContent(PointerEventData eventData)
        {
            bool generated = nodeNetwork.NodeNetwork.HasGeneratedRecipes();
            Debug.Log(generated);
            bool leftClick = eventData.button == PointerEventData.InputButton.Left;
            if (leftClick && Input.GetKey(KeyCode.LeftShift) && !generated)
            {
                nodeNetwork.SelectNode(this);
                return;
            }
            nodeNetwork.CraftingTreeGeneratorUI?.NodeEditorUI?.Initialize(node,nodeNetwork.NodeNetwork,nodeNetwork,openSearchInstantly: !leftClick && !generated);
        }

        [System.Serializable]
        private class NodeSprites
        {
            public Sprite ItemBackground;
            public Sprite TransmutableBackground;
            public Sprite ProcessorBackground;
        }
    }
}
