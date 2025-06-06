using System;
using System.Collections.Generic;
using System.Linq;
using DevTools.CraftingTrees.TreeEditor;
using Item.Slot;
using Item.Transmutation;
using Item.Transmutation.Info;
using Items;
using Items.Transmutable;
using Newtonsoft.Json;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
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
        
    }


    internal class ItemNodeData : CraftingTreeNodeData {
        
        public SerializedItemSlot SerializedItemSlot;
        public float Odds = 1f;
    }

    
    
    internal class TransmutationNodeData : CraftingTreeNodeData

    {
        public TransmutableItemState InputState;
        public uint InputAmount;
        public string OutputItemId;
        
        public ItemSlot GetItemSlot(TransmutationEfficency transmutationEfficency)
        {
            ITransmutableItem transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(OutputItemId);
            if (transmutableItemObject == null) return null;
            var (inputAmount, _) = TransmutableItemUtils.GetInputOutputAmount(InputState,transmutableItemObject.getState(),transmutationEfficency.Value());
            return new ItemSlot((ItemObject)transmutableItemObject, inputAmount,null);
        }
    }


    public  class RecipeMetaData
    {
        
    }

    public class PassiveRecipeMetaData : RecipeMetaData
    {
        public float Seconds = 1;
    }

    public class BurnerRecipeMetaData : PassiveRecipeMetaData
    {
        public float PassiveSpeed = 0;
    }

    public class GeneratorItemRecipeMetaData : PassiveRecipeMetaData
    {
        public ulong EnergyPerTick = 32;
    }

    public class ItemEnergyRecipeMetaData : RecipeMetaData
    {
        public ulong TotalInputEnergy = 8192;
        public ulong MinimumEnergyPerTick = 32;
    }
    
    internal class ProcessorNodeData : CraftingTreeNodeData
    {
        public int Mode;
        public string ProcessorGuid;
        public string RecipeGuid;
        public RecipeMetaData RecipeData;
    }

    internal class SerializedNodeData
    {
        public NodeNetworkData NodeNetworkData;
        public CraftingTreeNodeType NodeType;
        public string Data;
    }

    internal class SerializedProcessorNodeData
    {
        public int Mode;
        public string ProcessorGuid;
        public string RecipeGuid;
        public string MetaData;
    }
    internal class SerializedCraftingTreeNodeNetwork
    {
        public TransmutationEfficency Efficency;
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
                SerializedNodes = serializedNodes,
                Efficency = nodeNetwork.TransmutationEfficency
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
                Nodes = nodes,
                TransmutationEfficency = serializedNetwork.Efficency
            };
        }

        private static SerializedNodeData SerializedNode(CraftingTreeGeneratorNode node)
        {
            string serializedData = SerializeNodeData(node.NodeType, node.NodeData);
            return new SerializedNodeData
            {
                NodeType = node.NodeType,
                NodeNetworkData = node.NetworkData,
                Data = serializedData
            };
        }

        private static string SerializeNodeData(CraftingTreeNodeType type, CraftingTreeNodeData nodeData)
        {
            switch (type)
            {
                case CraftingTreeNodeType.Item:
                case CraftingTreeNodeType.Transmutation:
                    return JsonConvert.SerializeObject(nodeData);
                case CraftingTreeNodeType.Processor:
                    ProcessorNodeData processorNodeData = (ProcessorNodeData)nodeData;
                    SerializedProcessorNodeData serializedProcessorNodeData = new SerializedProcessorNodeData
                    {
                        Mode = processorNodeData.Mode,
                        ProcessorGuid = processorNodeData.ProcessorGuid,
                        RecipeGuid = processorNodeData.RecipeGuid,
                        MetaData = JsonConvert.SerializeObject(processorNodeData.RecipeData)
                    };
                    return JsonConvert.SerializeObject(serializedProcessorNodeData);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static CraftingTreeNodeData DeserializeNodeData(CraftingTreeNodeType type, string serializedData)
        {
            switch (type)
            {
                case CraftingTreeNodeType.Item:
                    return JsonConvert.DeserializeObject<ItemNodeData>(serializedData);
                case CraftingTreeNodeType.Transmutation:
                    return JsonConvert.DeserializeObject<TransmutationNodeData>(serializedData);
                case CraftingTreeNodeType.Processor:
                    RecipeProcessor recipeProcessor = null;
                    #if  UNITY_EDITOR
                    
                    SerializedProcessorNodeData serializedProcessorNodeData = JsonConvert.DeserializeObject<SerializedProcessorNodeData>(serializedData);
                    string processorGuid = serializedProcessorNodeData.ProcessorGuid;
                    string assetPath = AssetDatabase.GUIDToAssetPath(processorGuid);
                    recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(assetPath);
                    
                    #endif
                    if (!recipeProcessor) return null;
                    RecipeMetaData recipeMetaData;
                    switch (recipeProcessor.RecipeType)
                    {
                        case RecipeType.Item:
                            recipeMetaData = null;
                            break;
                        case RecipeType.Passive:
                            recipeMetaData = JsonConvert.DeserializeObject<PassiveRecipeMetaData>(serializedProcessorNodeData.MetaData);
                            break;
                        case RecipeType.Generator:
                            recipeMetaData = JsonConvert.DeserializeObject<GeneratorItemRecipeMetaData>(serializedProcessorNodeData.MetaData);
                            break;
                        case RecipeType.Machine:
                            recipeMetaData = JsonConvert.DeserializeObject<ItemEnergyRecipeMetaData>(serializedProcessorNodeData.MetaData);
                            break;
                        case RecipeType.Burner:
                            recipeMetaData = JsonConvert.DeserializeObject<BurnerRecipeMetaData>(serializedProcessorNodeData.MetaData);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return new ProcessorNodeData
                    {
                        Mode = serializedProcessorNodeData.Mode,
                        ProcessorGuid = processorGuid,
                        RecipeGuid = serializedProcessorNodeData.RecipeGuid,
                        RecipeData = recipeMetaData,
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static CraftingTreeGeneratorNode DeserializeNode(SerializedNodeData serializedData)
        {
            try
            {
                CraftingTreeNodeData nodeData = DeserializeNodeData(serializedData.NodeType, serializedData.Data);
                if (nodeData == null) return null;
                return new CraftingTreeGeneratorNode(serializedData.NodeType, serializedData.NodeNetworkData, nodeData);
            }
            catch (Exception e) when (e is JsonSerializationException or NullReferenceException or ArgumentException)
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
        public TransmutationEfficency TransmutationEfficency;
        public List<CraftingTreeGeneratorNode> Nodes;
        public List<CraftingTreeGeneratorNode> GetNodes()
        {
            return Nodes;
        }

        public bool HasGeneratedRecipes()
        {
            foreach (CraftingTreeGeneratorNode node in Nodes)
            {
                if (node == null) continue;
                if (node.NodeType != CraftingTreeNodeType.Processor) continue;
                ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                if (processorNodeData.RecipeGuid != null) return true;
            }
            return false;
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
            
            if (craftingInput.NodeType == CraftingTreeNodeType.Processor)
            {
                return craftingOutput.NodeType == CraftingTreeNodeType.Item;
            }

            if (craftingOutput.NodeType == CraftingTreeNodeType.Processor)
            {
                return true;
            }
            
            if (craftingInput.NodeType is CraftingTreeNodeType.Item or CraftingTreeNodeType.Transmutation && craftingOutput.NodeType == CraftingTreeNodeType.Transmutation)
            {
                if (craftingInput.NodeType == CraftingTreeNodeType.Item)
                {
                    ItemNodeData itemNodeData = (ItemNodeData)craftingInput.NodeData;
                    ITransmutableItem transmutableItemObject = ItemRegistry.GetInstance().GetTransmutableItemObject(itemNodeData.SerializedItemSlot?.id);
                    if (transmutableItemObject == null) return false;
                }
                if (craftingOutput.NetworkData.InputIds.Count == 0) return true;
                return craftingOutput.NetworkData.InputIds.Contains(craftingInput.GetId());
            }
            if (craftingInput.NodeType == CraftingTreeNodeType.Transmutation && craftingOutput.NodeType == CraftingTreeNodeType.Transmutation)
            {
                return true;
            }
            return false;
        }
        
    }
    
    internal class CraftingTreeNodeNetworkUI : NodeNetworkUI<CraftingTreeGeneratorNode,CraftingTreeNodeNetwork>, ITreeGenerationListener
    {
        [SerializeField] private CraftingTreeNodeUI mCraftingTreeNodeUIPrefab;
        private readonly Dictionary<int, CraftingTreeGeneratorNode> nodes = new();
        private CraftingTreeGenerator craftingTreeGenerator;
        private CraftingTreeGeneratorUI craftingTreeGeneratorUI;
        public CraftingTreeGeneratorUI CraftingTreeGeneratorUI => craftingTreeGeneratorUI;
        private bool generated;

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

        public override void OnConnectionModified(CraftingTreeGeneratorNode node)
        {
            if (node.NodeType != CraftingTreeNodeType.Transmutation) return;
            INodeUI nodeUI = nodeUIDict.GetValueOrDefault(node);
            nodeUI?.DisplayImage();
        }

        public override void OnDeleteSelectedNode()
        {
            craftingTreeGeneratorUI.NodeEditorUI.Hide();
        }
        
        public override CraftingTreeGeneratorNode LookUpNode(int id)
        {
            return nodes.GetValueOrDefault(id);
        }
        public override INode PlaceNewNode(Vector2 position)
        {
            if (generated) return null;
            int id = SerializedCraftingTreeNodeNetworkUtils.GetNextId(nodeNetwork);
            CraftingTreeGeneratorNode node = craftingTreeGenerator?.GenerateNewNode(id);
            if (node == null) return null;
            node.SetPosition(position);
            nodeNetwork.Nodes.Add(node);
            nodes[node.GetId()] = node;
            return node;
        }

        public override GameObject GenerateNewNodeObject()
        {
            if (generated) return null;
            if (craftingTreeGenerator == null) return null;
            return GameObject.Instantiate(mCraftingTreeNodeUIPrefab).gameObject;
        }

        public void OnStatusChange(bool generated)
        {
            this.generated = generated;
            if (generated)
            {
                editController.ClearSpawnedObject();
                SelectNode(null);
            }
            editController.gameObject.SetActive(!generated);
            
        }
    }
}
