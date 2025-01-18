using System.Collections.Generic;
using Item.ItemObjects.Instances.Tile.Chisel;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Item.ItemObjects.Instances.Tiles.Chisel
{
    public class ChiselCatalogueInfoUI : CatalogueInfoUI
    {
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private TextMeshProUGUI mTitleText;
        private ChiselCatalogueInfo chiselCatalogueInfo;
        public override void Display(ICatalogueElement element)
        {
            chiselCatalogueInfo = (ChiselCatalogueInfo)element;
            DisplayPage(0);
        }

        public override void DisplayPage(int pageIndex)
        {
            ChiselDisplayData chiselDisplayData = chiselCatalogueInfo.DisplayDataList[pageIndex];
            mTitleText.text = $"{chiselDisplayData.ChiselCollectionObject.name} Chisel Tiles";

            List<ItemSlot> slots = new List<ItemSlot>();
            foreach (ChiselTileItem itemObject in chiselDisplayData.ChiselCollectionObject.ChiselTiles)
            {
                slots.Add(new ItemSlot(itemObject,1,null));
            }
            mInventoryUI.DisplayInventory(slots);
            mInventoryUI.SetInteractMode(InventoryInteractMode.Recipe);

        }
    }
}
