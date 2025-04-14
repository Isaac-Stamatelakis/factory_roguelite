using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Player;
using Player.Tool;
using PlayerModule;
using Robot.Tool;
using Robot.Upgrades;
using Robot.Upgrades.Network;

namespace UI.Chat {
    [Obsolete]
    public class MaxUpgradeCommand : ChatCommand, IAutoFillChatCommand
    {
        public const string NAME = "upgrade";
        private const string MAX_MODIFIER = "max";
        private const string RESET_MODFIER = "reset";
        private const string ALL_TARGET = "all";
        private const string ROBOT_TARGET = "robot";
        private PlayerRobot playerRobot;
        public MaxUpgradeCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
            playerRobot = PlayerManager.Instance.GetPlayer().PlayerRobot;
        }

        public override void execute()
        {
            if (parameters.Length < 2) throw new ChatParseException("Invalid number of parameters");
            
            string modifier = parameters[0];
            List<string> modifiers = GetAllModifiers();
            if (!modifiers.Contains(modifier)) throw new ChatParseException("Invalid modifier");
            
            string target = parameters[1];
            List<string> targets = GetAllTargets();
            if (!targets.Contains(target)) throw new ChatParseException("Invalid target");
            
            List<List<RobotUpgradeData>> upgradeDataListCollection = new List<List<RobotUpgradeData>>();
            switch (target)
            {
                case ALL_TARGET:
                    upgradeDataListCollection.Add(playerRobot.RobotData.RobotUpgrades);
                    for (int i = 0; i < playerRobot.ToolTypes.Count; i++)
                    {
                        upgradeDataListCollection.Add(playerRobot.RobotData.ToolData.Upgrades[i]);
                    }
                    break;
                case ROBOT_TARGET:
                    upgradeDataListCollection.Add(playerRobot.RobotData.RobotUpgrades);
                    break;
                default: // TOOL
                    int toolIndex = targets.IndexOf(target) - 2;
                    upgradeDataListCollection.Add(playerRobot.RobotData.ToolData.Upgrades[toolIndex]);
                    break;
            }

            if (modifier == RESET_MODFIER)
            {
                foreach (List<RobotUpgradeData> upgradeDataList in upgradeDataListCollection)
                {
                    ResetUpgrades(upgradeDataList);
                }
            }
            List<string> upgradePaths = new List<string>();
            switch (target)
            {
                case ALL_TARGET:
                    upgradePaths.Add(playerRobot.CurrentRobot.UpgradePath);
                    upgradeDataListCollection.Add(playerRobot.RobotData.RobotUpgrades);
                    foreach (var robotTool in playerRobot.RobotTools)
                    {
                        upgradePaths.Add(robotTool.GetToolObject().UpgradePath);
                    }
                    break;
                case ROBOT_TARGET:
                    upgradePaths.Add(playerRobot.CurrentRobot.UpgradePath);
                    upgradeDataListCollection.Add(playerRobot.RobotData.RobotUpgrades);
                    break;
                default: // TOOL
                    int toolIndex = targets.IndexOf(target) - 2;
                    upgradePaths.Add(playerRobot.RobotTools[toolIndex].GetToolObject().UpgradePath);
                    break;
            }

            for (int i = 0; i < upgradePaths.Count; i++)
            {
                MaxUpgrades(upgradeDataListCollection[i], upgradePaths[i]);
            }
        }

        void ResetUpgrades(List<RobotUpgradeData> upgradeDataList)
        {
            Debug.Log("HI");
            foreach (RobotUpgradeData upgrade in upgradeDataList)
            {
                upgrade.Amount = 0;
            }
        }

        void MaxUpgrades(List<RobotUpgradeData> upgradeDataList, string upgradePath)
        {
            SerializedRobotUpgradeNodeNetwork network = RobotUpgradeUtils.DeserializeRobotNodeNetwork(upgradePath);
            
            if (network == null)
            {
                return;
            }
        
            ResetUpgrades(upgradeDataList);
            foreach (RobotUpgradeNodeData robotUpgradeNode in network.NodeData)
            {
                foreach (RobotUpgradeData robotUpgradeData in upgradeDataList)
                {
                    if (robotUpgradeNode.UpgradeType == robotUpgradeData.Id)
                    {
                        robotUpgradeData.Amount += robotUpgradeNode.UpgradeAmount;
                    }   
                }
            }
            
        }

        public override string getDescription()
        {
            List<string> targets = GetAllTargets();
            string targetMessage = "(";
            for (int i = 0; i < targets.Count; i++)
            {
                targetMessage += $"'{targets[i]}'";
                if (i != targets.Count - 1)
                {
                    targetMessage += " or ";
                }
            }
            targetMessage += ")";
            return $"/{NAME} ('{MAX_MODIFIER}' or '{RESET_MODFIER}') {targetMessage}\nModifies Robot Upgrades";
        }
        

        private List<string> GetAllTargets()
        {
            List<string> targets = new List<string>
            {
                ALL_TARGET,
                ROBOT_TARGET
            };
            foreach (RobotToolType robotToolType in playerRobot.ToolTypes)
            {
                targets.Add(robotToolType.ToString().ToLower());
            } 
            return targets;
        }

        private List<string> GetAllModifiers()
        {
            return new List<string>() { MAX_MODIFIER, RESET_MODFIER };
        }

        public List<string> getAutoFill(int paramIndex)
        {
            if (paramIndex == 0)
            {
                return GetAllModifiers();
            }

            if (paramIndex == 1)
            {
                return GetAllTargets();
            }

            return null;
        }
    }
}

