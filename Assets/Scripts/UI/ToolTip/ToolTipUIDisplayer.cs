using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.ToolTip
{
    public class ToolTipUIDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string message;
        private Func<string> messageAction;

        public void SetMessage(string message)
        {
            this.message = message;
        }

        public void SetAction(Func<string> action)
        {
            this.messageAction = action;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            string text = messageAction != null ? messageAction.Invoke() : message;
            ToolTipController.Instance.ShowToolTip(transform.position,text);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
