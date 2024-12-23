
using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls
{
    public abstract class ControlBindingCollection
    {
        public void SetDefault(bool overrideValues)
        {
            List<ControlBinding> bindings = GetBindings();
            foreach (ControlBinding binding in bindings)
            {
                SetKeyCode(binding.Key, binding.DefaultKeyCode, overrideValues);
            }
        }
        protected abstract List<ControlBinding> GetBindings();

        public List<string> GetBindingKeys()
        {
            List<ControlBinding> bindings = GetBindings();
            List<string> keys = new List<string>();
            foreach (ControlBinding binding in bindings)
            {
                keys.Add(binding.Key);
            }

            return keys;
        }

        protected void SetKeyCode(string key, List<KeyCode> keyCodes, bool overrideValue)
        {
            string prefKey = ControlUtils.GetPrefKey(key);
            if (overrideValue || PlayerPrefs.GetInt(prefKey) == 0)
            {
                ControlUtils.SetKeyValue(key,keyCodes);
            }
        }

        protected struct ControlBinding
        {
            public string Key;
            public List<KeyCode> DefaultKeyCode;

            public ControlBinding(string key, List<KeyCode> defaultKeyCode)
            {
                Key = key;
                DefaultKeyCode = defaultKeyCode;
            }
        }
    }
}
