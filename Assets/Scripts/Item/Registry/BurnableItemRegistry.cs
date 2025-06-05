using System.Collections.Generic;
using System.Linq;
using Item.Burnables;
using Item.Slot;
using Item.Transmutation;
using Items;
using Items.Transmutable;
using Recipe.Collection;
using UnityEngine;

namespace Item.Registry
{
     public class BurnableItemRegistry
    {
        public const int RANDOM_ITEM_AMOUNT = 15;
        public const int RANDOM_MATERIAL_AMOUNT = 3;
        public ItemObject BurnableRegistryImage;
        private Dictionary<string, uint> itemBurnDurations;
        private Dictionary<TransmutableItemMaterial, uint> materialBurnDurations;
        
        public int MaterialCount => materialBurnDurations.Count;
        public int ItemCount => itemBurnDurations.Count;

        public BurnableItemRegistry(BurnableItemCollection collection)
        {
            itemBurnDurations = new Dictionary<string, uint>();
            foreach (var tuple in collection.ItemBurnDurations)
            {
                itemBurnDurations[tuple.Item1.id] = tuple.Item2;
            }
            materialBurnDurations = new Dictionary<TransmutableItemMaterial, uint>();
            foreach (var tuple in collection.MaterialBurnDurations)
            {
                materialBurnDurations[tuple.Item1] = tuple.Item2;
            }

            BurnableRegistryImage = collection.BurnableRegistryImage;
        }

        public bool IsBurnable(ItemObject itemObject)
        {
            if (itemBurnDurations.ContainsKey(itemObject.id)) return true;
            if (itemObject is not ITransmutableItem transmutableItemObject) return false;
            var material = transmutableItemObject.getMaterial();
            return materialBurnDurations.ContainsKey(material);
        }

        public uint GetBurnDuration(ItemObject itemObject)
        {
            if (ReferenceEquals(itemObject, null)) return 0;
            if (itemBurnDurations.TryGetValue(itemObject.id, out var duration)) return duration;
            if (itemObject is not ITransmutableItem transmutableItem) return 0;
            var material = transmutableItem.getMaterial();
            if (!materialBurnDurations.TryGetValue(material, out var matDuration)) return 0;
            var state = transmutableItem.getState();
            
            if (state.GetMatterState() != ItemState.Solid) return 0;
            float stateRatio = state.GetRatio();
            return (uint) (matDuration / stateRatio);
        }

        public List<BurnableItemDisplay> GetAllItemsToDisplay()
        {
            List<BurnableItemDisplay> displayList = new List<BurnableItemDisplay>();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (string id in itemBurnDurations.Keys)
            {
                var item = itemRegistry.GetItemObject(id);
                if (ReferenceEquals(item, null)) continue;
                displayList.Add(new BurnableItemDisplay(new ItemSlot(item,1,null)));
            }

            return displayList;
        }

        public List<BurnableMaterialDisplay> GetAllMaterialsToDisplay()
        {
            List<BurnableMaterialDisplay> displayList = new List<BurnableMaterialDisplay>();
            foreach (var material in materialBurnDurations.Keys)
            {
                displayList.Add(new BurnableMaterialDisplay(material));
            }

            return displayList;
        }

        public List<ItemSlot> GetRandomBurnableItems()
        {
            List<ItemSlot> displayList = new List<ItemSlot>();
            int itemAmount = Random.Range(0, RANDOM_ITEM_AMOUNT);
            
            var ids = itemBurnDurations.Keys.ToList();
            System.Random random = new System.Random();
            var shuffledKeys = ids.OrderBy(_ => random.Next()).Take(itemAmount);
            
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (string id in shuffledKeys)
            {
                var item = itemRegistry.GetItemObject(id);
                if (ReferenceEquals(item, null)) continue;
                displayList.Add(new ItemSlot(item, 1, null));
            }
            
            var materials = materialBurnDurations.Keys.ToList();
            var shuffledMaterials = materials.OrderBy(_ => random.Next()).Take(RANDOM_MATERIAL_AMOUNT);
            foreach (var material in shuffledMaterials)
            {
                List<TransmutableItemState> states = material.MaterialOptions.GetAllStates();
                foreach (TransmutableItemState state in states)
                {
                    ITransmutableItem transmutableItem = TransmutableItemUtils.GetMaterialItem(material, state);
                    uint burnTime = GetBurnDuration((ItemObject)transmutableItem);
                    if (burnTime == 0) continue;
                    displayList.Add(new ItemSlot((ItemObject)transmutableItem, 1,null));
                }
            }
            var shuffledList = displayList.OrderBy(x => random.Next()).ToList();
            return shuffledList;
            
        }
        
        
    
    }
}
