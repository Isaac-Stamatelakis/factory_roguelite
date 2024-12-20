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
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                selectableKeys = ControlUtils.GetAllSelectableKeys();
            });
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
                if (Input.GetKey(keyCode))
                {
                    pressedKeys.Add(keyCode);
                }
            }
            
            

            if (pressedKeys.Count <= 0) return;
            selectableKeys = null;
            PlayerPrefs.SetInt(ControlUtils.GetPrefKey(key), (int) pressedKeys[0]);
            Display(key);
        }

        public void Display(string key)
        {
            this.key = key;
            text.text = ControlUtils.FormatKeyText(key);
            KeyCode keyCode = ControlUtils.GetPrefKeyCode(key);
            button.transform.GetComponentInChildren<TextMeshProUGUI>().text = keyCode.ToString();
        }
    }
}
