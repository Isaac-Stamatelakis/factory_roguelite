using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
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
        [SerializeField] private ItemSlotUI mItemSlotUI;
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
                case BurnableMaterialDisplay burnableMaterialDisplay:
                    TransmutableItemMaterial material = burnableMaterialDisplay.Material;
                    ItemObject defaultItem = TransmutableItemUtils.GetMaterialItem(material, material.MaterialOptions.BaseState);
                    ItemSlot defaultItemSlot = new ItemSlot(defaultItem, 1,null);
                    DisplayItemSlot(defaultItemSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DisplayItemSlot(ItemSlot itemSlot)
        {
            mItemSlotUI.Display(itemSlot);
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

    public class BurnableMaterialDisplay : BurnableDisplay
    {
        public TransmutableItemMaterial Material;

        public BurnableMaterialDisplay(TransmutableItemMaterial material)
        {
            Material = material;
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
            return null;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Burnable {pageIndex+1}/{BurnableItems.Count}";
        }

        public int GetPageCount()
        {
            return BurnableItems.Count;
        }

        public BurnableInfo(List<BurnableDisplay> burnableItems)
        {
            BurnableItems = burnableItems;
        }
    }
}
