using System;
using Player;
using Tiles;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileStateIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image tileImage;
        [SerializeField] private Sprite baseSprite;
        [SerializeField] private Sprite slabSprite;
        [SerializeField] private Sprite slantSprite;
        [SerializeField] private Sprite stairSprite;
        private PlayerTilePlacementOptions tilePlacementOption;
        public void Display(PlayerTilePlacementOptions tilePlacementOptions)
        {
            this.tilePlacementOption = tilePlacementOptions;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            Sprite sprite = GetSprite(tilePlacementOption.State);
            if (ReferenceEquals(sprite, null))
            {
                tileImage.enabled = false;
                return;
            }

            tileImage.enabled = true;
            tileImage.sprite = sprite;
        }

        private Sprite GetSprite(int state)
        {
            switch (state)
            {
                case HammerTile.BASE_TILE_STATE:
                    return baseSprite;
                case HammerTile.SLAB_TILE_STATE:
                    return slabSprite;
                case HammerTile.SLANT_TILE_STATE:
                    return slantSprite;
                case HammerTile.STAIR_TILE_STATE:
                    return stairSprite;
                default:
                    return null;
            }
        }

        private string GetStateName(int state)
        {
            switch (state)
            {
                case HammerTile.BASE_TILE_STATE:
                    return "Tile";
                case HammerTile.SLAB_TILE_STATE:
                    return "Slab";
                case HammerTile.SLANT_TILE_STATE:
                    return "Slant";
                case HammerTile.STAIR_TILE_STATE:
                    return "Stair";
                default:
                    return null;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Tile Place State:  {GetStateName(tilePlacementOption?.State ?? 0)}",reverse:true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            const int MAX_STATE = 3;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    tilePlacementOption.State++;
                    if (tilePlacementOption.State > MAX_STATE) tilePlacementOption.State = 0;
                    break;
                case PointerEventData.InputButton.Right:
                    tilePlacementOption.State--;
                    if (tilePlacementOption.State < 0) tilePlacementOption.State = MAX_STATE;
                    break;
                default:
                    return;
            }
            RefreshDisplay();
            OnPointerEnter(eventData);
        }
    }
}
