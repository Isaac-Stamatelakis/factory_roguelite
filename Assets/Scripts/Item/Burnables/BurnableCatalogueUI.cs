using System;
using System.Collections.Generic;
using Item.GameStage;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Transmutable;
using Player;
using Recipe;
using Recipe.Collection;
using TMPro;
using UI.Catalogue.InfoViewer;
using UnityEngine;
using BurnableItemRegistry = Item.Registry.BurnableItemRegistry;

namespace Item.Burnables
{
    public class BurnableCatalogueUI : CatalogueInfoUI
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        [SerializeField] private TextMeshProUGUI mBurnTimeText;
        [SerializeField] private InventoryUI mItemSlotInventoryUI;
        private BurnableInfo burnableInfo;
        public override void Display(ICatalogueElement element, PlayerGameStageCollection gameStages)
        {
            burnableInfo = (BurnableInfo)element;
            DisplayPage(0);
        }

        public override void DisplayPage(int pageIndex)
        {
            BurnableDisplay burnableDisplay = burnableInfo.BurnableItems[pageIndex];
            switch (burnableDisplay)
            {
                case BurnableItemDisplay burnableItemDisplay:
                    DisplayItemSlot(burnableItemDisplay.ItemSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DisplayItemSlot(ItemSlot itemSlot)
        {
            mItemSlotInventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},clear:false);
            mItemSlotInventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
            ItemObject itemObject = itemSlot.itemObject;
            mNameText.text = $"Name: {itemObject.name}";
            uint burnTicks = ItemRegistry.BurnableItemRegistry.GetBurnDuration(itemObject);
            mBurnTimeText.text = $"Burn Time: {burnTicks}T";
        }
    }

    public abstract class BurnableDisplay : IGameStageItemDisplay
    {
        public abstract bool FilterStage(PlayerGameStageCollection gameStageCollection);
    }

    public interface IGameStageItemDisplay
    {
        public bool FilterStage(PlayerGameStageCollection gameStageCollection);
    }
    public class BurnableItemDisplay : BurnableDisplay
    {
        public ItemSlot ItemSlot;
        public BurnableItemDisplay(ItemSlot itemSlot)
        {
            ItemSlot = itemSlot;
        }

        public GameStageObject GetGameStage()
        {
            return ItemSlot?.itemObject?.GetGameStageObject();
        }

        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            return gameStageCollection.HasStage(ItemSlot?.itemObject?.GetGameStageObject());
        }
    }
    
    public class BurnableInfo : ICatalogueElement
    {
        public List<BurnableDisplay> BurnableItems;
        public string GetName()
        {
            return "Burnable Info";
        }

        public ItemObject GetDisplayItem()
        {
            return ItemRegistry.BurnableItemRegistry.BurnableRegistryImage;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Burnable {pageIndex+1}/{BurnableItems.Count}";
        }

        public int GetPageCount()
        {
            return BurnableItems.Count;
        }

        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection)
        {
            BurnableItemRegistry burnableItemRegistry = ItemRegistry.BurnableItemRegistry;
            List<BurnableDisplay> toDisplay = new List<BurnableDisplay>();
            toDisplay.AddRange(burnableItemRegistry.GetAllItemsToDisplay());
            toDisplay.AddRange(burnableItemRegistry.GetAllMaterialsToDisplay());
            BurnableInfo burnableInfo = new BurnableInfo(toDisplay);
            CatalogueElementData catalogueElementData = new CatalogueElementData(burnableInfo, CatalogueInfoDisplayType.Burnable);
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData},gameStageCollection);
        }

        public bool Filter(PlayerGameStageCollection gameStageCollection)
        {
            CatalogueInfoUtils.FilterList(BurnableItems,gameStageCollection);
            return BurnableItems.Count > 0;
        }

        public BurnableInfo(List<BurnableDisplay> burnableItems)
        {
            BurnableItems = burnableItems;
        }
    }
}
