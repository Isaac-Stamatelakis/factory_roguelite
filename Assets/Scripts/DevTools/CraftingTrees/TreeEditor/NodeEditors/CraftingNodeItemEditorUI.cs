using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.Network;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Transmutable;
using TileEntity.Instances.WorkBenchs;
using TMPro;
using UI;
    
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

namespace DevTools.CraftingTrees.TreeEditor.NodeEditors
{
    internal class CraftingNodeItemEditorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUI;
        private DisplayInformation displayInformation;
        public void Display(CraftingTreeGeneratorNode node, CraftingTreeGeneratorUI generatorUI)
        {
            mInventoryUI.SetInteractMode(InventoryInteractMode.OverrideAction);
            mInventoryUI.OverrideClickAction(ClickOverride);
            displayInformation = GetDisplayInformation(node);
            
            DisplayInventory();
            return;

            void ClickOverride(PointerEventData.InputButton inputButton, int index)
            {
                SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(mItemSlotEditorUI);
                SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
                {
                    OnValueChange = OnItemChange,
                    DisplayAmount = true
                };
                displayInformation = GetDisplayInformation(node);
                serializedItemSlotEditorUI.Initialize(displayInformation.SerializedItemSlot,OnItemChange,parameters,itemRestrictions:displayInformation.Options);
                
                CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject,hideParent:false);
            }
            

            void OnItemChange(SerializedItemSlot serializedItemSlot)
            {
                
                switch (node.NodeType)
                {
                    case CraftingTreeNodeType.Item:
                        ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                        itemNodeData.SerializedItemSlot = serializedItemSlot;
                        break;
                    case CraftingTreeNodeType.Transmutation:
                    {
                        ItemRegistry itemRegistry = ItemRegistry.GetInstance();
                        TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                        ItemObject itemObject = itemRegistry.GetItemObject(serializedItemSlot?.id);
                        if (itemObject is TransmutableItemObject transmutableItemObject)
                        {
                            transmutationNodeData.OutputState = transmutableItemObject.getState();
                        }
                        else
                        {
                            transmutationNodeData.OutputState = TransmutableItemState.Ingot; // Default
                        }
                        break;
                    }

                    case CraftingTreeNodeType.Processor:
                    {
                        ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                        ItemRegistry itemRegistry = ItemRegistry.GetInstance();
                        ItemObject itemObject = itemRegistry.GetItemObject(serializedItemSlot?.id);
#if UNITY_EDITOR
                        

                        if (itemObject is TileItem tileItem)
                        {
                            if (tileItem.tileEntity is IProcessorTileEntity processorTileEntity)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(processorTileEntity.GetRecipeProcessor());
                                processorNodeData.ProcessorGuid = AssetDatabase.AssetPathToGUID(assetPath);
                            }
                        }
#endif
                        break;
                    }
                        
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                DisplayInventory();
                generatorUI.Rebuild();
            }

            void DisplayInventory()
            {
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(displayInformation.SerializedItemSlot);
                mTitleText.text = ItemSlotUtils.IsItemSlotNull(itemSlot) ? "Null" : itemSlot.itemObject.name;
                mInventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},clear:false);
            }
        }

        private DisplayInformation GetDisplayInformation(CraftingTreeGeneratorNode node)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            List<ItemObject> itemObjects;
            SerializedItemSlot serializedItemSlot = null;
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                    itemObjects = itemRegistry.GetAllItems();
                    ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                    serializedItemSlot = itemNodeData.SerializedItemSlot;
                    break;
                case CraftingTreeNodeType.Transmutation:
                    TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                    itemObjects = new List<ItemObject>();
                    List<TransmutableItemObject> transmutableItemObjects = itemRegistry.GetAllItemsOfType<TransmutableItemObject>();
                    const string DEFAULT_MATERIAL = "Iron";
                    foreach (TransmutableItemObject transmutable in transmutableItemObjects)
                    {
                        if (transmutable?.getMaterial()?.name != DEFAULT_MATERIAL) continue;
                        itemObjects.Add(transmutable);
                    }
                    TransmutableItemObject transmutableItemObject = TransmutableItemUtils.GetDefaultObjectOfState(transmutationNodeData.OutputState);
                    serializedItemSlot = new SerializedItemSlot(transmutableItemObject?.id, 1, null);
                    break;
                case CraftingTreeNodeType.Processor:
                    itemObjects = new List<ItemObject>();
                    List<TileItem> tileItems = itemRegistry.GetAllItemsOfType<TileItem>();
                    foreach (TileItem tileItem in tileItems)
                    {
                        if (tileItem.tileEntity is not IProcessorTileEntity) continue;
                        itemObjects.Add(tileItem);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node.NodeType), node.NodeType, null);
            }

            return new DisplayInformation
            {
                SerializedItemSlot = serializedItemSlot,
                Options = itemObjects
            };
        }
        private class DisplayInformation
        {
            public SerializedItemSlot SerializedItemSlot;
            public List<ItemObject> Options;
        }
    }
}
