using Item.ItemObjects.Interfaces;
using Items;
using Player;
using UI.Indicators.General;
using UnityEngine;

namespace UI.Indicators.Placement
{
    public class TilePlacementIndicatorManagerUI : BaseIndiciatorManagerUI
    {
        private IPlacableItem item;
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TilePlacementIndicatorUI tilePlacementIndicatorUI;
        
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;

        public void Initialize(PlayerScript playerScript)
        {
            conduitPlacementModeIndicatorUI?.Initialize(playerScript);
            tilePlacementIndicatorUI.Initialize(playerScript);
            tilePreviewerIndicatorUI.Display(playerScript);
            
        }
        public void DisplayTile(PlayerScript playerScript, IPlacableItem displayItem)
        {
            this.item = displayItem;
            for (int i = 0; i < indicatorContainer.childCount; i++)
            {
                indicatorContainer.GetChild(i).gameObject.SetActive(false);
            }
            
            tilePreviewerIndicatorUI.gameObject.SetActive(true);
            if (displayItem is ConduitItem conduitItem)
            {
                conduitPlacementModeIndicatorUI.gameObject.SetActive(true);
                conduitPlacementModeIndicatorUI.Display(conduitItem);
            } else if (displayItem is TileItem tileItem)
            {
                tilePlacementIndicatorUI.gameObject.SetActive(true);
                tilePlacementIndicatorUI.Display(tileItem);
            }
            SyncKeyCodes(true);
        }
        
    }
}
