using UI.Catalogue.ItemSearch;
using UI.Indicators;
using UI.Indicators.General;
using UI.Indicators.Placement;
using UnityEngine;

namespace Player.UI
{
    public class PlayerUIContainer : MonoBehaviour
    {
        public IndicatorManager IndicatorManager;
        public TilePlacementIndicatorManagerUI TileIndicatorManagerUI;
        public Transform IndicatorContainer;
        public GameObject InventoryIndicatorPrefab;

        public void SyncKeyCodes()
        {
            IndicatorManager.SyncKeyCodes(false);
            TileIndicatorManagerUI.SyncKeyCodes(false);
        }
    }
}
