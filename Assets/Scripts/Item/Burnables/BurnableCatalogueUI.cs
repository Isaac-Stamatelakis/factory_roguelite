using System;
using System.Collections.Generic;
using Item.GameStage;
using Item.Inventory;
using Item.Slot;
using Item.Transmutation;
using Items;
using Items.Inventory;
using Items.Transmutable;
using Player;
using Recipe;
using Recipe.Collection;
using Recipe.Processor;
using TMPro;
using UI.Catalogue.InfoViewer;
using Unity.VisualScripting;
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
        private InventoryUIRotator rotator;
        public override void Display(ICatalogueElement element, PlayerGameStageCollection gameStages)
        {
            rotator = mItemSlotInventoryUI.AddComponent<InventoryUIRotator>();
            burnableInfo = (BurnableInfo)element;
            DisplayPage(0);
        }

        public override void DisplayPage(int pageIndex)
        {
            if (rotator)
            {
                rotator.enabled = false;
            }
            BurnableDisplay burnableDisplay = burnableInfo.BurnableItems[pageIndex];
            burnableDisplay.Display(mItemSlotInventoryUI,mNameText,mBurnTimeText);
        }
    }

    public abstract class BurnableDisplay : IGameStageItemDisplay
    {
        public abstract bool FilterStage(PlayerGameStageCollection gameStageCollection);
        public abstract void Display(InventoryUI inventoryUI, TextMeshProUGUI itemText, TextMeshProUGUI extraText);
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

        public override void Display(InventoryUI inventoryUI, TextMeshProUGUI itemText, TextMeshProUGUI extraText)
        {
            inventoryUI.DisplayInventory(new List<ItemSlot>{ItemSlot},false);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
            ItemObject itemObject = ItemSlot.itemObject;
            itemText.text = $"Name: {itemObject.name}";
            uint burnTicks = ItemRegistry.BurnableItemRegistry.GetBurnDuration(itemObject);
            float burnSeconds = (float)burnTicks / Global.TicksPerSecond;
            extraText.text = $"Burn Duration: {burnSeconds} s";
        }
    }

    public class BurnableMaterialDisplay : BurnableDisplay
    {
        public TransmutableItemMaterial Material;
        public List<List<ItemSlot>> ItemSlotList;

        public BurnableMaterialDisplay(TransmutableItemMaterial material)
        {
            Material = material;
            ItemSlotList = new List<List<ItemSlot>>();
            BurnableItemRegistry burnableItemRegistry = ItemRegistry.BurnableItemRegistry;
            List<TransmutableItemState> states = material.MaterialOptions.GetAllStates();
            foreach (TransmutableItemState state in states)
            {
                
                ITransmutableItem transmutableItem = TransmutableItemUtils.GetMaterialItem(material, state);
                if (burnableItemRegistry.GetBurnDuration((ItemObject)transmutableItem) == 0) continue;
                
                if (transmutableItem == null) continue;
                ItemSlot itemSlot = new ItemSlot((ItemObject)transmutableItem, 1, null);
                List<ItemSlot> nestedItemSlotList = new List<ItemSlot> { itemSlot };
                ItemSlotList.Add(nestedItemSlotList);
            }
        }

        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            return gameStageCollection.HasStage(Material?.gameStageObject);
        }

        public override void Display(InventoryUI inventoryUI, TextMeshProUGUI itemText, TextMeshProUGUI extraText)
        {
            InventoryUIRotator rotator = inventoryUI.GetComponent<InventoryUIRotator>();
            rotator.enabled = true;
            int initialIndex = UnityEngine.Random.Range(0, ItemSlotList.Count);
            rotator.Initialize(
                ItemSlotList,1,50,
                initialIndex:initialIndex,
                onRotateCallback:DisplayIndex
            );
            DisplayIndex(initialIndex);
            return;

            void DisplayIndex(int index)
            {
                ItemObject itemObject = ItemSlotList[index][0].itemObject;
                itemText.text = $"Name: {itemObject.name}";
                uint burnTicks = ItemRegistry.BurnableItemRegistry.GetBurnDuration(itemObject);
                float burnSeconds = (float)burnTicks / Global.TicksPerSecond;
                extraText.text = $"Burn Duration: {burnSeconds} s";
            }
        }
    }

    public class BurnerProcessorDisplay : BurnableDisplay
    {
        public RecipeProcessorInstance Processor;

        public BurnerProcessorDisplay(RecipeProcessorInstance processor)
        {
            Processor = processor;
        }

        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            return true; // Might have to change this later
        }

        public override void Display(InventoryUI inventoryUI, TextMeshProUGUI itemText, TextMeshProUGUI extraText)
        {
            ItemSlot itemSlot = new ItemSlot(Processor.RecipeProcessorObject.DisplayImage, 1, null);
            inventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},false);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
            itemText.text = $"Name: {itemSlot.itemObject.name}";
            extraText.text = $"Burner Machine";
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
            List<BurnableDisplay> toDisplay = new List<BurnableDisplay>();
            List<RecipeProcessorInstance> burnerProcessors = RecipeRegistry.GetInstance().GetAllProcessorsOfType(RecipeType.Burner);
            foreach (RecipeProcessorInstance processor in burnerProcessors)
            {
                toDisplay.Add(new BurnerProcessorDisplay(processor));
            }
            
            BurnableItemRegistry burnableItemRegistry = ItemRegistry.BurnableItemRegistry;
            
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
