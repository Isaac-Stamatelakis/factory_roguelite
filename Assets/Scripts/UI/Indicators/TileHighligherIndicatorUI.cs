using Player;
using Tiles;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileHighligherIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image tileImage;
        private bool active;
        public void Display(PlayerTilePlacementOptions tilePlacementOptions)
        {
            this.active = tilePlacementOptions.Indiciator;
            tileImage.color = tilePlacementOptions.Indiciator ? Color.blue : Color.gray;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Placement Preview:  {(active ? "Active" : "Inactive")}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
