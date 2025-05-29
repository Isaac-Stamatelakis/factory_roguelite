using Item.ItemObjects.Interfaces;
using Player;
using UI.Indicators.General;
using UnityEngine;

namespace UI.Indicators.Placement
{
    public class TilePlacementIndicatorManagerUI : MonoBehaviour
    {
        private IPlacableItem item;
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TilePlacementIndicatorUI tilePlacementIndicatorUI;
        
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;
        
        public void DisplayTile(PlayerScript playerScript, IPlacableItem displayItem)
        {
            this.item = displayItem;
            conduitPlacementModeIndicatorUI?.Initialize(playerScript);
            tilePlacementIndicatorUI.Initialize(playerScript);
            tilePreviewerIndicatorUI.Display(playerScript);
        }
        
    }
}
