using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player.Controls.UI
{
    public class ControlUIElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;
        private PlayerControl key;
        private List<KeyCode> selectableKeys;
        public int listenUpdates;
        private List<KeyCode> cachedKeys;
        private ControlSettingUI controlSettingUI;
        private InputAction inputAction;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                
            });
        }

        

        public void HighlightConflictState(bool conflict)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = conflict ? Color.red : Color.white;
        }

        public void Update()
        {
            if (selectableKeys == null)
            {
                return;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                ControlUtils.SetKeyValue(key,new List<KeyCode>());
                controlSettingUI.CheckConflicts();
                Display();
                cachedKeys = null;
                selectableKeys = null;
                listenUpdates = 0;
                return;
            }
            foreach (KeyCode keyCode in selectableKeys)
            {
                if (Input.GetKey(keyCode) && !cachedKeys.Contains(keyCode))
                {
                    cachedKeys.Add(keyCode);
                }
            }
        }

        public void Display()
        {
            text.text = ControlUtils.FormatKeyText(key);
            List<KeyCode> keyCodes = ControlUtils.GetKeyCodes(key);
            string formatString = ControlUtils.KeyCodeListAsString(keyCodes,"+");
            button.transform.GetComponentInChildren<TextMeshProUGUI>().text = formatString;
        }
        public void Initalize(PlayerControl key, ControlSettingUI controlSettingUI)
        {
            this.controlSettingUI = controlSettingUI;
            this.key = key;
            Display();
        }
    }
}
