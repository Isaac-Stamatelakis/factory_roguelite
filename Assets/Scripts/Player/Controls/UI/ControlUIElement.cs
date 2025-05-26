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
        private PlayerControlBinding playerControlBinding;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                button.transform.GetComponentInChildren<TextMeshProUGUI>().text  = "Press any key...";
                rebindOperation?.Dispose();

                // Create new rebind operation
                rebindOperation = playerControlBinding.InputAction.PerformInteractiveRebinding()
                    .WithTargetBinding(playerControlBinding.BindingIndex)
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
            string path = playerControlBinding.InputAction.bindings[playerControlBinding.BindingIndex].effectivePath;
            ControlUtils.SetKeyValue(key, path);
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
        public void Initalize(PlayerControl key, ControlSettingUI controlSettingUI, PlayerControlBinding playerControlBinding)
        {
            this.controlSettingUI = controlSettingUI;
            this.key = key;
            this.playerControlBinding = playerControlBinding;
            Display();
        }
    }

    public struct PlayerControlBinding
    {
        public InputAction InputAction;
        public int BindingIndex;

        public PlayerControlBinding(InputAction inputAction, int bindingIndex)
        {
            InputAction = inputAction;
            BindingIndex = bindingIndex;
        }
    }
}
