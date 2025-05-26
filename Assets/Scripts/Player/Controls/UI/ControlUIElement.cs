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
                button.interactable = false;
                text.text = "Press any key...";
                rebindOperation?.Dispose();

                // Create new rebind operation
                rebindOperation = inputAction.PerformInteractiveRebinding()
                    .WithControlsExcluding("<Mouse>/position")
                    .WithControlsExcluding("<Mouse>/delta")
                    .WithCancelingThrough("<Keyboard>/escape")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => {
                        UpdateBindingDisplay(true);
                        operation.Dispose();
                    })
                    .OnCancel(operation => {
                        
                        UpdateBindingDisplay(false);
                        operation.Dispose();
                    })
                    .Start();
            });
        }

        private void UpdateBindingDisplay(bool operationSuccess)
        {
            button.interactable = true;
            if (!operationSuccess) return;
            string bindingsJson = inputAction.SaveBindingOverridesAsJson();
            ControlUtils.SetKeyValue(key, bindingsJson);
            text.text = ControlUtils.FormatKeyText(key);
        }

        public void HighlightConflictState(bool conflict)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = conflict ? Color.red : Color.white;
        }

        public void Display()
        {
            text.text = ControlUtils.FormatKeyText(key);
            string formatString = ControlUtils.FormatKeyText(key);
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
