using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.ToolTip
{
    public class ToolTipUIDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string message;
        private Func<string> messageAction;
        private bool focused;

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
            focused = true;
            Refresh();
        }

        public void Refresh()
        {
            string text = messageAction != null ? messageAction.Invoke() : message;
            ToolTipController.Instance.ShowToolTip(transform.position,text);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            focused = false;
            ToolTipController.Instance.HideToolTip();
        }

        public void OnDestroy()
        {
            if (focused) ToolTipController.Instance.HideToolTip();
        }
    }
}
