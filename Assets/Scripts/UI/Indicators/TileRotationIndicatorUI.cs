using System;
using Player;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileRotationIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image tileImage;
        private PlayerTilePlacementOptions displayedOptions;
        public void Display(PlayerTilePlacementOptions placementOptions)
        {
            displayedOptions = placementOptions;
            DisplayRotation();

        }

        private void DisplayRotation()
        {
            tileImage.transform.rotation = Quaternion.Euler(0,0 ,90*displayedOptions.Rotation);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Tile Rotation:  {90*displayedOptions?.Rotation}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            const int MAX_ROTATION = 3;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    displayedOptions.Rotation++;
                    if (displayedOptions.Rotation > MAX_ROTATION) displayedOptions.Rotation = 0;
                    break;
                case PointerEventData.InputButton.Right:
                    displayedOptions.Rotation--;
                    if (displayedOptions.Rotation < 0) displayedOptions.Rotation = MAX_ROTATION;
                    break;
                default:
                    return;
            }
            DisplayRotation();
            OnPointerEnter(eventData);
        }
    }
}
