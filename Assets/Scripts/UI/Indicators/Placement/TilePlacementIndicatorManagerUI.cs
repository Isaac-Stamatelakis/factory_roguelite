using Item.ItemObjects.Interfaces;
using Items;
using Player;
using Tiles;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using UI.Indicators.General;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace UI.Indicators.Placement
{
    public class TilePlacementIndicatorManagerUI : BaseIndiciatorManagerUI
    {
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TileStateIndicatorUI tileStateIndicatorUI;
        public TileRotationIndicatorUI rotationIndicatorUI;
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;

        public void Initialize(PlayerScript playerScript)
        {
            conduitPlacementModeIndicatorUI?.Initialize(playerScript);
            tileStateIndicatorUI.Initialize(playerScript);
            rotationIndicatorUI.Initialize(playerScript);
            tilePreviewerIndicatorUI.Display(playerScript);
            
        }
        public void DisplayTile(PlayerScript playerScript, IPlacableItem displayItem)
        {
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
                if (tileItem.tile is IStateTile)
                {
                    tileStateIndicatorUI.gameObject.SetActive(true);
                    tileStateIndicatorUI.Display(tileItem);
                }

                if (tileItem.tileOptions.rotatable)
                {
                    TryDisplayRotation(tileItem);
                }
            }
            SyncKeyCodes(true);

            return;

            void TryDisplayRotation(TileItem tileItem)
            {
                if (tileItem.tile is not PlatformStateTile)
                {
                    // Cannot rotate flat platforms
                    int currentState = playerScript.TilePlacementOptions.State;
                    bool sloped = currentState >= (int)PlatformTileState.SlopeDeco;
                    if (!sloped) return;
                }
                rotationIndicatorUI.gameObject.SetActive(true);
                rotationIndicatorUI.Display(tileItem);
            } 
        }
        
    }
}
