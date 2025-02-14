using System;
using Conduit.View;
using Conduits.Systems;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPlacementModeIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image conduitImage;

        [SerializeField] private Sprite anyModeSprite;
        [SerializeField] private Sprite newModeSprite;
        public void Display(ConduitPlacementOptions conduitPlacementOptions)
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
    }
}
