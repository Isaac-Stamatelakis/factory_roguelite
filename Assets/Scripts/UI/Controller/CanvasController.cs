using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Player.Controls;
using PlayerModule.KeyPress;
using TMPro;
using UI.PauseScreen;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public enum UIAudioClipType
    {
        Button
    }
    public abstract class CanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject mBlocker;
        [SerializeField] private UIAudioElements uiAudioElements;
        protected static CanvasController instance;
        public static CanvasController Instance => instance;
        protected Stack<DisplayedUIInfo> uiObjectStack = new Stack<DisplayedUIInfo>();
        public bool IsActive => uiObjectStack.Count > 0;
        private bool blockMovement => uiObjectStack.Count > 0 && uiObjectStack.Peek().blockMovement;
        
        private AudioSource audioSource;
        private PlayerScript playerScript;
        private InputActions inputActions;
        public InputActions InputActions => inputActions;
        public InputAction exitAction;
        
        public void Awake()
        {
            instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            inputActions = new InputActions();
            GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerScript>()?.SetInputActions(inputActions);
            
            var canvasKeyPresses = inputActions.CanvasController;
            exitAction = canvasKeyPresses.Exit;
            canvasKeyPresses.Exit.performed += OnEscapePress;
            canvasKeyPresses.Hide.performed += HidePress;
            canvasKeyPresses.Enable();
        }
        
        public void OnDestroy()
        {
            var canvasKeyPresses = inputActions.CanvasController;
            canvasKeyPresses.Exit.performed -= OnEscapePress;
            canvasKeyPresses.Hide.performed -= HidePress;
            inputActions.Dispose();
        }

        public void PlayAudioClip(UIAudioClipType audioClipType)
        {
            AudioClip clip = uiAudioElements.GetClip(audioClipType);
            audioSource.PlayOneShot(clip);
        }
        public void SetPlayerScript(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }
        public void AddTypingListener(TMP_InputField tmpInputField)
        {
            tmpInputField.onSelect.AddListener((text) =>
            {
                playerScript?.SyncKeyPressListeners(IsActive,true, blockMovement);
            });
            
            tmpInputField.onDeselect.AddListener((text) =>
            {
                if (!playerScript) return; // Required lifetime check so the game doesn't crash when exiting whilst typing
                playerScript.SyncKeyPressListeners(IsActive,false, blockMovement);
            });
        }

        public void OnEscapePress(InputAction.CallbackContext context)
        {
            if (uiObjectStack.Count == 0)
            {
                OnInactiveEscapePress();
                return;
            }
            OnEscapePress();
        }


        protected abstract void OnInactiveEscapePress();
        protected abstract void OnEscapePress();
        public void HidePress(InputAction.CallbackContext context)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            parentCanvas.enabled = !parentCanvas.enabled;
        }
        
        public void ClearStack()
        {
            if (ToolTipController.Instance) ToolTipController.Instance.HideToolTip();
            while (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo top = uiObjectStack.Pop();
                Destroy(top.gameObject);
            }
            playerScript?.SyncKeyPressListeners(false,false, blockMovement);
            mBlocker?.gameObject.SetActive(false);
            UpdateExitAction();
            
        }
        public void PopStack()
        {
            if (uiObjectStack.Count == 0)
            {
                return;
            }
            if (ToolTipController.Instance) ToolTipController.Instance.HideToolTip();
            DisplayedUIInfo top = uiObjectStack.Pop();
            if (ReferenceEquals(top.originalParent, null))
            {
                Destroy(top.gameObject);
            }
            else
            {
                Transform topObject = transform.GetChild(transform.childCount-1); // Use this as top.gameObject may be destroyed and top is always last in the heirarchy
                topObject.SetParent(top.originalParent,false);
                topObject.gameObject.SetActive(false);
            }
            
            if (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo newTop = uiObjectStack.Peek();
                newTop.gameObject.SetActive(true);
            }
            else
            {
                playerScript?.SyncKeyPressListeners(false,false, blockMovement);
                mBlocker?.gameObject.SetActive(false);
            }
            UpdateExitAction();
        }

        public bool TopHasComponent<T>() where T : Component
        {
            if (uiObjectStack.Count == 0) return false;
            return !ReferenceEquals(uiObjectStack.Peek().gameObject.GetComponent<T>(), null);
        }

        public void DisplayObject(GameObject uiObject, PlayerControl? keyCodes = null, ContextPathWrapper terminatorContextPath = null, bool hideOnStack = true, bool hideParent = true, Transform originalParent = null, bool terminateOnEscape = true, bool blockMovement = true, bool blocker = true)
        {
            DisplayObject(new DisplayedUIInfo
            {
                gameObject = uiObject,
                additionalTerminator = keyCodes,
                originalParent = originalParent,
                hideOnStack = hideOnStack,
                hideParent = hideParent,
                termianteOnEscape = terminateOnEscape,
                blockMovement = blockMovement,
                blocker = blocker,
                TerminateContextPathBinding = terminatorContextPath,
            });
        }

        private void DisplayObject(DisplayedUIInfo uiInfo)
        {
            if (ToolTipController.Instance) ToolTipController.Instance.HideToolTip();
            
            if (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo current = uiObjectStack.Peek();
                try
                {
                    bool hide = current.hideOnStack && uiInfo.hideParent;
                    current.gameObject.SetActive(!hide);
                }
                catch (MissingReferenceException e)
                {
                    Debug.LogWarning($"MainCanvasController tried to access already destroyed ui object\n{e.Message}");
                    uiObjectStack.Pop();
                }
            }

            mBlocker?.gameObject.SetActive(uiInfo.blocker);
            uiInfo.gameObject.transform.SetParent(transform,false);
            uiObjectStack.Push(uiInfo);
            
            playerScript?.SyncKeyPressListeners(true,false, blockMovement);
            UpdateExitAction();
        }

        public void UpdateExitAction()
        {
            if (uiObjectStack.Count == 0)
            {
                ResetExitAction();
                return;
            }
            DisplayedUIInfo uiInfo = uiObjectStack.Peek();
            if (!uiInfo.termianteOnEscape && !uiInfo.additionalTerminator.HasValue && uiInfo.TerminateContextPathBinding == null)
            {
                Debug.LogWarning($"Tried to override exit action for '{uiInfo.gameObject.name}' with no terminator");
                ResetExitAction();
                return;
            }
            exitAction.RemoveAllBindingOverrides();
            if (uiInfo.termianteOnEscape)
            {
                exitAction.ApplyBindingOverride(0,"/Keyboard/Escape");
            }

            if (uiInfo.additionalTerminator.HasValue)
            {
                PlayerControlData playerControlData = ControlUtils.GetControlValue(uiInfo.additionalTerminator.Value);
                if (playerControlData != null)
                {
                    exitAction.ApplyBindingOverride(1,playerControlData.KeyData);
                }
                
            }
            
            var contextWrapper = uiInfo.TerminateContextPathBinding;
            if (contextWrapper == null) return;
            var controlPath = contextWrapper.GetBinding();
            if (!string.IsNullOrEmpty(controlPath))
            {
                exitAction.ApplyBindingOverride(2,controlPath);
            }
        }

        public void ResetExitAction()
        {
            exitAction.RemoveAllBindingOverrides();
            //exitAction.ApplyBindingOverride("<Keyboard>/Escape");
        }
        
        public void DisplayOnParentCanvas(GameObject displayObject)
        {
            displayObject.transform.SetParent(transform.parent.parent, false);
            displayObject.transform.SetAsLastSibling();
        }

        [System.Serializable]
        private class UIAudioElements
        {
            public AudioClip ButtonClick;

            public AudioClip GetClip(UIAudioClipType audioClipType)
            {
                switch (audioClipType)
                {
                    case UIAudioClipType.Button:
                        return ButtonClick;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(audioClipType), audioClipType, null);
                }
            }
        }
    }

    public struct DisplayedUIInfo
    {
        public GameObject gameObject;
        public PlayerControl? additionalTerminator;
        public bool hideOnStack;
        public bool hideParent;
        public Transform originalParent;
        public bool termianteOnEscape;
        public bool blockMovement;
        public bool blocker;
        public ContextPathWrapper TerminateContextPathBinding;
    }

    public class ContextPathWrapper
    {
        private readonly string path;
        public string GetBinding()
        {
            return path;
        }

        public ContextPathWrapper(ref InputAction.CallbackContext context)
        {
            this.path = context.control.path;
        }
    }
    
}
