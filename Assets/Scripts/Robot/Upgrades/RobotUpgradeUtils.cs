using System.Collections.Generic;
using System.IO;
using DevTools;
using Newtonsoft.Json;
using Player.Tool;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using RobotModule;
using UnityEngine;
using WorldModule;

namespace Robot.Upgrades
{
    public static class RobotUpgradeUtils
    {
        public static SerializedRobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(string upgradePath)
        {
            if (!upgradePath.EndsWith(".bin"))
            {
                upgradePath += ".bin";
            }
            string filePath = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.Upgrade), upgradePath);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Tried to read invalid upgrade path '{filePath}'");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            return DeserializeRobotNodeNetwork(bytes);
        }
        public static SerializedRobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(byte[] bytes)
        {
            string json = WorldLoadUtils.DecompressString(bytes);
            if (json == null) return null;
            try
            {
                SerializedRobotUpgradeNodeNetwork network = JsonConvert.DeserializeObject<SerializedRobotUpgradeNodeNetwork>(json);
                network.NodeData ??= new List<RobotUpgradeNodeData>();
                return network;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public static RobotUpgradeLoadOut DeserializeRobotStatLoadOut(string json)
        {
            if (json == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<RobotUpgradeLoadOut>(json);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning($"Error deserializing stat loadout '{e.Message}'");
                return null;
            }
        }
        public static RobotUpgradeLoadOut VerifyIntegrityOfLoadOut(RobotUpgradeLoadOut loadOut, RobotItemData robotItemData)
        {
            if (loadOut == null)
            {
                RobotStatLoadOutCollection selfCollection = CreateNewLoadOutCollection(RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot,0));
                loadOut = new RobotUpgradeLoadOut(selfCollection, new Dictionary<RobotToolType, RobotStatLoadOutCollection>());
            }
            

            foreach (var tool in robotItemData.ToolData.Types)
            {
                if (loadOut.ToolLoadOuts.ContainsKey(tool)) continue;
                RobotUpgradeInfo toolInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Tool, (int)tool);
                if (toolInfo == null) continue;
                
                loadOut.ToolLoadOuts[tool] = CreateNewLoadOutCollection(toolInfo);

            }
            return loadOut;
        }

        private static RobotStatLoadOutCollection CreateNewLoadOutCollection(RobotUpgradeInfo robotUpgradeInfo)
        {
            return new RobotStatLoadOutCollection(
                0, 
                new List<RobotStatLoadOut>{robotUpgradeInfo.GetRobotStatLoadOut()});
        }
        public static RobotUpgradeNodeNetwork FromSerializedNetwork(SerializedRobotUpgradeNodeNetwork sNetwork)
        {
            if (sNetwork == null) return null;
            List<RobotUpgradeNode> robotUpgradeNodes = new List<RobotUpgradeNode>();
            foreach (RobotUpgradeNodeData nodeData in sNetwork.NodeData)
            {
                robotUpgradeNodes.Add(new RobotUpgradeNode(nodeData,new RobotUpgradeData(nodeData.Id,0)));
            }

            return new RobotUpgradeNodeNetwork(sNetwork.Type,sNetwork.SubType,robotUpgradeNodes);
        }
        
        public static RobotUpgradeNodeNetwork FromSerializedNetwork(SerializedRobotUpgradeNodeNetwork sNetwork, List<RobotUpgradeData> upgradeDataList)
        {
            RobotUpgradeNodeNetwork robotUpgradeNodeNetwork = FromSerializedNetwork(sNetwork);
            if (robotUpgradeNodeNetwork == null) return null;
            Dictionary<int, RobotUpgradeNode> upgradeNodeDict = new Dictionary<int, RobotUpgradeNode>();
            foreach (RobotUpgradeNode node in robotUpgradeNodeNetwork.UpgradeNodes)
            {
                upgradeNodeDict[node.GetId()] = node;
            }


            HashSet<int> upgradeDataListIds = new HashSet<int>();

            for (var index = 0; index < upgradeDataList.Count; index++)
            {
                var upgradeData = upgradeDataList[index];
                RobotUpgradeNode node = upgradeNodeDict.GetValueOrDefault(upgradeData.Id);
                if (node == null)
                {
                    upgradeDataList.RemoveAt(index); // Remove nodes that no longer exist
                    continue;
                }

                upgradeDataListIds.Add(node.GetId());
                node.InstanceData = upgradeData;
            }

            // Add nodes to upgrade list which are not included
            foreach (var (id, node) in upgradeNodeDict)
            {
                if (upgradeDataListIds.Contains(id)) continue;
                upgradeDataList.Add(new RobotUpgradeData(id,0));
            }

            return robotUpgradeNodeNetwork;
        }

        public static SerializedRobotUpgradeNodeNetwork ToSerializedNetwork(RobotUpgradeNodeNetwork network)
        {
            if (network == null) return null;
            List<RobotUpgradeNodeData> nodeDataList = new List<RobotUpgradeNodeData>();
            foreach (RobotUpgradeNode node in network.UpgradeNodes)
            {
                nodeDataList.Add(node.NodeData);
            }
            return new SerializedRobotUpgradeNodeNetwork(network.Type,network.SubType,nodeDataList);
        }
        public static int GetNextUpgradeId(RobotUpgradeNodeNetwork robotUpgradeNodeNetwork)
        {
            List<RobotUpgradeNode> nodes = robotUpgradeNodeNetwork.GetNodes();
            if (nodes == null || nodes.Count == 0) return 0;
            int largest = 0;
            foreach (RobotUpgradeNode node in nodes)
            {
                int id = node.GetId();
                if (id > largest) largest = id;
            }

            return largest + 1;
        }
        
        public static uint GetRequireAmountMultiplier(RobotUpgradeNode robotUpgradeNode)
        {
            if ((robotUpgradeNode.InstanceData?.Amount ?? 0) == 0) return 1;
            int amount = robotUpgradeNode.InstanceData.Amount;
            return (uint)(Mathf.Pow(robotUpgradeNode.NodeData.CostMultiplier, amount));
        }

        public static int GetDiscreteValue(RobotStatLoadOutCollection loadOut, int upgrade)
        {
            return loadOut?.GetCurrent()?.GetDiscreteValue(upgrade) ?? 0;
        }
        
        public static float GetContinuousValue(RobotUpgradeLoadOut loadOut, int upgrade)
        {
            return loadOut?.SelfLoadOuts?.GetCurrent()?.GetCountinuousValue(upgrade) ?? 0;
        }


        public static Dictionary<int, int> BuildUpgradeDict(List<RobotUpgradeNodeData> nodeDataList, List<RobotUpgradeData> upgradeDataList)
        {
            Dictionary<int, int> upgradeCount = new Dictionary<int, int>();
            foreach (RobotUpgradeNodeData upgradeNodeData in nodeDataList)
            {
                foreach (RobotUpgradeData upgradeData in upgradeDataList)
                {
                    if (upgradeData.Id != upgradeNodeData.Id) continue;
                    upgradeCount.TryAdd(upgradeNodeData.UpgradeType, 0);
                    upgradeCount[upgradeNodeData.UpgradeType] += upgradeData.Amount;
                }
            }

            return upgradeCount;
        }
        
        
    }
}