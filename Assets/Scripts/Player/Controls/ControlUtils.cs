using System;
using System.Collections.Generic;
using System.Linq;
using Player.Controls.Bindings;
using Unity.VisualScripting;
using UnityEngine;

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
        private static Dictionary<string, KeyCode[]> controlDict;
        private static HashSet<KeyCode> singleKeyCodes;
        private static int modifierCount = 0;

        public static void UpdateModifierCount()
        {
            modifierCount = 0;
            if (Input.GetKey(KeyCode.LeftShift)) modifierCount ++;
            if (Input.GetKey(KeyCode.RightShift)) modifierCount ++;
            if (Input.GetKey(KeyCode.LeftControl)) modifierCount ++;
            if (Input.GetKey(KeyCode.RightControl)) modifierCount ++;
            if (Input.GetKey(KeyCode.LeftAlt)) modifierCount ++;
            if (Input.GetKey(KeyCode.RightAlt)) modifierCount ++;
        }

        private static readonly HashSet<KeyCode> unselectableKeys = new HashSet<KeyCode>
        {
            KeyCode.Escape
        };
        public static Dictionary<string, ControlBindingCollection> GetKeyBindingSections()
        {
            Dictionary<string, ControlBindingCollection> sections = new Dictionary<string, ControlBindingCollection>
            {
                ["Movement"] = new MovementControlBindings(),
                ["Equips"] = new EquipmentControlBindings(),
                ["Conduit"] = new ConduitControlBindings(),
                ["Misc"] = new MiscBindings()
            };
            foreach (var kvp in sections)
            {
                kvp.Value.SetDefault(false);
            }
            return sections;
        }

        public static void LoadBindings()
        {
            controlDict = new Dictionary<string, KeyCode[]>();
            singleKeyCodes = new HashSet<KeyCode>();
            var keyCountDict = new Dictionary<KeyCode, int>();
            var sections = GetKeyBindingSections();
            foreach (var (name, bindings) in sections)
            {
                foreach (string binding in bindings.GetBindingKeys())
                {
                    KeyCode[] keyCodes = GetKeyCodes(binding).ToArray();
                    controlDict[binding] = keyCodes;
                    KeyCode primary = keyCodes.Last();
                    keyCountDict.TryAdd(primary, 0);

                    keyCountDict[primary]++;
                }
            }

            foreach (var (keycode, count) in keyCountDict)
            {
                if (count == 1) singleKeyCodes.Add(keycode);
            }
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

        public static bool GetControlKey(string name)
        {
            if (controlDict == null) return false;
            controlDict.TryGetValue(name, out KeyCode[] keycodes);
            if (keycodes == null) return false;
            if (keycodes.Length == 1)
            {
                KeyCode keyCode = keycodes[0];
                if (!singleKeyCodes.Contains(keyCode) && modifierCount > 0) return false;
                return Input.GetKey(keycodes.Last());
            }
            for (int i = 0; i < keycodes.Length-1; i++)
            {
                if (!Input.GetKey(keycodes[i])) return false;
            }
            return Input.GetKey(keycodes.Last());
        }
        
        public static bool GetControlKeyDown(string name)
        {
            if (controlDict == null) return false;
            controlDict.TryGetValue(name, out KeyCode[] keycodes);
            if (keycodes == null) return false;
            if (keycodes.Length == 1)
            {
                KeyCode keyCode = keycodes[0];
                return !(!singleKeyCodes.Contains(keyCode) && modifierCount > 0) && Input.GetKeyDown(keycodes.Last());
            }
            for (int i = 0; i < keycodes.Length-1; i++)
            {
                if (!Input.GetKey(keycodes[i])) return false;
            }
            return Input.GetKeyDown(keycodes.Last());
        }
        
        public static string FormatKeyText(string key)
        {
            return key.Replace("_"," ").FirstCharacterToUpper();
        }
        public static void SetDefault()
        {
            Dictionary<string, ControlBindingCollection> sections = GetKeyBindingSections();
            foreach (var controlBinding in sections.Values)
            {
                controlBinding.SetDefault(true);
            }
        }

       
    }
}
