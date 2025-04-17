using System;
using Item.Slot;
using Items;
using UI.NodeNetwork;
using UnityEngine;

namespace DevTools.CraftingTrees.Network
{
    internal class CraftingTreeNodeUI : NodeUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetworkUI>
    {
        public override void DisplayImage()
        {
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                    ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                    ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemNodeData.SerializedItemSlot);
                    mItemSlotUI.Display(itemSlot);
                    break;
                case CraftingTreeNodeType.Transmutation:
                    break;
                case CraftingTreeNodeType.Processor:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void openContent()
        {
            nodeNetwork.CraftingTreeGeneratorUI?.NodeEditorUI?.Initialize(node,nodeNetwork.NodeNetwork,nodeNetwork);
        }
    }
}
