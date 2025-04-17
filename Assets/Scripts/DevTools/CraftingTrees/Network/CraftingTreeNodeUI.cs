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

namespace DevTools.CraftingTrees.Network
{
    internal class CraftingTreeNodeUI : NodeUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetworkUI>
    {
        public override void DisplayImage()
        {
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                {
                    ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                    ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemNodeData.SerializedItemSlot);
                    mItemSlotUI.Display(itemSlot);
                    break;
                }
                    
                case CraftingTreeNodeType.Transmutation:
                {
                    TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                    TransmutableItemObject transmutableItemObject = TransmutableItemUtils.GetDefaultObjectOfState(transmutationNodeData.OutputState);
                    ItemSlot itemSlot = new ItemSlot(transmutableItemObject, 1, null);
                    mItemSlotUI.Display(itemSlot);
                    break;
                }
                case CraftingTreeNodeType.Processor:
#if  UNITY_EDITOR
                {
                    ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                    string path = AssetDatabase.GUIDToAssetPath(processorNodeData.ProcessorGuid);
                    RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(path);
                    ItemSlot itemSlot = new ItemSlot(recipeProcessor?.DisplayImage, 1, null);
                    mItemSlotUI.Display(itemSlot);
                }
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void openContent()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                nodeNetwork.SelectNode(this);
                return;
            }
            
            nodeNetwork.CraftingTreeGeneratorUI?.NodeEditorUI?.Initialize(node,nodeNetwork.NodeNetwork,nodeNetwork);
        }
    }
}
