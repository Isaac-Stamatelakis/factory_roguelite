using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Transmutable;
using Recipe;
using Recipe.Collection;
using TMPro;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Item.Burnables
{
    public class BurnableCatalogueUI : CatalogueInfoUI
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        [SerializeField] private TextMeshProUGUI mBurnTimeText;
        [SerializeField] private InventoryUI mItemSlotInventoryUI;
        private BurnableInfo burnableInfo;
        public override void Display(ICatalogueElement element)
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
            uint burnTicks = RecipeRegistry.BurnableItemRegistry.GetBurnDuration(itemObject);
            mBurnTimeText.text = $"Burn Time: {burnTicks}T";
        }
    }

    public abstract class BurnableDisplay
    {
        
    }

    public class BurnableItemDisplay : BurnableDisplay
    {
        public ItemSlot ItemSlot;

        public BurnableItemDisplay(ItemSlot itemSlot)
        {
            ItemSlot = itemSlot;
        }
    }
    
    public class BurnableInfo : ICatalogueElement
    {
        public List<BurnableDisplay> BurnableItems;
        public string GetName()
        {
            return "Burnable Info";
        }

        public Sprite GetSprite()
        {
            return RecipeRegistry.BurnableItemRegistry.BurnableRegistryImage;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Burnable {pageIndex+1}/{BurnableItems.Count}";
        }

        public int GetPageCount()
        {
            return BurnableItems.Count;
        }

        public void DisplayAllElements()
        {
            BurnableItemRegistry burnableItemRegistry = RecipeRegistry.BurnableItemRegistry;
            List<BurnableDisplay> toDisplay = new List<BurnableDisplay>();
            toDisplay.AddRange(burnableItemRegistry.GetAllItemsToDisplay());
            toDisplay.AddRange(burnableItemRegistry.GetAllMaterialsToDisplay());
            BurnableInfo burnableInfo = new BurnableInfo(toDisplay);
            CatalogueElementData catalogueElementData = new CatalogueElementData(burnableInfo, CatalogueInfoDisplayType.Burnable);
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData});
        }

        public BurnableInfo(List<BurnableDisplay> burnableItems)
        {
            BurnableItems = burnableItems;
        }
    }
}
