using System;
using Conduit.View;
using Conduits.Systems;
using Player;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPlacementModeIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image conduitImage;

        [SerializeField] private Sprite anyModeSprite;
        [SerializeField] private Sprite newModeSprite;
        private ConduitPlacementMode conduitPlacementMode;
        public void Display(ConduitPlacementOptions conduitPlacementOptions)
        {
            this.conduitPlacementMode = conduitPlacementOptions.PlacementMode;
            switch (conduitPlacementOptions.PlacementMode)
            {
                case ConduitPlacementMode.Any:
                    conduitImage.sprite = anyModeSprite;
                    break;
                case ConduitPlacementMode.New:
                    conduitImage.sprite = newModeSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Conduit Placement Mode: Connect {conduitPlacementMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
