using Tiles;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileStateIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image tileImage;
        [SerializeField] private Sprite baseSprite;
        [SerializeField] private Sprite slabSprite;
        [SerializeField] private Sprite slantSprite;
        [SerializeField] private Sprite stairSprite;
        private int currentState;
        public void Display(int state)
        {
            currentState = state;
            Sprite sprite = GetSprite(state);
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
            ToolTipController.Instance.ShowToolTip(transform.position, $"Tile Place State:  {GetStateName(currentState)}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
