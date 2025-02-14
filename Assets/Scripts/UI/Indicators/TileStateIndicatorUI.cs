using Tiles;
using UnityEngine;
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
        public void Display(int state)
        {
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
    }
}
