using System.Collections.Generic;
using UI.PauseScreen;
using UnityEngine;

namespace UI
{
    public abstract class CanvasController : MonoBehaviour
    {
        protected static CanvasController instance;
        public static CanvasController Instance => instance;
        private Stack<DisplayedUIInfo> uiObjectStack = new Stack<DisplayedUIInfo>();
        public bool IsActive => uiObjectStack.Count > 0;
        public void Awake()
        {
            instance = this;
        }

        public abstract void EmptyListen();
        public abstract void ListenKeyPresses();
        
        

        public void Update()
        {
            
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
                if (Input.GetKey(key))
                {
                    PopStack();
                    return;
                }
            }
        }

        public void PopStack()
        {
            if (uiObjectStack.Count == 0)
            {
                return;
            }
            DisplayedUIInfo top = uiObjectStack.Pop();
            GameObject.Destroy(top.gameObject);
            
            if (uiObjectStack.Count > 0)
            {
                DisplayedUIInfo newTop = uiObjectStack.Peek();
                newTop.gameObject.SetActive(true);
            }
        }

        public void DisplayObject(GameObject uiObject, List<KeyCode> keyCodes = null, bool hideOnStack = true, bool hideParent = true)
        {
            DisplayObject(new DisplayedUIInfo(uiObject,keyCodes,hideOnStack,hideParent));
        }

        private void DisplayObject(DisplayedUIInfo uiInfo)
        {
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
            uiInfo.gameObject.transform.SetParent(transform,false);
            uiObjectStack.Push(uiInfo);
        }
    }

    public class DisplayedUIInfo
    {
        public GameObject gameObject;
        public List<KeyCode> additionalTerminators;
        public bool hideOnStack;
        public bool hideParent;

        public DisplayedUIInfo(GameObject gameObject, List<KeyCode> additionalTerminators, bool hideOnStack, bool hideParent)
        {
            this.gameObject = gameObject;
            this.additionalTerminators = additionalTerminators;
            this.hideOnStack = hideOnStack;
            this.hideParent = hideParent;
        }
    }
    
}
