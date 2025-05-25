using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Player.Controls;
using Player.Controls.Bindings;
using PlayerModule.KeyPress;
using TMPro;
using UI.PauseScreen;
using UI.ToolTip;
using UnityEngine;

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
        private bool canTerminate;
        private AudioSource audioSource;
        public bool IsTyping => isTyping;
        private PlayerScript playerScript;
        public void Awake()
        {
            instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayAudioClip(UIAudioClipType audioClipType)
        {
            AudioClip clip = uiAudioElements.GetClip(audioClipType);
            audioSource.PlayOneShot(clip);
        }
        public abstract void EmptyListen();
        public abstract void ListenKeyPresses();

        public void SetPlayerScript(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }
        public void AddTypingListener(TMP_InputField tmpInputField)
        {
            tmpInputField.onSelect.AddListener((text) =>
            {
                playerScript.SyncKeyPressListeners(IsActive,true);
                isTyping = true;
            });
            
            tmpInputField.onDeselect.AddListener((text) =>
            {
                playerScript.SyncKeyPressListeners(IsActive,false);
                isTyping = false;
            });
        }
        private bool isTyping;
        public bool BlockKeyInput => (uiObjectStack.Count > 0 && uiObjectStack.Peek().blockMovement) || isTyping;

        public void Update()
        {
            if (Input.GetKeyDown(ControlUtils.GetPrefKeyCode(PlayerControl.HideUI)))
            {
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                parentCanvas.enabled = !parentCanvas.enabled;
            }
            if (!canTerminate) // Prevents instant terminating if key to activate ui element is the same that destroys it
            {
                canTerminate = true;
                return;
            }
            
            if (uiObjectStack.Count == 0)
            {
                EmptyListen();
                return;
            }
            ListenKeyPresses();
            if (uiObjectStack.Count == 0)
            {
                return;
            }

            if (isTyping) return;
            DisplayedUIInfo top = uiObjectStack.Peek();
            List<KeyCode> additionalTerminators = top.additionalTerminators;
            if (additionalTerminators == null) return;
            
            foreach (KeyCode key in additionalTerminators)
            {
                if (Input.GetKeyDown(key))
                {
                    StartCoroutine(DelayStartPopStack());
                    return;
                }
            }
        }

        public bool CanEscapePop()
        {
            return Input.GetKeyDown(KeyCode.Escape) && uiObjectStack.Count > 0 && uiObjectStack.Peek().termianteOnEscape;
        }

        private IEnumerator DelayStartPopStack()
        {
            yield return null;
            PopStack();

        }

        public void ClearStack()
        {
            if (ToolTipController.Instance) ToolTipController.Instance.HideToolTip();
            isTyping = false;
            while (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo top = uiObjectStack.Pop();
                Destroy(top.gameObject);
            }
            playerScript?.SyncKeyPressListeners(false,isTyping);
            mBlocker?.gameObject.SetActive(false);
            
        }
        public void PopStack()
        {
            if (uiObjectStack.Count == 0)
            {
                return;
            }
            isTyping = false;
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
                playerScript?.SyncKeyPressListeners(false,isTyping);
                mBlocker?.gameObject.SetActive(false);
            }
        }

        public bool TopHasComponent<T>() where T : Component
        {
            if (uiObjectStack.Count == 0) return false;
            return !ReferenceEquals(uiObjectStack.Peek().gameObject.GetComponent<T>(), null);
        }

        public void DisplayObject(GameObject uiObject, List<KeyCode> keyCodes = null, bool hideOnStack = true, bool hideParent = true, Transform originalParent = null, bool terminateOnEscape = true, bool blockMovement = true, bool blocker = true)
        {
            DisplayObject(new DisplayedUIInfo
            {
                gameObject = uiObject,
                additionalTerminators = keyCodes,
                originalParent = originalParent,
                hideOnStack = hideOnStack,
                hideParent = hideParent,
                termianteOnEscape = terminateOnEscape,
                blockMovement = blockMovement,
                blocker = blocker
            });
        }

        private void DisplayObject(DisplayedUIInfo uiInfo)
        {
            playerScript?.SyncKeyPressListeners(true,isTyping);
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
            canTerminate = false;
            uiInfo.gameObject.transform.SetParent(transform,false);
            uiObjectStack.Push(uiInfo);
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

    public class DisplayedUIInfo
    {
        public GameObject gameObject;
        public List<KeyCode> additionalTerminators;
        public bool hideOnStack;
        public bool hideParent;
        public Transform originalParent;
        public bool termianteOnEscape;
        public bool blockMovement;
        public bool blocker;
    }
    
}
