using Conduit.Placement.LoadOut;
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
        public ConduitLoadoutIndicatorUI conduitLoadoutIndicatorUI;
        public TileHighligherIndicatorUI tileHighligherIndicatorUI;
        public TileSearchIndicatorUI tileSearchIndicatorUI;
        

        public void Initialize(PlayerScript playerScript)
        {
            conduitPlacementModeIndicatorUI?.Initialize(playerScript);
            tileStateIndicatorUI.Initialize(playerScript,rotationIndicatorUI);
            rotationIndicatorUI.Initialize(playerScript);
            conduitLoadoutIndicatorUI.Initialize(playerScript);
            tileHighligherIndicatorUI.Initialize(playerScript);
            tileSearchIndicatorUI.Initialize(playerScript);
            
        }
        public void DisplayTile(PlayerScript playerScript, IPlacableItem displayItem)
        {
            for (int i = 0; i < indicatorContainer.childCount; i++)
            {
                indicatorContainer.GetChild(i).gameObject.SetActive(false);
            }
            tileHighligherIndicatorUI.gameObject.SetActive(true);
            
            if (displayItem is ConduitItem conduitItem)
            {
                if (conduitItem.GetConduitType() == ConduitType.Matrix) return;
                conduitPlacementModeIndicatorUI.gameObject.SetActive(true);
                conduitPlacementModeIndicatorUI.Display(conduitItem);
                conduitLoadoutIndicatorUI.gameObject.SetActive(true);
                conduitLoadoutIndicatorUI.Display(LoadOutConduitTypeExtension.FromConduitType(conduitItem.GetConduitType()));
            } else if (displayItem is TileItem tileItem)
            {
                if (tileItem.tile is HammerTile or PlatformStateTile)
                {
                    tileStateIndicatorUI.gameObject.SetActive(true);
                    tileStateIndicatorUI.Display(tileItem);
                }

                if (tileItem.tileOptions.rotatable && tileItem.tile is not IMousePositionStateTile)
                {
                    TryDisplayRotation(tileItem);
                }
            }

            if (playerScript.PlayerMouse.TileSearchResultCacher.HasSearcher)
            {
                tileSearchIndicatorUI.gameObject.SetActive(true);
            }

            if (AllInactive())
            {
                gameObject.SetActive(false);
                return;
            }
            SyncKeyCodes(true);
            return;

            void TryDisplayRotation(TileItem tileItem)
            {
                /*
                if (tileItem.tile is PlatformStateTile)
                {
                    // Cannot rotate flat platforms
                    int currentState = playerScript.TilePlacementOptions.State;
                    bool sloped = currentState >= (int)PlatformTileState.SlopeDeco;
                    if (!sloped) return;
                }
                */
                rotationIndicatorUI.gameObject.SetActive(true);
                rotationIndicatorUI.Display(tileItem);
            }

            bool AllInactive()
            {
                for (int i = 0; i < indicatorContainer.childCount; i++)
                {
                    if (indicatorContainer.GetChild(i).gameObject.activeInHierarchy) return false;
                }

                return true;
            }
        }
        
    }
}
