using System;
using System.Collections.Generic;
using DevTools.CraftingTrees.TreeEditor;
using UnityEngine;

namespace DevTools.CraftingTrees.Network
{
    internal class CraftingTreeGeneratorUI : MonoBehaviour
    {
        [SerializeField] private CraftingTreeNodeNetworkUI mNodeNetworkUI;
        [SerializeField] private CraftingTreeNodeEditorUI mNodeEditorUI;
        [SerializeField] private CraftingTreeSettingEditorUI mSettingEditorUI;
        private CraftingTreeGenerator craftingTreeGenerator;
        private string filePath;
        private CraftingTreeNodeNetwork nodeNetwork;
        public CraftingTreeNodeEditorUI NodeEditorUI => mNodeEditorUI;
        public CraftingTreeSettingEditorUI SettingEditorUI => mSettingEditorUI;
        public void Initialize(CraftingTreeNodeNetwork nodeNetwork, string filePath)
        {
            this.nodeNetwork = nodeNetwork;
            this.filePath = filePath;
            craftingTreeGenerator = new CraftingTreeGenerator();
            mNodeNetworkUI.Initialize(nodeNetwork,craftingTreeGenerator,this);
            mSettingEditorUI.Initialize(nodeNetwork,craftingTreeGenerator);
        }

        public void OnDestroy()
        {
            Save();
        }

        private void Save()
        {
            SerializedCraftingTreeNodeNetwork serializedCraftingTreeNodeNetwork = SerializedCraftingTreeNodeNetworkUtils.SerializeNodeNetwork(nodeNetwork);
            GlobalHelper.SerializeCompressedJson(serializedCraftingTreeNodeNetwork,filePath);
        }

        public void Rebuild()
        {
            mNodeNetworkUI.Display();
        }
    }

    internal class CraftingTreeGenerator
    {
        private CraftingTreeNodeType nodeGenerationType;

        public void SetType(CraftingTreeNodeType nodeType)
        {
            nodeGenerationType = nodeType;
        }
        public CraftingTreeGeneratorNode GenerateNewNode(int nextId)
        {
            NodeNetworkData nodeNetworkData = new NodeNetworkData
            {
                Id = nextId,
                InputIds = new List<int>()
            };
            CraftingTreeNodeData data;
            switch (nodeGenerationType)
            {
                case CraftingTreeNodeType.Item:
                    data = new ItemNodeData();
                    break;
                case CraftingTreeNodeType.Transmutation:
                    data = new TransmutationNodeData();
                    break;
                case CraftingTreeNodeType.Processor:
                    data = new ProcessorNodeData();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new CraftingTreeGeneratorNode(nodeGenerationType, nodeNetworkData, data);
        }
    }
}
