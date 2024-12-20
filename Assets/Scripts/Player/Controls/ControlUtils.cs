using System;
using System.Collections.Generic;
using Player.Controls.Bindings;
using UnityEngine;

namespace Player.Controls
{
    public static class ControlUtils
    {
        private static readonly string PREF_PREFIX = "CONTROL_";

        private static readonly HashSet<KeyCode> unselectableKeys = new HashSet<KeyCode>
        {
            KeyCode.Escape
        };
        public static Dictionary<string, ControlBindingCollection> GetKeyBindingSections()
        {
            Dictionary<string, ControlBindingCollection> sections = new Dictionary<string, ControlBindingCollection>
            {
                ["Movement"] = new MovementControlBindings()
            };
            foreach (var kvp in sections)
            {
                kvp.Value.SetDefault(false);
            }
            return sections;
        }

        public static List<KeyCode> GetAllSelectableKeys()
        {
            KeyCode[] keys = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
            List<KeyCode> result = new List<KeyCode>();
            foreach (KeyCode key in keys)
            {
                if (unselectableKeys.Contains(key))
                {
                    continue;
                }
                result.Add(key);
                
            }

            return result;
        }

        public static string GetPrefKey(string name)
        {
            return $"CONTROL_{name}";
        }

        public static KeyCode GetPrefKeyCode(string name)
        {
            return (KeyCode) PlayerPrefs.GetInt(GetPrefKey(name));
        }

        public static string FormatKeyText(string key)
        {
            return key.Replace("_"," ");
        }
        public static void SetDefault()
        {
            Dictionary<string, ControlBindingCollection> sections = GetKeyBindingSections();
            foreach (var controlBinding in sections.Values)
            {
                controlBinding.SetDefault(true);;
            }
        }

       
    }
}
