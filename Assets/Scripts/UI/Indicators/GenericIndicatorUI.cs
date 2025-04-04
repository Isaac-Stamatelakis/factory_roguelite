using System;
using Player.Controls;
using UI.ToolTip;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Indicators
{
    public class GenericIndicatorUI : MonoBehaviour, IKeyCodeIndicator, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private PlayerControl? playerControl;
        private string highlightText;
        private Action onClick;
            public void Initialize(PlayerControl? playerControl, string highlightText, Action onClick)
        {
            this.playerControl = playerControl;
            this.highlightText = highlightText;
            this.onClick = onClick;
        }
            
        public PlayerControl? GetPlayerControl()
        {
            return playerControl;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position,highlightText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
