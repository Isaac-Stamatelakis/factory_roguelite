using Player;
using Player.Controls;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class TileHighligherIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
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
            tileImage.color = playerScript.TilePlacementOptions.Indiciator ? Color.blue : Color.gray;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string text = playerScript?.TilePlacementOptions?.Indiciator.ToString() ?? string.Empty;
            ToolTipController.Instance.ShowToolTip(transform.position, $"Placement Preview: {text}");
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
            playerScript.TilePlacementOptions.Indiciator = !playerScript.TilePlacementOptions.Indiciator;
            playerScript.TileViewers.SetPlacePreviewerState(playerScript.TilePlacementOptions.Indiciator);
            Refresh();
        }

        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.PlacePreview;
        }
    }
}
