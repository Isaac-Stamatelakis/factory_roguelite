using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.Network;
using Item.Slot;
using Item.Transmutation;
using Items;
using Items.Inventory;
using Items.Transmutable;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
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
    internal static class CraftingTreeNodeUtils
    {
        public static ItemSlot GetDisplaySlot(CraftingTreeGeneratorNode node, CraftingTreeNodeNetwork nodeNetwork)
        {
            switch (node.NodeType)
            {
                case CraftingTreeNodeType.Item:
                    ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                    return ItemSlotFactory.deseralizeItemSlot(itemNodeData.SerializedItemSlot);
                case CraftingTreeNodeType.Transmutation:
                    TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                    return transmutationNodeData.GetItemSlot(nodeNetwork.TransmutationEfficency);
                case CraftingTreeNodeType.Processor:
                    ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                    ItemSlot itemSlot = null;
#if  UNITY_EDITOR
                    string path = AssetDatabase.GUIDToAssetPath(processorNodeData.ProcessorGuid);
                    RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(path);
                    itemSlot = new ItemSlot(recipeProcessor?.DisplayImage, 1, null);
#endif
                    return itemSlot;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void UpdateTransmutationNode(SerializedItemSlot serializedItemSlot, CraftingTreeNodeNetwork nodeNetwork, CraftingTreeGeneratorNode node)
        {
            if (serializedItemSlot == null) return;
            if (node.NodeType != CraftingTreeNodeType.Transmutation) return;
            TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
            ITransmutableItem currentNewItem = ItemRegistry.GetInstance().GetTransmutableItemObject(serializedItemSlot.id);
            if (currentNewItem == null)
            {
                transmutationNodeData.OutputItemId = null;
                node.NetworkData.InputIds.Clear(); // Can clear because will only ever have one element
                return;
            }
                            
            ITransmutableItem transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(transmutationNodeData.OutputItemId);
            if (transmutableItemObject == null) return;
            
            TransmutableItemMaterial material = currentNewItem.getMaterial();
            if (!material) return;
            
            ITransmutableItem newTransmutableItemObject = TransmutableItemUtils.GetMaterialItem(material, transmutableItemObject.getState());
            transmutationNodeData.OutputItemId = ((ItemObject)newTransmutableItemObject)?.id;
            transmutationNodeData.InputState = currentNewItem.getState();
            transmutationNodeData.InputAmount = serializedItemSlot.amount;
            int id = node.GetId();
            foreach (var otherNode in nodeNetwork.Nodes)
            {
                if (otherNode.NodeType != CraftingTreeNodeType.Transmutation) continue;
                if (!otherNode.NetworkData.InputIds.Contains(id)) continue;
                
                var (inputAmount, _) = TransmutableItemUtils.GetInputOutputAmount(transmutationNodeData.InputState,transmutableItemObject.getState(),nodeNetwork.TransmutationEfficency.Value());
                UpdateTransmutationNode(new SerializedItemSlot(transmutationNodeData.OutputItemId,inputAmount,null),nodeNetwork,otherNode);
            }
        }
    }
    internal class CraftingNodeItemEditorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUI;
        private List<ItemObject> displaySlots;
        private CraftingTreeNodeNetwork nodeNetwork;
        
        public void Display(CraftingTreeGeneratorNode node, CraftingTreeGeneratorUI generatorUI, CraftingTreeNodeNetworkUI network, bool openSearchInstantly)
        {
            this.nodeNetwork = network.NodeNetwork;
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
                // Prevents spamming editors
                GameObject currentTop = CanvasController.Instance.PeekTopGameObject();
                if (currentTop && currentTop.GetComponent<SerializedItemSlotEditorUI>()) return;
                
                SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(mItemSlotEditorUI);
                SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
                {
                    OnValueChange = OnItemChange,
                    RemoveOnSelect = true
                };
                ItemSlot itemSlot = CraftingTreeNodeUtils.GetDisplaySlot(node,nodeNetwork);
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
                        if (serializedItemSlot.amount == 0) serializedItemSlot.amount = 1;
                        int id = node.GetId();
                        bool update = false;
                        foreach (var otherNode in nodeNetwork.Nodes)
                        {
                            if (otherNode.NodeType != CraftingTreeNodeType.Transmutation) continue;
                            if (!otherNode.NetworkData.InputIds.Contains(id)) continue;
                            CraftingTreeNodeUtils.UpdateTransmutationNode(serializedItemSlot, nodeNetwork, otherNode);
                            update = true;
                        }

                        if (update) network.Display();
                        
                        break;
                    case CraftingTreeNodeType.Transmutation:
                    {
                        TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                        transmutationNodeData.OutputItemId = serializedItemSlot?.id;
                        if (node.NetworkData.InputIds.Count == 0)
                        {
                            transmutationNodeData.InputState = TransmutableItemState.Ingot;
                            transmutationNodeData.InputAmount = 1;
                        }
                        else
                        {
                            var childId = node.NetworkData.InputIds[0];
                            foreach (var otherNode in nodeNetwork.Nodes)
                            {
                                if (otherNode.GetId() != childId) continue;
                                if (otherNode.NodeType == CraftingTreeNodeType.Item)
                                {
                                    ItemNodeData otherItemNodeData = (ItemNodeData)otherNode.NodeData;
                                    transmutationNodeData.InputState = TransmutableItemState.Ingot;
                                    transmutationNodeData.InputAmount = otherItemNodeData.SerializedItemSlot?.amount ?? 1;
                                    ITransmutableItem transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(otherItemNodeData.SerializedItemSlot?.id);
                                    if (transmutableItemObject == null) break;
                                    transmutationNodeData.InputState = transmutableItemObject.getState();
                                    break;
                                }

                                if (otherNode.NodeType == CraftingTreeNodeType.Transmutation)
                                {
                                    TransmutationNodeData otherTransmutationNodeData = (TransmutationNodeData)otherNode.NodeData;
                                    ItemSlot itemSlot = otherTransmutationNodeData.GetItemSlot(nodeNetwork.TransmutationEfficency);
                                    transmutationNodeData.InputAmount = itemSlot?.amount ?? 0;
                                }
                                
                                break;
                            }
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
                            if (!processorTileEntity.GetRecipeProcessor()) return;
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
                ItemSlot itemSlot = CraftingTreeNodeUtils.GetDisplaySlot(node,nodeNetwork);
                if (!ItemSlotUtils.IsItemSlotNull(itemSlot)) itemSlot.amount = 1;
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
                    TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                    itemObjects = new List<ItemObject>();
                    List<TransmutableItemObject> transmutableItemObjects = itemRegistry.GetAllItemsOfType<TransmutableItemObject>();
                    TransmutableItemMaterial material = GetMaterial(node);
                    string materialName = material.name;
                    foreach (TransmutableItemObject transmutable in transmutableItemObjects)
                    {
                        if (transmutable?.getMaterial()?.name != materialName) continue;
                        if (transmutable.getState() == transmutationNodeData.InputState) continue;
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

            TransmutableItemMaterial GetMaterial(CraftingTreeGeneratorNode searchNode)
            {
                if (searchNode.NetworkData.InputIds.Count == 0)
                {
                    return ReturnDefault();
                }
                int inputId = searchNode.NetworkData.InputIds[0];
                
                CraftingTreeGeneratorNode inputNode = null;
                foreach (var otherNode in nodeNetwork.Nodes)
                {
                    if (otherNode.GetId() == inputId)
                    {
                        inputNode = otherNode;
                        break;
                    }
                }
                if (inputNode == null)
                {
                    return ReturnDefault();
                }

                if (inputNode.NodeType == CraftingTreeNodeType.Transmutation)
                {
                    return GetMaterial(inputNode);
                }
                if (inputNode.NodeType != CraftingTreeNodeType.Item) return ReturnDefault();
                ItemNodeData itemNodeData = (ItemNodeData)inputNode.NodeData;
                ITransmutableItem transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(itemNodeData.SerializedItemSlot?.id);
                if (transmutableItemObject == null)
                {
                    return ReturnDefault();
                }
                TransmutableItemMaterial material = transmutableItemObject.getMaterial();
                if (!material)
                {
                    return ReturnDefault();
                }
                return material;
                
                TransmutableItemMaterial ReturnDefault()
                {
                    return TransmutableItemUtils.GetDefaultObjectOfState(TransmutableItemState.Ingot).getMaterial();
                }
            }
        }
    }
}
