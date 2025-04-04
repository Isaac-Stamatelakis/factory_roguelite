using System;
using Conduit.View;
using Conduits.Systems;
using Player;
using Player.Controls;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPlacementModeIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [SerializeField] private Image conduitImage;

        [SerializeField] private Sprite anyModeSprite;
        [SerializeField] private Sprite newModeSprite;
        private ConduitPlacementOptions conduitPlacementOptions;
        public void Display(ConduitPlacementOptions conduitPlacementOptions)
        {
            this.conduitPlacementOptions = conduitPlacementOptions;
            Refresh();
        }

        public void Refresh()
        {
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
            ToolTipController.Instance.ShowToolTip(transform.position, $"Conduit Placement Mode: Connect {conduitPlacementOptions?.PlacementMode}",reverse:true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    conduitPlacementOptions.PlacementMode = GlobalHelper.ShiftEnum(1, conduitPlacementOptions.PlacementMode);
                    break;
                case PointerEventData.InputButton.Right:
                    conduitPlacementOptions.PlacementMode = GlobalHelper.ShiftEnum(-1, conduitPlacementOptions.PlacementMode);
                    break;
                default:
                    return;
            }
            Refresh();
            OnPointerEnter(eventData);
           
        }

        public PlayerControl? GetPlayerControl()
        {
            return PlayerControl.SwitchConduitPlacementMode;
        }
    }
}
