using System.Collections.Generic;
using UI.PauseScreen;
using UI.ToolTip;
using UnityEngine;

namespace UI
{
    public abstract class CanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject mBlocker;
        protected static CanvasController instance;
        public static CanvasController Instance => instance;
        private Stack<DisplayedUIInfo> uiObjectStack = new Stack<DisplayedUIInfo>();
        public bool IsActive => uiObjectStack.Count > 0;
        private bool canTerminate;
        public void Awake()
        {
            instance = this;
        }

        public abstract void EmptyListen();
        public abstract void ListenKeyPresses();
        
        

        public void Update()
        {
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
            DisplayedUIInfo top = uiObjectStack.Peek();
            List<KeyCode> additionalTerminators = top.additionalTerminators;
            if (additionalTerminators == null) return;
            
            foreach (KeyCode key in additionalTerminators)
            {
                if (Input.GetKeyDown(key))
                {
                    PopStack();
                    return;
                }
            }
        }

        public void ClearStack()
        {
            ToolTipController.Instance.HideToolTip();
            while (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo top = uiObjectStack.Pop();
                Destroy(top.gameObject);
            }
            mBlocker.gameObject.SetActive(false);
            
        }
        public void PopStack()
        {
            
            if (uiObjectStack.Count == 0)
            {
                return;
            }
            ToolTipController.Instance.HideToolTip();
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
                mBlocker.gameObject.SetActive(false);
            }
            
        }

        public bool TopHasComponent<T>() where T : Component
        {
            if (uiObjectStack.Count == 0) return false;
            return !ReferenceEquals(uiObjectStack.Peek().gameObject.GetComponent<T>(), null);
        }

        public void DisplayObject(GameObject uiObject, List<KeyCode> keyCodes = null, bool hideOnStack = true, bool hideParent = true, Transform originalParent = null)
        {
            DisplayObject(new DisplayedUIInfo(uiObject,keyCodes,hideOnStack,hideParent,originalParent));
        }

        private void DisplayObject(DisplayedUIInfo uiInfo)
        {
            ToolTipController.Instance.HideToolTip();
            mBlocker.gameObject.SetActive(true);
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

            canTerminate = false;
            uiInfo.gameObject.transform.SetParent(transform,false);
            uiObjectStack.Push(uiInfo);
        }
        
        public void DisplayOnParentCanvas(GameObject displayObject)
        {
            displayObject.transform.SetParent(transform.parent, false);
        }
    }

    public class DisplayedUIInfo
    {
        public GameObject gameObject;
        public List<KeyCode> additionalTerminators;
        public bool hideOnStack;
        public bool hideParent;
        public Transform originalParent;

        public DisplayedUIInfo(GameObject gameObject, List<KeyCode> additionalTerminators, bool hideOnStack, bool hideParent, Transform originalParent)
        {
            this.gameObject = gameObject;
            this.additionalTerminators = additionalTerminators;
            this.hideOnStack = hideOnStack;
            this.hideParent = hideParent;
            this.originalParent = originalParent;
        }
    }
    
}
