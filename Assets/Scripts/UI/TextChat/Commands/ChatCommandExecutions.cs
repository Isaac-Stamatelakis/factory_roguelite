using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ItemModule;
using PlayerModule;

namespace UI.Chat {
    public static class ChatCommendExecutionExtension {
        public static void execute(this ChatCommand command, string[] parameters) {
            switch (command) {
                case ChatCommand.help:
                    executeHelp(parameters);
                    break;
                case ChatCommand.teleport:
                    executeTeleport(parameters);
                    break;
                case ChatCommand.spawn:
                    executeSpawn(parameters);
                    break;
                case ChatCommand.give:
                    executeGive(parameters);
                    break;
                case ChatCommand.setrobot:
                    executeSetRobot(parameters);
                    break;
                case ChatCommand.gamemode:
                    executeGamemode(parameters);
                    break;
                default:
                    Debug.LogWarning("No execute behavior for command :" + command);
                    break;
            }
        }

        private static void executeHelp(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            var commands = Enum.GetValues(typeof(ChatCommand));
            List<string> stringCommands = new List<string>();
            foreach (ChatCommand chatCommand in commands) {
                stringCommands.Add(chatCommand.ToString());
            }
            stringCommands.Sort();
            if (parameters.Length == 0) {
                string output = "";
                foreach (string stringCommand in stringCommands) {
                    output += stringCommand + ", ";
                }
                GlobalUIContainer.getInstance().getTextChatUI().sendMessage(output);
            } else {
                try {
                    int page = Convert.ToInt32(parameters[0]);
                    page --; // Page 1 -> 0, 2 -> 1, ...
                    if (page < 0 || page * ChatCommandUtils.CommandsPerHelpPage >= stringCommands.Count) {
                        chatUI.sendMessage("Page is not between 1 and " + ChatCommandUtils.getHelpPages());
                        return;
                    }
                    string output = "";
                    for (int i = 0; i < ChatCommandUtils.CommandsPerHelpPage; i++) {
                        int index = page * ChatCommandUtils.CommandsPerHelpPage + i;
                        if (index >= stringCommands.Count) {
                            break;
                        }
                        output += stringCommands[index].ToString() +  ", ";
                    }
                    chatUI.sendMessage(output);
                    
                } catch (FormatException) {
                    ChatCommand? paramCommand = ChatCommandFactory.getCommand(parameters[0]);
                    if (paramCommand == null) {
                        chatUI.sendMessage("Could not find command '" + parameters[0] + "'");
                        return;
                    }
                    chatUI.sendMessage(((ChatCommand)paramCommand).getDescription());
                } catch (OverflowException) {
                    chatUI.sendMessage("Page is too large");
                }
            }
        }

        private static void executeTeleport(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            try {
                int x = Convert.ToInt32(parameters[0]);
                int y = Convert.ToInt32(parameters[1]);
                Transform playerTransform = PlayerContainer.getInstance().getTransform();
                Vector3 playerPosition = playerTransform.position;
                playerPosition.x = x;
                playerPosition.y = y;
                playerTransform.position = playerPosition;
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
            catch (FormatException e) {
                GlobalUIContainer.getInstance().getTextChatUI().sendMessage(e.ToString());
            } catch (OverflowException e) {
                GlobalUIContainer.getInstance().getTextChatUI().sendMessage(e.ToString());
            }
        }

        private static void executeSpawn(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            try {
                string id = parameters[0];
                
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }

        private static void executeGive(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            try {
                string id = parameters[0];
                int amount = Convert.ToInt32(parameters[1]);
                amount = Mathf.Clamp(amount, 1,Global.MaxSize);
                PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
                ItemSlot toGive = ItemSlotFactory.createNewItemSlot(id,amount);
                if (toGive == null) {
                    chatUI.sendMessage("Invalid id");
                    return;
                }
                playerInventory.give(toGive);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            } catch (FormatException) {
                chatUI.sendMessage("Amount is not a number");
            } catch (OverflowException) {
                chatUI.sendMessage("Amount is too large");
            }
        }

        private static void executeSetRobot(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            try {
                string id = parameters[0];
                RobotItem robotItem = ItemRegistry.getInstance().GetRobotItem(id);
                if (robotItem == null) {
                    chatUI.sendMessage("Invalid id");
                    return;
                }
                ItemSlot itemSlot = ItemSlotFactory.createNewItemSlot(robotItem,1);
                PlayerContainer.getInstance().getPlayerRobot().setRobot(itemSlot);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }

        private static void executeGamemode(string[] parameters) {
            TextChatUI chatUI = GlobalUIContainer.getInstance().getTextChatUI();
            try {
                int mode = Convert.ToInt32(parameters[0]);
                
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            } catch (FormatException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }
    }
}

