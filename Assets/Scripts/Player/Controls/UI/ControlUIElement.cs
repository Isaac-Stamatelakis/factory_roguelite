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
        private InputActions inputActions;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private InputActionBinding[] inputActionBindings;
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                button.transform.GetComponentInChildren<TextMeshProUGUI>().text  = "Press any key...";
                rebindOperation?.Dispose();

                inputActionBindings ??= ControlUtils.GetPlayerControlBinding(key, inputActions);
                
                if (inputActionBindings.Length == 0) return;
                var inputActionBinding = inputActionBindings[0];
                rebindOperation = inputActionBinding.InputAction.PerformInteractiveRebinding()
                    .WithTargetBinding(inputActionBinding.BindingIndex)
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

            if (!operationSuccess)
            {
                button.transform.GetComponentInChildren<TextMeshProUGUI>().text = ControlUtils.FormatInputText(key);
                return;
            }

            ModifierKeyCode? modifier = null;
            var modifierKeyCodes = System.Enum.GetValues(typeof(ModifierKeyCode));
            foreach (ModifierKeyCode modifierKeyCode in modifierKeyCodes)
            {
                if (!ControlUtils.ModifierActive(modifierKeyCode)) continue;
                modifier = modifierKeyCode;
                break;
            }
            Debug.Log(modifier);
            var inputActionBinding = inputActionBindings[0];
            string path = inputActionBinding.InputAction.bindings[inputActionBinding.BindingIndex].effectivePath;

            for (int i = 1; i < inputActionBindings.Length; i++)
            {
                var otherBinding = inputActionBindings[i];
                otherBinding.InputAction.ApplyBindingOverride(otherBinding.BindingIndex,path);
            }
            
            ControlUtils.SetKeyValue(key, path,modifier);
            button.transform.GetComponentInChildren<TextMeshProUGUI>().text = ControlUtils.FormatInputText(key);
        }

        public void HighlightConflictState(bool conflict)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = conflict ? Color.red : Color.white;
        }

        public void Display()
        {
            text.text = ControlUtils.FormatControlText(key);
            string formatString = ControlUtils.FormatInputText(key);
            button.transform.GetComponentInChildren<TextMeshProUGUI>().text = formatString;
        }
        public void Initalize(PlayerControl key, InputActions inputActions)
        {
            this.key = key;
            this.inputActions = inputActions;
            Display();
        }
    }

    
    public struct InputActionBinding
    {
        public InputAction InputAction;
        public int BindingIndex;

        public InputActionBinding(InputAction inputAction, int bindingIndex)
        {
            InputAction = inputAction;
            BindingIndex = bindingIndex;
        }
    }
}
