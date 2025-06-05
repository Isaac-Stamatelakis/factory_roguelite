using System.Collections.Generic;
using System.IO;
using DevTools;
using Item.Slot;
using Newtonsoft.Json;
using Player;
using Player.Tool;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using RobotModule;
using Unity.VisualScripting;
using UnityEngine;
using WorldModule;

namespace Robot.Upgrades
{
    public static class RobotUpgradeUtils
    {
        public const int TILES_PER_VEIN_MINE_UPGRADE = 4;
        public const int BASE_REACH = 5;

        public static string FormatLoadOut(int loadOut)
        {
            return $"[{loadOut + 1}]";
        }
        public static SerializedRobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(string upgradePath)
        {
            if (!upgradePath.EndsWith(".json"))
            {
                upgradePath += ".json";
            }
            string filePath = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.Upgrade), upgradePath);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Tried to read invalid upgrade path '{filePath}'");
                return null;
            }

            string json = File.ReadAllText(filePath);
            
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

        public static RobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(string upgradePath, List<RobotUpgradeData> nodes)
        {
            SerializedRobotUpgradeNodeNetwork sNetwork = DeserializeRobotNodeNetwork(upgradePath);
            return FromSerializedNetwork(sNetwork,nodes);
        }

        public static void MaxOutUpgrades(PlayerScript playerScript, string upgradePath, List<RobotUpgradeData> nodes)
        {
            RobotUpgradeNodeNetwork robotUpgradeNodeNetwork = DeserializeRobotNodeNetwork(upgradePath,nodes);
            foreach (var node in robotUpgradeNodeNetwork.UpgradeNodes)
            {
                ApplyUpgrade(playerScript,RobotUpgradeType.Robot,0,node,robotUpgradeNodeNetwork.UpgradeNodes,-9999); // Reset
                ApplyUpgrade(playerScript,RobotUpgradeType.Robot,0,node,robotUpgradeNodeNetwork.UpgradeNodes,100);
            }
        }

        public static void ResetUpgrades(PlayerScript playerScript, string upgradePath, List<RobotUpgradeData> nodes)
        {
            RobotUpgradeNodeNetwork robotUpgradeNodeNetwork = DeserializeRobotNodeNetwork(upgradePath,nodes);
            foreach (var node in robotUpgradeNodeNetwork.UpgradeNodes)
            {
                ApplyUpgrade(playerScript,RobotUpgradeType.Robot,0,node,robotUpgradeNodeNetwork.UpgradeNodes,-9999); // Reset
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

        public static bool CanReach(Vector2 position, Vector2 mousePosition, RobotStatLoadOutCollection selfLoadOuts)
        {
            float distanceFromPlayer = Vector2.Distance(mousePosition, position);
            if (distanceFromPlayer <= RobotUpgradeUtils.BASE_REACH) return true;
            float bonusReach = RobotUpgradeUtils.GetContinuousValue(selfLoadOuts, (int)RobotUpgrade.Reach);
            return (distanceFromPlayer > RobotUpgradeUtils.BASE_REACH + bonusReach);
        }

        internal static void ApplyUpgrade(PlayerScript playerScript, RobotUpgradeType robotUpgradeType, int subType, RobotUpgradeNode robotUpgradeNode, List<RobotUpgradeNode> robotUpgradeNodes, int amount)
        {
            RobotUpgradeInfo upgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(robotUpgradeType, subType);
            List<int> constUpgrades = upgradeInfo.GetConstantUpgrades();
            int upgrade = robotUpgradeNode.NodeData.UpgradeType;
            robotUpgradeNode.InstanceData.Amount += amount;
            if (robotUpgradeNode.InstanceData.Amount < 0)
            {
                robotUpgradeNode.InstanceData.Amount = 0;
            }
            
            if (constUpgrades.Contains(upgrade))
            {
                UpdateConstantValues(playerScript.PlayerRobot,upgrade);
            }
            return;
            void UpdateConstantValues(PlayerRobot playerRobot, int constantUpgrade)
            {
                int totalUpgrades = 0;
                foreach (var node in robotUpgradeNodes)
                {
                    if (node.NodeData.UpgradeType != constantUpgrade) continue;
                    totalUpgrades += node.InstanceData.Amount;
                }
            
                RobotStatLoadOutCollection loadOutCollection = playerRobot.RobotUpgradeLoadOut.GetCollection(robotUpgradeType, subType);
                foreach (RobotStatLoadOut robotStatLoadOut in loadOutCollection.LoadOuts)
                {
                    if (robotStatLoadOut.DiscreteValues.ContainsKey(constantUpgrade))
                    {
                        robotStatLoadOut.DiscreteValues[constantUpgrade] = totalUpgrades;
                    } else if (robotStatLoadOut.ContinuousValues.ContainsKey(constantUpgrade))
                    {
                        robotStatLoadOut.DiscreteValues[constantUpgrade] = totalUpgrades;
                    }
                }
            }
            
        }
        public static RobotUpgradeLoadOut VerifyIntegrityOfLoadOut(RobotUpgradeLoadOut loadOut, RobotItemData robotItemData)
        {
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot, 0);
            if (loadOut == null)
            {
                RobotStatLoadOutCollection selfCollection = CreateNewLoadOutCollection(robotUpgradeInfo);
                loadOut = new RobotUpgradeLoadOut(selfCollection, new Dictionary<RobotToolType, RobotStatLoadOutCollection>());
            }
            else
            {
                foreach (var selfLoadout in loadOut.SelfLoadOuts.LoadOuts)
                {
                    robotUpgradeInfo.VerifyStatLoadOut(selfLoadout);
                }
            }
            
            foreach (var tool in robotItemData.ToolData.Types)
            {
                RobotUpgradeInfo toolInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Tool, (int)tool);
                if (toolInfo == null) continue;
                if (loadOut.ToolLoadOuts.TryGetValue(tool, out var value))
                {
                    foreach (var toolLoadOut in value.LoadOuts)
                    {
                        toolInfo.VerifyStatLoadOut(toolLoadOut);
                    }
                    continue;
                }
                loadOut.ToolLoadOuts[tool] = CreateNewLoadOutCollection(toolInfo);

            }
            return loadOut;
        }
        

        internal static RobotStatLoadOutCollection CreateNewLoadOutCollection(RobotUpgradeInfo robotUpgradeInfo)
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
                upgradeDataList.Add(node.InstanceData);
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
        
        public static float GetContinuousValue(RobotStatLoadOutCollection loadOut, int upgrade)
        {
            return loadOut.GetCurrent()?.GetCountinuousValue(upgrade) ?? 0;
        }


        public static Dictionary<int, int> GetAmountOfUpgrades(List<RobotUpgradeNodeData> nodeDataList, List<RobotUpgradeData> upgradeDataList)
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
        
        public static int GetAmountOfUpgrade(int upgrade, List<RobotUpgradeNodeData> nodeDataList, List<RobotUpgradeData> upgradeDataList)
        {
            int count = 0;
            foreach (RobotUpgradeNodeData upgradeNodeData in nodeDataList)
            {
                if (upgradeNodeData.UpgradeType != upgrade) continue;
                foreach (RobotUpgradeData upgradeData in upgradeDataList)
                {
                    if (upgradeData.Id != upgradeNodeData.Id) continue;
                    count += upgradeData.Amount;
                }
            }
            return count;
        }

        public static int GetVeinMinePower(float veinMineUpgrades)
        {
            return 1 + (int)(TILES_PER_VEIN_MINE_UPGRADE * veinMineUpgrades);
        }

        
    }
}