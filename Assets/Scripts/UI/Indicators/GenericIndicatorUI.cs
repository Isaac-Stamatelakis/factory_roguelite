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
        private Func<string> highlightStringAction;
        private Action onClick;
            public void Initialize(PlayerControl? playerControl, Func<string> highlightStringAction, Action onClick)
        {
            this.playerControl = playerControl;
            this.highlightStringAction = highlightStringAction;
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
            ToolTipController.Instance.ShowToolTip(transform.position,highlightStringAction?.Invoke());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
