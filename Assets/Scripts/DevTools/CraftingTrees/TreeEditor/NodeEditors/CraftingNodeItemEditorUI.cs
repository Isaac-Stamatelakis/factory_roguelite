using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.Network;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Transmutable;
using Recipe;
using Recipe.Data;
using TileEntity.Instances.WorkBenchs;
using TMPro;
using UI;
    
#if UNITY_EDITOR
using Recipe.Processor;
using Recipe.Viewer;
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
        private List<ItemObject> displaySlots;
        public void Display(CraftingTreeGeneratorNode node, CraftingTreeGeneratorUI generatorUI, bool openSearchInstantly)
        {
            mInventoryUI.SetInteractMode(InventoryInteractMode.OverrideAction);
            mInventoryUI.OverrideClickAction(ClickOverride);
            displaySlots = GetDisplayInformation(node);
            if (openSearchInstantly)
            {
                ClickOverride(PointerEventData.InputButton.Left,0);
            }
            
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
                ItemSlot itemSlot = node.NodeData.GetDisplaySlot();
                SerializedItemSlot serializedItemSlot = new SerializedItemSlot(itemSlot?.itemObject?.id, itemSlot?.amount ?? 0, null);
                serializedItemSlotEditorUI.Initialize(serializedItemSlot,OnItemChange,parameters,itemRestrictions:displaySlots);
                
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
                        

                        if (itemObject is TileItem { tileEntity: IProcessorTileEntity processorTileEntity })
                        {
                            string assetPath = AssetDatabase.GetAssetPath(processorTileEntity.GetRecipeProcessor());
                            processorNodeData.ProcessorGuid = AssetDatabase.AssetPathToGUID(assetPath);
                            switch (processorTileEntity.GetRecipeProcessor().RecipeType)
                            {
                                case RecipeType.Item:
                                    processorNodeData.RecipeData = null;
                                    break;
                                case RecipeType.Passive:
                                    processorNodeData.RecipeData = new PassiveRecipeMetaData();
                                    break;
                                case RecipeType.Generator:
                                    processorNodeData.RecipeData = new GeneratorItemRecipeMetaData();
                                    break;
                                case RecipeType.Machine:
                                    processorNodeData.RecipeData = new ItemEnergyRecipeMetaData();
                                    break;
                                case RecipeType.Burner:
                                    processorNodeData.RecipeData = new BurnerRecipeMetaData();
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
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
                ItemSlot itemSlot = node.NodeData.GetDisplaySlot();
                mTitleText.text = ItemSlotUtils.IsItemSlotNull(itemSlot) ? "Null" : itemSlot.itemObject.name;
                mInventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},clear:false);
            }
        }

        private List<ItemObject> GetDisplayInformation(CraftingTreeGeneratorNode node)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            List<ItemObject> itemObjects;
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                    itemObjects = itemRegistry.GetAllItems();
                    break;
                case CraftingTreeNodeType.Transmutation:
                    itemObjects = new List<ItemObject>();
                    List<TransmutableItemObject> transmutableItemObjects = itemRegistry.GetAllItemsOfType<TransmutableItemObject>();
                    const string DEFAULT_MATERIAL = "Iron";
                    foreach (TransmutableItemObject transmutable in transmutableItemObjects)
                    {
                        if (transmutable?.getMaterial()?.name != DEFAULT_MATERIAL) continue;
                        itemObjects.Add(transmutable);
                    }
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

            return itemObjects;
        }
    }
}
