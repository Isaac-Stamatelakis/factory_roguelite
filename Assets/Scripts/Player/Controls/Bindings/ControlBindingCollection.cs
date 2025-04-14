
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

        public List<PlayerControl> GetBindingKeys()
        {
            List<ControlBinding> bindings = GetBindings();
            List<PlayerControl> keys = new List<PlayerControl>();
            foreach (ControlBinding binding in bindings)
            {
                keys.Add(binding.Key);
            }

            return keys;
        }

        protected void SetKeyCode(PlayerControl key, List<KeyCode> keyCodes, bool overrideValue)
        {
            string prefKey = ControlUtils.GetPrefKey(key);
            if (overrideValue || PlayerPrefs.GetInt(prefKey) == 0)
            {
                ControlUtils.SetKeyValue(key,keyCodes);
            }
        }

        protected struct ControlBinding
        {
            public PlayerControl Key;
            public List<KeyCode> DefaultKeyCode;

            public ControlBinding(PlayerControl key, List<KeyCode> defaultKeyCode)
            {
                Key = key;
                DefaultKeyCode = defaultKeyCode;
            }
        }
    }

    public class UserInterfaceBindingCollection : ControlBindingCollection
    {
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new(PlayerControl.OpenInventory, new List<KeyCode> { KeyCode.Tab }),
                new(PlayerControl.OpenSearch, new List<KeyCode> { KeyCode.I }),
                new(PlayerControl.OpenQuestBook, new List<KeyCode> { KeyCode.L }),
                new(PlayerControl.SwitchPlacementMode, new List<KeyCode> { KeyCode.H }),
                new(PlayerControl.SwitchPlacementSubMode, new List<KeyCode> { KeyCode.H, KeyCode.LeftControl}),
            };
        }
    }
}
