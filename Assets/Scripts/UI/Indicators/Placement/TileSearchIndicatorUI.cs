using Player;
using Player.Controls;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class TileSearchIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [SerializeField] private Image tileImage;
        private PlayerScript playerScript;
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            Refresh();
        }

        private void Refresh()
        {
            tileImage.color = playerScript.TilePlacementOptions.AutoPlace ? Color.yellow : Color.gray;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string text = playerScript?.TilePlacementOptions?.AutoPlace.ToString() ?? string.Empty;
            ToolTipController.Instance.ShowToolTip(transform.position, $"Smart Placement: {text}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
            OnPointerEnter(eventData);
        }
        
        public void Toggle()
        {
            playerScript.TilePlacementOptions.AutoPlace = !playerScript.TilePlacementOptions.AutoPlace;
            playerScript.TileViewers.TilePlacePreviewer.ClearPlacementRecord();
            Refresh();
        }

        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.SmartPlace;
        }
    }
}
