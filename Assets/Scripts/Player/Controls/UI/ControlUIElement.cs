using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Controls.UI
{
    public class ControlUIElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;
        private string key;
        private List<KeyCode> selectableKeys;
        public int count;
        private List<KeyCode> cachedKeys;
        private ControlSettingUI controlSettingUI;
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                if (cachedKeys != null)
                {
                    return;
                }
                count = 12;
                cachedKeys = new List<KeyCode>();
                selectableKeys = ControlUtils.GetAllSelectableKeys();
            });
        }

        public void FixedUpdate()
        {
            if (cachedKeys == null || cachedKeys.Count == 0)
            {
                return;
            }
            count--;
            if (count < 0)
            {
                selectableKeys = null;
                ControlUtils.SetKeyValue(key,cachedKeys);
                controlSettingUI.CheckConflicts();
                cachedKeys = null;
                Display();
            }
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
            List<KeyCode> pressedKeys = new List<KeyCode>();

            if (Input.GetKey(KeyCode.Escape))
            {
                selectableKeys = null;
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
            string formatString = ControlUtils.KeyCodeListAsString(keyCodes);
            button.transform.GetComponentInChildren<TextMeshProUGUI>().text = formatString;
        }
        public void Initalize(string key, ControlSettingUI controlSettingUI)
        {
            this.controlSettingUI = controlSettingUI;
            this.key = key;
            Display();
        }
    }
}
