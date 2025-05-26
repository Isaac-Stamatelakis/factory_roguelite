using System;
using System.Collections.Generic;
using System.Linq;
using Player.Controls;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Controls
{
    public enum ValidAdditionalKeys
    {
        LShift = 1,
        RShift = 2,
        Ctrl = 4,
        Alt = 8,
        
    }
    public static class ControlUtils
    {
        private static readonly string PREF_PREFIX = "CONTROL_";
        
        private static readonly HashSet<KeyCode> unselectableKeys = new HashSet<KeyCode>
        {
            KeyCode.Escape
        };
       
        

        public static HashSet<PlayerControl> GetConflictingBindings()
        {
            /*
            var sections = GetKeyBindingSections();
            HashSet<PlayerControl> conflicts = new HashSet<PlayerControl>();
            Dictionary<int,PlayerControl> serializedKeyCodes = new Dictionary<int,PlayerControl>();
            foreach (var kvp in sections)
            {
                List<PlayerControl> sectionKeys = kvp.Value.GetBindingKeys();
                foreach (var key in sectionKeys)
                {
                    int value = PlayerPrefs.GetInt(GetPrefKey(key));
                    if (serializedKeyCodes.TryGetValue(value, out var code))
                    {
                        conflicts.Add(key);
                        conflicts.Add(code);
                        continue;
                    }
                    serializedKeyCodes.Add(value, key);
                }
            }
            return conflicts;
            */
            return new HashSet<PlayerControl>();
        }

        public static void RebindKeys(InputActions inputActions)
        {
            
        }
        
        public static void SetKeyValue(PlayerControl playerControl, string data)
        {
            string prefKey = GetPrefKey(playerControl);
            PlayerPrefs.SetString(prefKey, data);
        }
        
        public static string GetPrefKey(PlayerControl playerControl)
        {
            return PREF_PREFIX + playerControl.ToString().ToLower();
        }
        
        public static string GetControlValue(PlayerControl playerControl)
        {
            return PlayerPrefs.GetString(GetPrefKey(playerControl));
        }


        public static InputAction GetInputAction(PlayerControl playerControl, InputActions inputActions)
        {
            switch (playerControl)
            {
                case PlayerControl.Jump:
                    return inputActions.StandardMovement.Jump;
                case PlayerControl.MoveLeft:
                    return inputActions.StandardMovement.Move;
                case PlayerControl.MoveRight:
                    break;
                case PlayerControl.MoveDown:
                    break;
                case PlayerControl.MoveUp:
                    break;
                case PlayerControl.Teleport:
                    break;
                case PlayerControl.Recall:
                    break;
                case PlayerControl.SwitchToolMode:
                    break;
                case PlayerControl.OpenConduitOptions:
                    break;
                case PlayerControl.SwitchPlacementMode:
                    break;
                case PlayerControl.TerminateConduitGroup:
                    break;
                case PlayerControl.SwitchConduitPortView:
                    break;
                case PlayerControl.ChangeConduitViewMode:
                    break;
                case PlayerControl.HideUI:
                    break;
                case PlayerControl.SwapToolLoadOut:
                    break;
                case PlayerControl.SwapRobotLoadOut:
                    break;
                case PlayerControl.OpenQuestBook:
                    break;
                case PlayerControl.OpenInventory:
                    break;
                case PlayerControl.OpenSearch:
                    break;
                case PlayerControl.AutoSelect:
                    break;
                case PlayerControl.OpenRobotLoadOut:
                    break;
                case PlayerControl.OpenToolLoadOut:
                    break;
                case PlayerControl.SwitchPlacementSubMode:
                    break;
                case PlayerControl.PlacePreview:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerControl), playerControl, null);
            }

            return null;
        }
        
        public static string FormatKeyText(PlayerControl key)
        {
            string keyJson = GetControlValue(key);
            InputControlPath.ToHumanReadableString(keyJson, InputControlPath.HumanReadableStringOptions.OmitDevice);
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
    }
}
