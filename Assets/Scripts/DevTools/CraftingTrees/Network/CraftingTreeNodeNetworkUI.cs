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
<<<<<<< HEAD
<<<<<<< HEAD
        public CraftingTreeNodeData NodeData;
=======
        public CraftingTreeGeneratorNodeData NodeData;
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
        public CraftingTreeNodeData NodeData;
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
<<<<<<< HEAD
<<<<<<< HEAD
    internal abstract class CraftingTreeNodeData
=======
    internal abstract class CraftingTreeGeneratorNodeData
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
    internal abstract class CraftingTreeNodeData
>>>>>>> 1420320e (Added basic ui for crafting trees)
    {
        public int Id;
        public float X;
        public float Y;
        public List<int> InputIds;
    }

<<<<<<< HEAD
<<<<<<< HEAD
    internal class ItemNodeData : CraftingTreeNodeData
=======
    internal class ItemNodeData : CraftingTreeGeneratorNodeData
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
    internal class ItemNodeData : CraftingTreeNodeData
>>>>>>> 1420320e (Added basic ui for crafting trees)
    {
        public SerializedItemSlot SerializedItemSlot;
    }

<<<<<<< HEAD
<<<<<<< HEAD
    internal class TransmutationNodeData : CraftingTreeNodeData
=======
    internal class TransmutationNodeData : CraftingTreeGeneratorNodeData
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
    internal class TransmutationNodeData : CraftingTreeNodeData
>>>>>>> 1420320e (Added basic ui for crafting trees)
    {
        public TransmutableItemState OutputState;
    }

<<<<<<< HEAD
<<<<<<< HEAD
    internal class ProcessorNodeData : CraftingTreeNodeData
=======
    internal class ProcessorNodeData : CraftingTreeGeneratorNodeData
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
    internal class ProcessorNodeData : CraftingTreeNodeData
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
<<<<<<< HEAD
<<<<<<< HEAD
                CraftingTreeNodeData nodeData;
=======
                CraftingTreeGeneratorNodeData nodeData;
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
                CraftingTreeNodeData nodeData;
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
<<<<<<< HEAD
<<<<<<< HEAD

    
    internal class CraftingTreeNodeNetworkUI : NodeNetworkUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetwork>
    {
        [SerializeField] private CraftingTreeNodeUI mCraftingTreeNodeUIPrefab;
        private readonly Dictionary<int, CraftingTreeGeneratorNode> nodes = new();
        private CraftingTreeGenerator craftingTreeGenerator;

        public void Initialize(CraftingTreeNodeNetwork craftingTreeNodeNetwork,
            CraftingTreeGenerator generator)
        {
            this.nodeNetwork = craftingTreeNodeNetwork;
            craftingTreeGenerator = generator;
        }
        protected override INodeUI GenerateNode(CraftingTreeGeneratorNode node)
        {
            CraftingTreeNodeUI robotUpgradeNodeUI = GameObject.Instantiate(mCraftingTreeNodeUIPrefab);
            robotUpgradeNodeUI.Initialize(node,this);
            RectTransform nodeRectTransform = (RectTransform)robotUpgradeNodeUI.transform;
            robotUpgradeNodeUI.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return robotUpgradeNodeUI;
=======
=======

    
>>>>>>> 1420320e (Added basic ui for crafting trees)
    internal class CraftingTreeNodeNetworkUI : NodeNetworkUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetwork>
    {
        [SerializeField] private CraftingTreeNodeUI mCraftingTreeNodeUIPrefab;
        private readonly Dictionary<int, CraftingTreeGeneratorNode> nodes = new();
        private CraftingTreeGenerator craftingTreeGenerator;

        public void Initialize(CraftingTreeNodeNetwork craftingTreeNodeNetwork,
            CraftingTreeGenerator generator)
        {
            this.nodeNetwork = craftingTreeNodeNetwork;
            craftingTreeGenerator = generator;
        }
        protected override INodeUI GenerateNode(CraftingTreeGeneratorNode node)
        {
<<<<<<< HEAD
            throw new System.NotImplementedException();
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
            CraftingTreeNodeUI robotUpgradeNodeUI = GameObject.Instantiate(mCraftingTreeNodeUIPrefab);
            robotUpgradeNodeUI.Initialize(node,this);
            RectTransform nodeRectTransform = (RectTransform)robotUpgradeNodeUI.transform;
            robotUpgradeNodeUI.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return robotUpgradeNodeUI;
>>>>>>> 1420320e (Added basic ui for crafting trees)
        }

        public override bool ShowAllComplete()
        {
<<<<<<< HEAD
<<<<<<< HEAD
            return false;
=======
            throw new System.NotImplementedException();
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
            return false;
>>>>>>> 1420320e (Added basic ui for crafting trees)
        }

        public override void OnDeleteSelectedNode()
        {
<<<<<<< HEAD
<<<<<<< HEAD
            // TODO Hide side view
=======
            throw new System.NotImplementedException();
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
            // TODO Hide side view
>>>>>>> 1420320e (Added basic ui for crafting trees)
        }
        
        public override CraftingTreeGeneratorNode LookUpNode(int id)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            return nodes.GetValueOrDefault(id);
        }
        public override void PlaceNewNode(Vector2 position)
        {
            CraftingTreeGeneratorNode node = craftingTreeGenerator?.GenerateNewNode(0);
            if (node == null) return;
            node.SetPosition(position);
            nodeNetwork.Nodes.Add(node);
=======
            throw new System.NotImplementedException();
        }
        public override void PlaceNewNode(Vector2 position)
        {
            throw new System.NotImplementedException();
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
            return nodes.GetValueOrDefault(id);
        }
        public override void PlaceNewNode(Vector2 position)
        {
            CraftingTreeGeneratorNode node = craftingTreeGenerator?.GenerateNewNode(0);
            if (node == null) return;
            node.SetPosition(position);
            nodeNetwork.Nodes.Add(node);
>>>>>>> 1420320e (Added basic ui for crafting trees)
        }

        public override GameObject GenerateNewNodeObject()
        {
<<<<<<< HEAD
<<<<<<< HEAD
            if (craftingTreeGenerator == null) return null;
            return GameObject.Instantiate(mCraftingTreeNodeUIPrefab).gameObject;
=======
            throw new System.NotImplementedException();
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
=======
            if (craftingTreeGenerator == null) return null;
            return GameObject.Instantiate(mCraftingTreeNodeUIPrefab).gameObject;
>>>>>>> 1420320e (Added basic ui for crafting trees)
        }
    }
}
