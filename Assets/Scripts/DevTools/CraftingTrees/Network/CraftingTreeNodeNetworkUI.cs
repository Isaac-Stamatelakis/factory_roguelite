using System;
using System.Collections.Generic;
using System.Linq;
using DevTools.CraftingTrees.TreeEditor;
using Item.Slot;
using Items;
using Items.Transmutable;
using Newtonsoft.Json;
using Recipe.Data;
using Recipe.Processor;
using Recipe.Viewer;
using UI.NodeNetwork;
#if  UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DevTools.CraftingTrees.Network
{
    internal class CraftingTreeGeneratorNode : INode
    {
        public CraftingTreeNodeType NodeType;
        public NodeNetworkData NetworkData;
        public CraftingTreeNodeData NodeData;
        public Vector3 GetPosition()
        {
            return new Vector3(NetworkData.X, NetworkData.Y, 0);
        }

        public void SetPosition(Vector3 pos)
        {
            NetworkData.X = pos.x;
            NetworkData.Y = pos.y;
        }

        public int GetId()
        {
            return NetworkData.Id;
        }

        public List<int> GetPrerequisites()
        {
            return NetworkData.InputIds;
        }

        public bool IsCompleted()
        {
            return true;
        }

        public CraftingTreeGeneratorNode(CraftingTreeNodeType nodeType, NodeNetworkData networkData, CraftingTreeNodeData nodeData)
        {
            NodeType = nodeType;
            NetworkData = networkData;
            NodeData = nodeData;
        }
    }
    internal enum CraftingTreeNodeType
    {
        Item,
        Transmutation,
        Processor
    }
    internal class NodeNetworkData
    {
        public int Id;
        public float X;
        public float Y;
        public List<int> InputIds;
        
        
    }

    internal abstract class CraftingTreeNodeData
    {
        public abstract ItemSlot GetDisplaySlot();
    }


    internal class ItemNodeData : CraftingTreeNodeData {
        
        public SerializedItemSlot SerializedItemSlot;
        public float Odds = 1f;
        public override ItemSlot GetDisplaySlot()
        {
            return ItemSlotFactory.deseralizeItemSlot(SerializedItemSlot);
        }
    }

    
    
    internal class TransmutationNodeData : CraftingTreeNodeData

    {
        public TransmutableItemState OutputState;
        public override ItemSlot GetDisplaySlot()
        {
            TransmutableItemObject transmutableItemObject = TransmutableItemUtils.GetDefaultObjectOfState(OutputState);
            return new ItemSlot(transmutableItemObject, 1, null);
        }
    }



    internal class ProcessorNodeData : CraftingTreeNodeData
    {
        public int Mode;
        public string ProcessorGuid;
        public string RecipeGuid;
        public ItemRecipe RecipeData;
        public override ItemSlot GetDisplaySlot()
        {
            ItemSlot itemSlot = null;
#if  UNITY_EDITOR
            string path = AssetDatabase.GUIDToAssetPath(ProcessorGuid);
            RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(path);
            itemSlot = new ItemSlot(recipeProcessor?.DisplayImage, 1, null);
#endif
            return itemSlot;
        }
    }

    internal class SerializedNodeData
    {
        public NodeNetworkData NodeNetworkData;
        public CraftingTreeNodeType NodeType;
        public string Data;
    }
    internal class SerializedCraftingTreeNodeNetwork
    {
        public List<SerializedNodeData> SerializedNodes;
    }

    internal static class SerializedCraftingTreeNodeNetworkUtils
    {
        public static SerializedCraftingTreeNodeNetwork SerializeNodeNetwork(CraftingTreeNodeNetwork nodeNetwork)
        {
            var serializedNodes = new List<SerializedNodeData>();
            foreach (CraftingTreeGeneratorNode node in nodeNetwork.Nodes)
            {
                var serializedNode = SerializedNode(node);
                serializedNodes.Add(serializedNode);
            }

            return new SerializedCraftingTreeNodeNetwork
            {
                SerializedNodes = serializedNodes
            };
        }

        public static CraftingTreeNodeNetwork DeserializeNodeNetwork(
            SerializedCraftingTreeNodeNetwork serializedNetwork)
        {
            List<CraftingTreeGeneratorNode> nodes = new List<CraftingTreeGeneratorNode>();
            foreach (SerializedNodeData serializedNodeData in serializedNetwork.SerializedNodes)
            {
                CraftingTreeGeneratorNode node = DeserializeNode(serializedNodeData);
                if (node == null) continue;
                nodes.Add(node);
            }
            return new CraftingTreeNodeNetwork
            {
                Nodes = nodes
            };
        }

        private static SerializedNodeData SerializedNode(CraftingTreeGeneratorNode node)
        {
            string serializedData = JsonConvert.SerializeObject(node.NodeData);
            return new SerializedNodeData
            {
                NodeType = node.NodeType,
                NodeNetworkData = node.NetworkData,
                Data = serializedData
            };
        }

        private static CraftingTreeGeneratorNode DeserializeNode(SerializedNodeData serializedData)
        {
            try
            {
                CraftingTreeNodeData nodeData;

                switch (serializedData.NodeType)
                {
                    case CraftingTreeNodeType.Item:
                        nodeData = JsonConvert.DeserializeObject<ItemNodeData>(serializedData.Data);
                        break;
                    case CraftingTreeNodeType.Transmutation:
                        nodeData = JsonConvert.DeserializeObject<TransmutationNodeData>(serializedData.Data);
                        break;
                    case CraftingTreeNodeType.Processor:
                        nodeData = JsonConvert.DeserializeObject<ProcessorNodeData>(serializedData.Data);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new CraftingTreeGeneratorNode(serializedData.NodeType, serializedData.NodeNetworkData, nodeData);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError($"Failed to deserialize node data {e.Message}");
                return null;
            }
        }

        public static int GetNextId(CraftingTreeNodeNetwork craftingTreeNodeNetwork)
        {
            int largestNode = -1;
            foreach (CraftingTreeGeneratorNode node in craftingTreeNodeNetwork.Nodes)
            {
                if (node == null) continue;
                if (node.GetId() > largestNode)
                {
                    largestNode = node.GetId();
                }
            }

            return largestNode + 1;
        }
    }
    internal class CraftingTreeNodeNetwork : INodeNetwork<CraftingTreeGeneratorNode>
    {
        public List<CraftingTreeGeneratorNode> Nodes;
        public List<CraftingTreeGeneratorNode> GetNodes()
        {
            return Nodes;
        }
    }

    internal class CraftingTreeTypeEnforcer : NodeConnectionFilterer
    {
        public CraftingTreeTypeEnforcer(INodeNetworkUI nodeNetworkUI) : base(nodeNetworkUI)
        {
        }

        public override bool CanConnect(INode input, INode output)
        {
            if (input is not CraftingTreeGeneratorNode craftingInput || output is not CraftingTreeGeneratorNode craftingOutput) return false;
            
            if (craftingInput.NodeType == craftingOutput.NodeType) return false; // Nodes of the same type cannot connect to each other

            // Transmutation nodes only allow one input and one output, which must be item nodes.
            // Transmutation states must be different. Transmutation states must be transmutable. EG No dust to screw.

            bool IsTransmutationValid(CraftingTreeGeneratorNode transmutationNode, CraftingTreeGeneratorNode otherNode, bool same)
            {
                if (otherNode.NodeType != CraftingTreeNodeType.Item) return false;
                ItemNodeData itemNodeData = (ItemNodeData)otherNode.NodeData;
                TransmutableItemObject transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(itemNodeData.SerializedItemSlot?.id);
                if (!transmutableItemObject) return false;
                TransmutationNodeData transmutationNodeData = (TransmutationNodeData)transmutationNode.NodeData;
                return same != (transmutableItemObject.getState() != transmutationNodeData.OutputState);
            }
            if (craftingOutput.NodeType == CraftingTreeNodeType.Transmutation)
            {
                if (!IsTransmutationValid(craftingOutput, craftingInput,false)) return false;
                if (craftingOutput.NetworkData.InputIds.Count == 0) return true;
                return craftingOutput.NetworkData.InputIds.Contains(craftingInput.GetId());
            }

            if (craftingInput.NodeType == CraftingTreeNodeType.Transmutation)
            {
                if (!IsTransmutationValid(craftingInput, craftingOutput,true)) return false;
                int outputs = nodeNetworkUI.GetNodeOutputs(craftingInput);
                if (outputs == 0) return true;
                return craftingOutput.NetworkData.InputIds.Contains(craftingInput.GetId());
            }

            // Processors can only connect to item nodes.
            if (craftingInput.NodeType == CraftingTreeNodeType.Processor)
            {
                return craftingOutput.NodeType == CraftingTreeNodeType.Item;
            }

            if (craftingOutput.NodeType == CraftingTreeNodeType.Processor)
            {
                return craftingInput.NodeType == CraftingTreeNodeType.Item;
            }

            return false;
        }
        
    }
    
    internal class CraftingTreeNodeNetworkUI : NodeNetworkUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetwork>
    {
        [SerializeField] private CraftingTreeNodeUI mCraftingTreeNodeUIPrefab;
        private readonly Dictionary<int, CraftingTreeGeneratorNode> nodes = new();
        private CraftingTreeGenerator craftingTreeGenerator;
        private CraftingTreeGeneratorUI craftingTreeGeneratorUI;
        public CraftingTreeGeneratorUI CraftingTreeGeneratorUI => craftingTreeGeneratorUI;

        public void Initialize(CraftingTreeNodeNetwork craftingTreeNodeNetwork, CraftingTreeGenerator generator, CraftingTreeGeneratorUI craftingTreeGeneratorUI)
        {
            this.craftingTreeGeneratorUI = craftingTreeGeneratorUI;
            this.nodeNetwork = craftingTreeNodeNetwork;
            craftingTreeGenerator = generator;
            bool inDevTools = DevToolUtils.OnDevToolScene;
            editController.gameObject.SetActive(inDevTools);
            editController.Initialize(this);
            foreach (CraftingTreeGeneratorNode node in craftingTreeNodeNetwork.Nodes)
            {
                nodes[node.GetId()] = node;
            }
            connectionFilterer = new CraftingTreeTypeEnforcer(this);
            
            Display();
            
        }
        protected override INodeUI GenerateNode(CraftingTreeGeneratorNode node)
        {
            CraftingTreeNodeUI craftingTreeNodeUI = GameObject.Instantiate(mCraftingTreeNodeUIPrefab);
            craftingTreeNodeUI.Initialize(node,this);
            RectTransform nodeRectTransform = (RectTransform)craftingTreeNodeUI.transform;
            craftingTreeNodeUI.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return craftingTreeNodeUI;
        }

        public override bool ShowAllComplete() {
            return false;

        }

        public override void OnDeleteSelectedNode()
        {
            // TODO Hide side view
        }
        
        public override CraftingTreeGeneratorNode LookUpNode(int id)
        {
            return nodes.GetValueOrDefault(id);
        }
        public override void PlaceNewNode(Vector2 position)
        {
            int id = SerializedCraftingTreeNodeNetworkUtils.GetNextId(nodeNetwork);
            CraftingTreeGeneratorNode node = craftingTreeGenerator?.GenerateNewNode(id);
            if (node == null) return;
            node.SetPosition(position);
            nodeNetwork.Nodes.Add(node);
            nodes[node.GetId()] = node;
        }

        public override GameObject GenerateNewNodeObject()
        {
            if (craftingTreeGenerator == null) return null;
            return GameObject.Instantiate(mCraftingTreeNodeUIPrefab).gameObject;
        }
    }
}
