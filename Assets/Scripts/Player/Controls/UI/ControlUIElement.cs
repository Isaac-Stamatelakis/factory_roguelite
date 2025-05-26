using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player.Controls.UI
{
    public class ControlUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;
        private PlayerControl key;
        private InputActions inputActions;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private InputActionBinding[] inputActionBindings;

        private bool listening;
        private bool focused;

        public void Update()
        {
            if (focused)
            {
               
            }
            
        }

        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                listening = true;
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
                    .OnMatchWaitForAnother(.15f)
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
            listening = false;
            if (!operationSuccess)
            {
                button.transform.GetComponentInChildren<TextMeshProUGUI>().text = ControlUtils.FormatInputText(key);
                return;
            }

            ModifierKeyCode? modifier = null;
            string primaryPath = null;
            Dictionary<string, ModifierKeyCode> keyPathModifierDict = new Dictionary<string, ModifierKeyCode>
            {
                { "/Keyboard/shift", ModifierKeyCode.Shift },
                { "/Keyboard/alt", ModifierKeyCode.Alt },
                { "/Keyboard/ctrl", ModifierKeyCode.Ctrl },
            };

            List<string> ignorePaths = new List<string>
            {
                "/Keyboard/anyKey",
                "/Keyboard/leftShift",
                "/Keyboard/rightShift",
                "/Keyboard/leftCtrl",
                "/Keyboard/rightCtrl",
                "/Keyboard/leftAlt",
                "/Keyboard/rightAlt",
            };
            foreach (var candiate in rebindOperation.candidates)
            {
                string candiatePath = candiate.path;
                if (ignorePaths.Contains(candiatePath)) continue;
                if (keyPathModifierDict.TryGetValue(candiatePath, out var value))
                {
                    modifier = value;
                    continue;
                }
                
                primaryPath = candiatePath;
            }

            Debug.Log(primaryPath);
            if (string.IsNullOrEmpty(primaryPath))
            {
                button.transform.GetComponentInChildren<TextMeshProUGUI>().text = ControlUtils.FormatInputText(key);
                return;
            }
            Debug.Log("A");
            
            foreach (var binding in inputActionBindings)
            {
                binding.InputAction.ApplyBindingOverride(binding.BindingIndex,primaryPath);
            }

            ControlUtils.SetKeyValue(key, primaryPath, modifier);
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            focused = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            focused = false;
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
