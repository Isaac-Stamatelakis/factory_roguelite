using System;
using Player;
using PlayerModule.Mouse;
using Tiles;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileAutoSelectIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image image;
        private bool autoSelectActive;
        private PlayerMouse playerMouse;

        public void Initialize(PlayerMouse playerMouse)
        {
            this.playerMouse = playerMouse;
            Display(playerMouse.AutoSelectEnabled);
        }
        public void Display(bool autoSelectActive)
        {
            this.autoSelectActive = autoSelectActive;
            image.color = autoSelectActive ? Color.white : Color.gray;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string stateText = autoSelectActive ? "On" : "Off";
            ToolTipController.Instance.ShowToolTip(transform.position, $"Auto Select: {stateText}",reverse:true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            playerMouse.ToggleAutoSelect();
            autoSelectActive = !autoSelectActive;
            Display(autoSelectActive);
        }
    }
}
