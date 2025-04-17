using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.TreeEditor;
using Item.Slot;
using Newtonsoft.Json;
using Recipe.Data;
using Recipe.Viewer;
using UI.NodeNetwork;
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
        
    }


    internal class ItemNodeData : CraftingTreeNodeData {
        
        public SerializedItemSlot SerializedItemSlot;
    }

    
    
    internal class TransmutationNodeData : CraftingTreeNodeData

    {
        public TransmutableItemState OutputState;
    }



    internal class ProcessorNodeData : CraftingTreeNodeData
    {
        public string ProcessorGuid;
        public string RecipeGuid;
        public ItemRecipe RecipeData;
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
