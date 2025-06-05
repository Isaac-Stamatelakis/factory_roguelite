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

            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            string modifier = parameters[0];
            List<string> modifiers = GetAllModifiers();
            if (!modifiers.Contains(modifier)) throw new ChatParseException("Invalid modifier");
            
            string target = parameters[1];
            List<string> targets = GetAllTargets();
            if (!targets.Contains(target)) throw new ChatParseException("Invalid target");

            List<UpgradePathNodeValues> upgradeData = new List<UpgradePathNodeValues>();
            switch (target)
            {
                case ALL_TARGET:
                    upgradeData.Add(FromPlayer());
                    for (var index = 0; index < playerRobot.ToolTypes.Count; index++)
                    {
                        upgradeData.Add(FromTool(index));
                    }
                    break;
                case ROBOT_TARGET:
                    upgradeData.Add(FromPlayer());
                    break;
                default: // TOOL
                    int toolIndex = targets.IndexOf(target) - 2;
                    upgradeData.Add(FromTool(toolIndex));
                    break;
            }


            if (modifier == RESET_MODFIER)
            {
                foreach (UpgradePathNodeValues upgradeValues in upgradeData)
                {
                    RobotUpgradeUtils.ResetUpgrades(playerScript,upgradeValues.UpgradePath,upgradeValues.Nodes);
                }
            } else if (modifier == MAX_MODIFIER)
            {
                foreach (UpgradePathNodeValues upgradeValues in upgradeData)
                {
                    RobotUpgradeUtils.MaxOutUpgrades(playerScript,upgradeValues.UpgradePath,upgradeValues.Nodes);
                }
            }


            return;
            UpgradePathNodeValues FromPlayer()
            {
                return new UpgradePathNodeValues(playerRobot.CurrentRobot.UpgradePath, playerScript.PlayerRobot.RobotData.RobotUpgrades);
            }
            
            UpgradePathNodeValues FromTool(int toolIndex)
            {
                var upgradeNodes = playerRobot.RobotData.ToolData.Upgrades[toolIndex];
                var tool = playerScript.PlayerRobot.RobotTools[toolIndex];
                return new UpgradePathNodeValues(tool.GetToolObject().UpgradePath, upgradeNodes);
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

        private struct UpgradePathNodeValues
        {
            public string UpgradePath;
            public List<RobotUpgradeData> Nodes;

            public UpgradePathNodeValues(string upgradePath, List<RobotUpgradeData> nodes)
            {
                UpgradePath = upgradePath;
                Nodes = nodes;
            }
        }
    }
}

