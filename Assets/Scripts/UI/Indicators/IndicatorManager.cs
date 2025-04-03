
using System;
using Dimensions;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class IndicatorManager : MonoBehaviour
    {
        [SerializeField]
        public ConduitPortIndicatorUI conduitPortIndicatorUI;
        public ConduitViewIndicatorUI conduitViewIndicatorUI;
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TileRotationIndicatorUI tileRotationIndicatorUI;
        public TileStateIndicatorUI tileStateIndicatorUI;
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;

        public void Start()
        {
            conduitPortIndicatorUI = GetComponentInChildren<ConduitPortIndicatorUI>();
            conduitViewIndicatorUI = GetComponentInChildren<ConduitViewIndicatorUI>();
            conduitPlacementModeIndicatorUI = GetComponentInChildren<ConduitPlacementModeIndicatorUI>();
            tileRotationIndicatorUI = GetComponentInChildren<TileRotationIndicatorUI>();
            tileStateIndicatorUI = GetComponentInChildren<TileStateIndicatorUI>();
            tilePreviewerIndicatorUI = GetComponentInChildren<TileHighligherIndicatorUI>();
        }

        public void Initialize(PlayerScript playerScript)
        {
            conduitPortIndicatorUI?.Display(playerScript);
            conduitViewIndicatorUI?.Display(playerScript);
            conduitPlacementModeIndicatorUI?.Display(playerScript.ConduitPlacementOptions);
            tileRotationIndicatorUI.Display(playerScript.TilePlacementOptions);
            tileStateIndicatorUI.Display(playerScript.TilePlacementOptions);
            tilePreviewerIndicatorUI.Display(playerScript);
        }

        public void SetColor(Color color)
        {
            Image[] images = GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                image.color = color;
            }
        }
    }
}
