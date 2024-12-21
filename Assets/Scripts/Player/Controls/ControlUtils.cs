using System;
using System.Collections.Generic;
using System.Linq;
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
                ["Movement"] = new MovementControlBindings(),
                ["Equips"] = new EquipmentControlBindings()
            };
            foreach (var kvp in sections)
            {
                kvp.Value.SetDefault(false);
            }
            return sections;
        }

        public static HashSet<string> GetConflictingBindings()
        {
            var sections = GetKeyBindingSections();
            HashSet<string> conflicts = new HashSet<string>();
            Dictionary<int,string> serializedKeyCodes = new Dictionary<int,string>();
            foreach (var kvp in sections)
            {
                List<string> sectionKeys = kvp.Value.GetBindingKeys();
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
        }

        public static int SerializeKeyCodes(List<KeyCode> keyCodes)
        {
            var sortedBySize = keyCodes.OrderByDescending(key => (int)key).ToList();
            int sum = 0;
            int mult = 1;
            for (int i = 0; i < sortedBySize.Count; i++)
            {
                sum +=  mult*(int)sortedBySize[i];
                mult *= 512;
            }
            return sum;
        }

        public static string KeyCodeListAsString(List<KeyCode> keyCodes)
        {
            string resultString = "";
            for (int i = 0; i < keyCodes.Count; i++)
            {
                string keyCodeString =  keyCodes[i].ToString();
                keyCodeString = keyCodeString.Replace("Left", "L").Replace("Right","R");
                resultString += keyCodeString;
                
                if (i < keyCodes.Count - 1)
                {
                    resultString += "+";
                }
            }

            return resultString;
        }
        
        public static List<KeyCode> GetKeyCodes(string key)
        {
            string prefKey = GetPrefKey(key);
            
            int value = PlayerPrefs.GetInt(prefKey);
            List<KeyCode> keyCodes = new List<KeyCode>();
            while (value > 0)
            {
                keyCodes.Add((KeyCode)(value%512));
                value /= 512;
            }
            
            return keyCodes;
        }

        public static void SetKeyValue(string key, List<KeyCode> keyCodes)
        {
            string prefKey = GetPrefKey(key);
            int sum = SerializeKeyCodes(keyCodes);
            PlayerPrefs.SetInt(prefKey, sum);
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
            return PREF_PREFIX + name;
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
