using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Items;
using PlayerModule;
using Entities;
using Dimensions;
using Entities.Mobs;
using UnityEngine.Rendering.Universal;
using TileMaps;

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
                case ChatCommand.setlight:
                    executeLightMode(parameters);
                    break;
                case ChatCommand.setdim:
                    executeSetDim(parameters);
                    break;
                default:
                    Debug.LogWarning("No execute behavior for command :" + command);
                    break;
            }
        }

        private static void executeHelp(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
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
                TextChatUI.Instance.sendMessage(output);
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
            TextChatUI chatUI = TextChatUI.Instance;
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
                TextChatUI.Instance.sendMessage(e.ToString());
            } catch (OverflowException e) {
                TextChatUI.Instance.sendMessage(e.ToString());
            }
        }

        private static void executeSpawn(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
            try {
                Vector3 playerPosition = PlayerContainer.getInstance().getTransform().position;
                string id = parameters[0];
                Transform player = PlayerContainer.getInstance().getTransform();
                int dim = DimensionManager.Instance.getPlayerDimension(player);
                DimController dimController = DimensionManager.Instance.getDimController(dim);
                EntityRegistry.getInstance().spawnEntity(id,playerPosition,null,dimController.EntityContainer);
                
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }

        private static void onEntityReady(Entity entity) {
            Debug.Log(entity.name);
        }

        private static void executeGive(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
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
            TextChatUI chatUI = TextChatUI.Instance;
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
            TextChatUI chatUI = TextChatUI.Instance;
            try {
                int mode = Convert.ToInt32(parameters[0]);
                
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            } catch (FormatException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }

        private static void executeLightMode(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
            Light2D globalLight = GameObject.Find("GlobalLight").GetComponent<Light2D>();
            double intensity;
            Color color;
            try {
                intensity = Convert.ToDouble(parameters[0]);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Intensity not provided");
                return;
            } catch (FormatException) {
                chatUI.sendMessage("Intensity is not a number");
                return;
            }
            globalLight.intensity = (float) intensity;
            if (parameters.Length == 1) {
                globalLight.color = Color.white;
                return;
            }
            int r = getColorInt(parameters,1,chatUI,"Red");
            int g = getColorInt(parameters,2,chatUI,"Green");
            int b = getColorInt(parameters,3,chatUI,"Blue");
            if (Mathf.Min(r,g,b) < 0) { // Atleast one value will be -1 if an error occurs
                return;
            }
            color = new Color(r/255f,g/255f,b/255f);
            globalLight.color = color;
        }

        private static int getColorInt(string[] parameters, int index, TextChatUI chatUI, string parameterName) {
            try {
                return Mathf.Clamp(Convert.ToInt32(parameters[index]),0,255);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage($"{parameterName} not provided");
            } catch {
                chatUI.sendMessage($"{parameterName} is not a number");
            }
            return -1;
        }

        private static void executeSetDim(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
            int? dim = null; 
            int x = 0;
            int y = 0;
            try {
                dim = Convert.ToInt32(parameters[0]);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage($"Dimension not provided");
            } catch (FormatException) {
                chatUI.sendMessage($"Dimension is not a number");
            }
            if (dim == null) {
                return;
            }
            if (parameters.Length > 1) {
                try {
                    x = Convert.ToInt32(parameters[1]);
                    y = Convert.ToInt32(parameters[2]);
                }  catch (IndexOutOfRangeException) {
                    chatUI.sendMessage($"X and Y are not both provided");
                } catch (FormatException) {
                    chatUI.sendMessage($"One of X,Y is not a number");
                }
            }
            Transform player = PlayerContainer.getInstance().getTransform();
            DimensionManager.Instance.setPlayerSystem(player,(int)dim,new Vector2Int(x,y));
        }

        private static void executeOutline(string[] parameters) {
            TextChatUI chatUI = TextChatUI.Instance;
            DimensionManager dimensionManager = DimensionManager.Instance;
            bool wireFrame = true;
            OutlineTileGridMap[] outlineTileGridMaps = dimensionManager.GetComponentsInChildren<OutlineTileGridMap>();
            foreach (OutlineTileGridMap outlineTileGridMap in outlineTileGridMaps) {
                
            }
        }
    }
}

