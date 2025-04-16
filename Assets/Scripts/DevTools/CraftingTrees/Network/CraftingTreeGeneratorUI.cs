using System;
using UnityEngine;

namespace DevTools.CraftingTrees.Network
{
    internal class CraftingTreeGeneratorUI : MonoBehaviour
    {
        [SerializeField] private CraftingTreeNodeNetworkUI mNodeNetworkUI;

        private CraftingTreeGenerator craftingTreeGenerator;
        private string filePath;
        private CraftingTreeNodeNetwork nodeNetwork;
        public void Initialize(CraftingTreeNodeNetwork nodeNetwork, string filePath)
        {
            this.nodeNetwork = nodeNetwork;
            this.filePath = filePath;
            craftingTreeGenerator = new CraftingTreeGenerator();
            mNodeNetworkUI.Initialize(nodeNetwork,craftingTreeGenerator);
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
    }

    internal class CraftingTreeGenerator
    {
        private CraftingTreeNodeType nodeGenerationType;

        public CraftingTreeGeneratorNode GenerateNewNode(int nextId)
        {
            switch (nodeGenerationType)
            {
                case CraftingTreeNodeType.Item:
                    break;
                case CraftingTreeNodeType.Transmutation:
                    break;
                case CraftingTreeNodeType.Processor:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}
