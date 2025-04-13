using Player;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class TileHighligherIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image tileImage;
        private PlayerScript playerScript;
        public void Display(PlayerScript playerScript)
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
            playerScript.TilePlacementOptions.Indiciator = !playerScript.TilePlacementOptions.Indiciator;
            OnPointerEnter(eventData);
            playerScript.TileViewers.SetPlacePreviewerState(playerScript.TilePlacementOptions.Indiciator);
            Refresh();
        }
    }
}
