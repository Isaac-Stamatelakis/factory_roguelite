using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player.Controls;
using Player.Controls.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Controls
{
    public static class ControlUtils
    {
        private static readonly string PREF_PREFIX = "CONTROL_";

        private static Dictionary<PlayerControl, ModifierKeyCode?> requiredModifiers = new();
        private static Dictionary<PlayerControl, List<ModifierKeyCode>> blockedModifiers = new();

        public static HashSet<PlayerControl> GetConflictingBindings()
        {
            HashSet<PlayerControl> conflicts = new HashSet<PlayerControl>();
            var (controlDataDict, bindingConflictDict) = GetDataAndBindingConflicts();

            foreach (var (_, bindingConflicts) in bindingConflictDict)
            {
                for (int i = 0; i < bindingConflicts.Count; i++)
                {
                    var current = bindingConflicts[i];
                    var currentData =  controlDataDict[current];
                    for (int j = 0; j < bindingConflicts.Count; j++)
                    {
                        if (i == j) continue;
                        var other =  bindingConflicts[j];
                        var otherData = controlDataDict[other];
                        if (currentData.Modifier != otherData.Modifier) continue;
                        conflicts.Add(current);
                        conflicts.Add(other);
                    }
                }
            }
            return conflicts;
        }

        public static bool AllowModificationKeyCodes(PlayerControl playerControl)
        {
            switch (playerControl)
            {
                case PlayerControl.Jump:
                case PlayerControl.MoveLeft:
                case PlayerControl.MoveRight:
                case PlayerControl.MoveDown:
                case PlayerControl.MoveUp:
                    return false;
                default:
                    return true;
            }
        }

        private static (Dictionary<PlayerControl, PlayerControlData>, Dictionary<string,  List<PlayerControl>>)
            GetDataAndBindingConflicts()
        {
            var playerControls = System.Enum.GetValues(typeof(PlayerControl));
            Dictionary<PlayerControl, PlayerControlData> controlDataDict = new();
            Dictionary<string, List<PlayerControl>> bindingConflictDict = new();
            foreach (PlayerControl playerControl in playerControls)
            {
                PlayerControlData playerControlData = GetControlValue(playerControl);
                if (playerControlData == null) continue;
                controlDataDict[playerControl] = playerControlData;
                if (!bindingConflictDict.ContainsKey(playerControlData.KeyData))
                {
                    bindingConflictDict.Add(playerControlData.KeyData, new List<PlayerControl>());
                }
                bindingConflictDict[playerControlData.KeyData].Add(playerControl);
            }
            return (controlDataDict, bindingConflictDict);
        }

        public static void LoadRequiredAndBlocked()
        {
            requiredModifiers.Clear();
            blockedModifiers.Clear();
            
            var (controlDataDict, bindingConflictDict) = GetDataAndBindingConflicts();
            
            foreach (var (_, bindingConflicts) in bindingConflictDict)
            {
                for (int i = 0; i < bindingConflicts.Count; i++)
                {
                    var current = bindingConflicts[i];
                    var currentData = controlDataDict[current];
                    ModifierKeyCode? required = currentData.Modifier;
                    List<ModifierKeyCode> blocked = new List<ModifierKeyCode>();
                    for (int j = 0; j < bindingConflicts.Count; j++)
                    {
                        if (i == j) continue;
                        var other =  bindingConflicts[j];
                        var otherData =  controlDataDict[other];
                        if (!otherData.Modifier.HasValue) continue;
                        var otherModifier = otherData.Modifier.Value;
                        if (otherModifier == required) continue;
                        blocked.Add(otherModifier);
                    }
                    requiredModifiers[current] = required;
                    blockedModifiers[current] = blocked;
                }
            }

            //PrintRequiredAndBlocked(true);
        }

        public static void PrintRequiredAndBlocked(bool ignoreEmpty)
        {
            var playerControls = System.Enum.GetValues(typeof(PlayerControl));
            foreach (PlayerControl playerControl in playerControls)
            {
                ModifierKeyCode? required = requiredModifiers.GetValueOrDefault(playerControl);
                List<ModifierKeyCode> blocked = blockedModifiers.GetValueOrDefault(playerControl);
                if (ignoreEmpty && !required.HasValue && (blocked == null || blocked.Count == 0)) continue;
                
                string requiredText = required.HasValue ? required.Value.ToString() : "None";
                string blockedText = string.Empty;
                if (blocked == null || blocked.Count == 0)
                {
                    blockedText = "None";
                }
                else
                {
                    for (var index = 0; index < blocked.Count; index++)
                    {
                        var blockedModifier = blocked[index];
                        blockedText += blockedModifier.ToString();
                        if (index != blocked.Count - 1) blockedText += ", ";
                    }
                }
                
                Debug.Log($"{playerControl}: Required: {requiredText} & Blocked: {blockedText}");
                
            }
        }
        
        public static void AssignAction(InputAction inputAction, PlayerControl playerControl, Action<InputAction.CallbackContext> action)
        {
            inputAction.performed += ctx =>
            {
                if (requiredModifiers.TryGetValue(playerControl, out var required))
                {
                    if (required.HasValue && !ModifierActive(required.Value)) return;
                }
                if (blockedModifiers.TryGetValue(playerControl, out var blocked))
                {
                    foreach (ModifierKeyCode modifierKey in blocked)
                    {
                        if (ModifierActive(modifierKey)) return;
                    }
                }
                
                action.Invoke(ctx);
            };
        }

        public static bool ModifierActive(ModifierKeyCode modifierKey)
        {
            switch (modifierKey)
            {
                case ModifierKeyCode.Ctrl:
                    return Keyboard.current.ctrlKey.isPressed;
                case ModifierKeyCode.Shift:
                    return Keyboard.current.shiftKey.isPressed;
                case ModifierKeyCode.Alt:
                    return Keyboard.current.altKey.isPressed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifierKey), modifierKey, null);
            }
        }
        public static void SetKeyValue(PlayerControl playerControl, string keyData, ModifierKeyCode? modifierKeyCode)
        {
            PlayerControlData playerControlData = new PlayerControlData(keyData, modifierKeyCode);
            string data = JsonConvert.SerializeObject(playerControlData);
            string prefKey = GetPrefKey(playerControl);
            PlayerPrefs.SetString(prefKey, data);
        }
        
        public static string GetPrefKey(PlayerControl playerControl)
        {
            return PREF_PREFIX + playerControl.ToString().ToLower();
        }
        
        public static PlayerControlData GetControlValue(PlayerControl playerControl)
        {
            string data = PlayerPrefs.GetString(GetPrefKey(playerControl));
            try
            {
                return JsonConvert.DeserializeObject<PlayerControlData>(data);
            }
            catch (JsonException)
            {
                return null;
            }
        }


        public static InputActionBinding[] GetPlayerControlBinding(PlayerControl playerControl, InputActions inputActions)
        {
            switch (playerControl)
            {
                case PlayerControl.Jump:
                    return new InputActionBinding[]
                    {
                        new(inputActions.StandardMovement.Jump, 0)
                    };
                case PlayerControl.MoveLeft:
                    return new InputActionBinding[]
                    {
                        new(inputActions.StandardMovement.Move, 1),
                        new(inputActions.FlightMovement.Move, 3),
                    };
                case PlayerControl.MoveRight:
                    return new InputActionBinding[]
                    {
                        new(inputActions.StandardMovement.Move, 2),
                        new(inputActions.FlightMovement.Move, 4),
                    };
                case PlayerControl.MoveDown:
                    return new InputActionBinding[]
                    {
                        new(inputActions.StandardMovement.Down, 0),
                        new(inputActions.FlightMovement.Move, 2),
                        new(inputActions.LadderMovement.Move, 1),
                    };
                
                case PlayerControl.MoveUp:
                    return new InputActionBinding[]
                    {
                        new(inputActions.FlightMovement.Move, 1),
                        new(inputActions.LadderMovement.Move, 2),
                    };
                case PlayerControl.Teleport:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscMovement.Teleport, 0),
                    };
                case PlayerControl.SwitchToolMode:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.SwitchToolMode, 0),
                    };
                case PlayerControl.OpenConduitOptions:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.ConduitOptions, 0),
                    };
                case PlayerControl.SwitchPlacementMode:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.SwitchPlacementMode, 0),
                    };
                case PlayerControl.TerminateConduitGroup:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.TerminateConduitGroup, 0),
                    };
                case PlayerControl.SwitchConduitPortView:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.ConduitPortView, 0),
                    };
                case PlayerControl.ChangeConduitViewMode:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.ConduitView, 0),
                    };
                case PlayerControl.HideUI:
                    return new InputActionBinding[]
                    {
                        new(inputActions.CanvasController.Hide, 0),
                    };
                case PlayerControl.SwapToolLoadOut:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.SwitchToolMode, 0),
                    };
                case PlayerControl.SwapRobotLoadOut:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.SwitchRobotLoadout, 0),
                    };
                case PlayerControl.OpenQuestBook:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.QuestBook, 0),
                    };
                case PlayerControl.OpenInventory:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.Inventory, 0),
                    };
                case PlayerControl.OpenSearch:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.ItemSearch, 0),
                    };
                case PlayerControl.AutoSelect:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.AutoSelect, 0),
                    };
                case PlayerControl.OpenRobotLoadOut:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.OpenRobotLoadout, 0),
                    };
                case PlayerControl.OpenToolLoadOut:
                    return new InputActionBinding[]
                    {
                        new(inputActions.ToolBindings.OpenToolLoadout, 0),
                    };
                case PlayerControl.SwitchPlacementSubMode:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.SubPlacementMode, 0),
                    };
                case PlayerControl.PlacePreview:
                    return new InputActionBinding[]
                    {
                        new(inputActions.MiscKeys.ConduitOptions, 0),
                    };
                case PlayerControl.ShowItemRecipes:
                    return new InputActionBinding[]
                    {
                        new(inputActions.InventoryUtils.ShowRecipes, 0),
                    };
                case PlayerControl.ShowItemUses:
                    return new InputActionBinding[]
                    {
                        new(inputActions.InventoryUtils.ShowUses, 0),
                    };
                case PlayerControl.EditItemTag:
                    return new InputActionBinding[]
                    {
                        new(inputActions.InventoryUtils.EditTag, 0),
                    };
            }
            Debug.LogWarning($"{playerControl} is not synced to any bindings");
            return null;
        }
        
        /// <summary>
        /// Returns a human readable formatted string of a given player control
        /// </summary>
        /// <example>PlayerControl.AutoSelect => "Auto Select"</example>
        public static string FormatInputText(PlayerControl key)
        {
            PlayerControlData playerControlData = GetControlValue(key);
            if (playerControlData == null) return string.Empty;
            string modifierString = playerControlData.Modifier.HasValue ? playerControlData.Modifier.Value.ToString() + " " : string.Empty;
            return modifierString + InputControlPath.ToHumanReadableString(playerControlData?.KeyData, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        /// <summary>
        /// Returns a human readable formatted string of the input of a player control
        /// </summary>
        /// <example>PlayerControl.AutoSelect => "<Keyboard>\w" => "W"</example>
        public static string FormatControlText(PlayerControl key)
        {
            return GlobalHelper.AddSpaces(key.ToString());
        }
        
        
        public static void SetDefault()
        {
            /*
            Dictionary<string, ControlBindingCollection> sections = GetKeyBindingSections();
            foreach (var controlBinding in sections.Values)
            {
                controlBinding.SetDefault(true);
            }
            */
        }

        public static void InitializeKeyBindings(InputActions inputActions)
        {
            var playerControls = System.Enum.GetValues(typeof(PlayerControl));
            foreach (PlayerControl playerControl in playerControls)
            {
                PlayerControlData playerControlData = GetControlValue(playerControl);
                if (playerControlData == null) continue;
                InputActionBinding[] bindings = GetPlayerControlBinding(playerControl,inputActions);
                foreach (InputActionBinding inputActionBinding in bindings)
                {
                    inputActionBinding.InputAction.ApplyBindingOverride(inputActionBinding.BindingIndex,playerControlData.KeyData);
                }
            }
        }
        
    }
    public class PlayerControlData
    {
        public string KeyData;
        public ModifierKeyCode? Modifier;

        public PlayerControlData(string keyData, ModifierKeyCode? modifier)
        {
            KeyData = keyData;
            Modifier = modifier;
        }
    }
}
