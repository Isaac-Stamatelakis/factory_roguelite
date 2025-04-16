using System;
using System.Collections.Generic;
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
        public CraftingTreeGeneratorNodeData NodeData;
        public Vector3 GetPosition()
        {
            return new Vector3(NodeData.X, NodeData.Y, 0);
        }

        public void SetPosition(Vector3 pos)
        {
            NodeData.X = pos.x;
            NodeData.Y = pos.y;
        }

        public int GetId()
        {
            return NodeData.Id;
        }

        public List<int> GetPrerequisites()
        {
            return NodeData.InputIds;
        }

        public bool IsCompleted()
        {
            return true;
        }
        
    }
    internal enum CraftingTreeNodeType
    {
        Item,
        Transmutation,
        Processor
    }
    internal abstract class CraftingTreeGeneratorNodeData
    {
        public int Id;
        public float X;
        public float Y;
        public List<int> InputIds;
    }

    internal class ItemNodeData : CraftingTreeGeneratorNodeData
    {
        public SerializedItemSlot SerializedItemSlot;
    }

    internal class TransmutationNodeData : CraftingTreeGeneratorNodeData
    {
        public TransmutableItemState OutputState;
    }

    internal class ProcessorNodeData : CraftingTreeGeneratorNodeData
    {
        public string ProcessorGuid;
        public string RecipeGuid;
        public ItemRecipe RecipeData;
    }

    internal class SerializedNodeData
    {
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
                Data = serializedData
            };
        }

        private static CraftingTreeGeneratorNode DeserializeNode(SerializedNodeData serializedData)
        {
            try
            {
                CraftingTreeGeneratorNodeData nodeData;
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

                return new CraftingTreeGeneratorNode
                {
                    NodeType = serializedData.NodeType,
                    NodeData = nodeData,
                };
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError($"Failed to deserialize node data {e.Message}");
                return null;
            }
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
        protected override INodeUI GenerateNode(CraftingTreeGeneratorNode node)
        {
            throw new System.NotImplementedException();
        }

        public override bool ShowAllComplete()
        {
            throw new System.NotImplementedException();
        }

        public override void OnDeleteSelectedNode()
        {
            throw new System.NotImplementedException();
        }
        
        public override CraftingTreeGeneratorNode LookUpNode(int id)
        {
            throw new System.NotImplementedException();
        }
        public override void PlaceNewNode(Vector2 position)
        {
            throw new System.NotImplementedException();
        }

        public override GameObject GenerateNewNodeObject()
        {
            throw new System.NotImplementedException();
        }
    }
}
