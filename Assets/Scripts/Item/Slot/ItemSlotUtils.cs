using System.Collections.Generic;
using Items;
using Items.Tags;
using Tiles;
using UnityEngine;

namespace Item.Slot
{
    public static class ItemSlotUtils
    {
        
        /// <summary>
        /// Builds tag dict in itemslot if null
        /// </summary>
        /// <param name="itemSlot"></param>
        /// <returns>True if dict was null, false if no build required</returns>
        public static bool BuildTagDictIfNull(ItemSlot itemSlot)
        {
            if (itemSlot.tags == null)
            {
                itemSlot.tags = new ItemTagCollection(new Dictionary<ItemTag, object>());
                return true;
            }

            if (itemSlot.tags.Dict == null)
            {
                itemSlot.tags.Dict = new Dictionary<ItemTag, object>();
                return true;
            }

            return false;
        }
        public static void AddTag(ItemSlot itemSlot, ItemTag tag, object value)
        {
            if (itemSlot.tags == null)
            {
                itemSlot.tags = new ItemTagCollection(new Dictionary<ItemTag, object>());
            } else if (itemSlot.tags.Dict == null)
            {
                itemSlot.tags.Dict = new Dictionary<ItemTag, object>();
            }
            itemSlot.tags.Dict[tag] = value;
            
        }
        public static bool InventoryAllNull(List<ItemSlot> itemSlots) {
            foreach (ItemSlot itemSlot in itemSlots)
            {
                if (!IsItemSlotNull(itemSlot)) return false;
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEmpty(List<ItemSlot> inventory)
        {
            if (inventory == null) return true;
            foreach (ItemSlot itemSlot in inventory)
            {
                if (IsItemSlotNull(itemSlot)) continue;
                if (itemSlot.amount > 0) return false;
            }
            return true;
        }

        public static void InsertInventoryIntoInventory(List<ItemSlot> to, List<ItemSlot> from, uint maxSize)
        {
            if (to == null || from == null) return;
            foreach (ItemSlot itemSlot in from)
            {
                InsertIntoInventory(to,itemSlot,maxSize);
            }
        }
        public static bool InsertIntoInventory(List<ItemSlot> contained, ItemSlot toInsert, uint maxSize) {
            if (contained == null) {
                return false;
            }
          
            int firstNullIndex = -1;
            // First pass look for matches
            for (int i = 0; i < contained.Count; i++) {
                ItemSlot inputSlot = contained[i];
                if (IsItemSlotNull(inputSlot))
                {
                    if (firstNullIndex < 0) firstNullIndex = i;
                    continue;
                }
                if (!AreEqual(inputSlot,toInsert) || inputSlot.amount >= maxSize) {
                    continue;
                }
                InsertIntoSlot(inputSlot,toInsert,maxSize);
                return true;
            }

            if (firstNullIndex < 0) return false;
          
            contained[firstNullIndex] = new ItemSlot(toInsert.itemObject,toInsert.amount,toInsert.tags);
            toInsert.amount=0;
            return true;
        }
        
        public static bool InsertOneIdInventory(List<ItemSlot> contained, string id, uint maxSize, uint amount) {
            if (contained == null) {
                return false;
            }
          
            int firstNullIndex = -1;
            // First pass look for matches
            for (int i = 0; i < contained.Count; i++) {
                ItemSlot inputSlot = contained[i];
                if (IsItemSlotNull(inputSlot))
                {
                    if (firstNullIndex < 0) firstNullIndex = i;
                    continue;
                }
                if (inputSlot.amount >= maxSize || inputSlot.itemObject.id != id) continue;
                inputSlot.amount++;
                return true;
            }

            if (firstNullIndex < 0) return false;
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            contained[firstNullIndex] = new ItemSlot(itemObject,1,null);
            return true;
        }

        public static void AppendToInventory(List<ItemSlot> to, ItemSlot toAppend, uint maxSize)
        {
            if (to == null || ItemSlotUtils.IsItemSlotNull(toAppend))
            {
                return;
            }
         
            for (int i = 0; i < to.Count; i++) {
                if (toAppend.amount == 0) return;
                ItemSlot inputSlot = to[i];
                if (IsItemSlotNull(inputSlot))
                {
                    continue;
                }
                if (!AreEqual(inputSlot,toAppend) || inputSlot.amount >= maxSize) {
                    continue;
                }
                InsertIntoSlot(inputSlot,toAppend,maxSize);
            }
            to.Add(toAppend);
        }

        public static bool IsItemSlotNull(ItemSlot itemSlot)
        {
            return ReferenceEquals(itemSlot?.itemObject,null) || itemSlot.amount == 0;
        }

        public static ItemSlot ExtractFromInventory(List<ItemSlot> inventory) {
            foreach (ItemSlot itemSlot in inventory) {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                return itemSlot;
            }
            return null;
        }

        public static uint AmountOf(ItemSlot itemSlot, List<ItemSlot> inventory)
        {
            if (inventory == null || IsItemSlotNull(itemSlot)) return 0;
            uint count = 0;
            foreach (ItemSlot inventorySlot in inventory)
            {
                if (!AreEqual(inventorySlot,itemSlot)) continue;
                count += inventorySlot.amount;
            }
            return count;
        }

        public static List<ItemSlot> CreateNullInventory(int count) {
            List<ItemSlot> inventory = new List<ItemSlot>();
            for (int i = 0; i < count; i++) {
                inventory.Add(null);
            }
            return inventory;
        }

        public static void InsertIntoSlot(ItemSlot toCombineInto, ItemSlot toTakeFrom, uint maxSize) {
            uint sum = toCombineInto.amount + toTakeFrom.amount;
            if (sum > maxSize) {
                toCombineInto.amount = maxSize;
                toTakeFrom.amount = sum-maxSize;
            } else {
                toCombineInto.amount = sum;
                toTakeFrom.amount = 0;
                toTakeFrom.itemObject = null;
            }
        }

        public static bool AreEqual(ItemSlot first, ItemSlot second)
        {
            if (IsItemSlotNull(first) || IsItemSlotNull(second)) return false;
            ItemTagKey firstTagKey = new ItemTagKey(first.tags);
            ItemTagKey secondTagKey = new ItemTagKey(second.tags);
            return AreEqual(first.itemObject.id,firstTagKey,second.itemObject.id,secondTagKey);
        }

        public static bool AreEqual(string firstId, ItemTagKey firstTagKey, string secondId, ItemTagKey secondTagKey) {
            return firstId.Equals(secondId) && firstTagKey.Equals(secondTagKey);
        }

        public static bool AreEqualNoNullCheck(ItemSlot first, ItemSlot second) {
            if (first.itemObject.id != second.itemObject.id) {
                return false;
            }
            if (!ItemTagFactory.TagsEqual(first.tags,second.tags)) {
                return false;
            }
            return true;
        }

        public static bool AreEqualWithAmount(ItemSlot first, ItemSlot second) {
            if (!AreEqual(first,second)) {
                return false;
            }
            return first.amount == second.amount;
        }

        public static bool CanInsertIntoInventory(List<ItemSlot> inventory, ItemSlot toInsert, uint maxAmount) {
            if (inventory == null) {
                return false;
            }
            foreach (ItemSlot itemSlot in inventory) {
                if (CanInsertIntoSlot(itemSlot,toInsert,maxAmount)) {
                    return true;
                }
            }
            return false;
        }

        public static bool CanInsertIntoInventory(List<ItemSlot> to, List<ItemSlot> from, uint maxAmount)
        {
            if (to == null || from == null) return false;
            foreach (ItemSlot toInsert in from) {
                if (!CanInsertIntoInventory(to,toInsert,maxAmount))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanInsertIntoSlot(ItemSlot itemSlot, ItemSlot toInsert, uint maxAmount)
        {
            if (IsItemSlotNull(toInsert)) return false;
            if (IsItemSlotNull(itemSlot)) return true;
            if (itemSlot.itemObject.id != toInsert.itemObject.id) return false;
            if (itemSlot.amount >= maxAmount) return false; // ?
            if (itemSlot.tags != null && toInsert.tags != null && !itemSlot.tags.Equals(toInsert.tags)) {
                return false;
            }
        
            return true;
        }

        public static List<ItemSlot> InitEmptyInventory(int count) {
            if (count <= 0) {
                return null;
            }
   
            List<ItemSlot> inventory = new List<ItemSlot>();
            for (int i = 0; i < count; i++) {
                inventory.Add(null);
            }
            return inventory; 
        }
 
        public static void InsertListIntoInventory(List<ItemSlot> inventory, List<ItemSlot> insertList, uint maxSize) {
            if (inventory == null) {
                return;
            }
            int n = 0;
            while (n < insertList.Count) {
                ItemSlot outputItem = insertList[n];
                bool inserted = InsertIntoInventory(inventory,outputItem,maxSize);
            
                if (outputItem.amount == 0 || !inserted) { 
                    n ++;
                }
            }
        }

        public static void handleInsert(ItemSlot inputSlot, ItemSlot toInsert) {
            if (inputSlot.itemObject.id != toInsert.itemObject.id) {
                return;
            }
            if (inputSlot.amount >= Global.MAX_SIZE) {
                return;
            }
            // Success
            uint sum = inputSlot.amount + toInsert.amount;
            if (sum > Global.MAX_SIZE) {
                toInsert.amount = sum - Global.MAX_SIZE;
                inputSlot.amount = Global.MAX_SIZE;
            } else {
                inputSlot.amount = sum;
                toInsert.amount = 0;
            }
        }

        public static void sortInventoryByState<T>(List<T> inputs,out List<T> solidRecipeInputs,out List<T> fluidRecipeInputs) where T : ItemSlot{
            solidRecipeInputs = new List<T>();
            fluidRecipeInputs = new List<T>();
            foreach (T itemSlot in inputs) {
                if (IsItemSlotNull(itemSlot)) continue;
                switch (itemSlot.getState()) {
                    case ItemState.Solid:
                        solidRecipeInputs.Add(itemSlot);
                        break;
                    case ItemState.Fluid:
                        fluidRecipeInputs.Add(itemSlot);
                        break;
                }
            }
        }

        public static List<ItemSlot> GetTileItemDrop(TileItem tileItem)
        {
            if (!tileItem) return new List<ItemSlot>();
            List<ItemSlot> dropItems = new List<ItemSlot>();
            var dropOptions = tileItem.tileOptions.dropOptions;
            int count = dropOptions?.Count ?? 0;
            if (count == 0) {
                dropItems.Add(new ItemSlot(tileItem,1,null));
                return dropItems;
            }

            if (count == 1) // Optimization for common case
            {
                DropOption dropOption = dropOptions[0];
                if (dropOption.lowerAmount == dropOption.upperAmount)
                {
                    dropItems.Add(new ItemSlot(dropOption.itemObject,(uint)dropOption.lowerAmount,null));
                    return dropItems;
                }
            }
            int totalWeight = 0;
            foreach (DropOption dropOption in dropOptions) {
                totalWeight += dropOption.weight;
            }
            
            int ran = UnityEngine.Random.Range(0,totalWeight);
            totalWeight = 0;
            foreach (DropOption dropOption in dropOptions) {
                totalWeight += dropOption.weight;
                if (totalWeight < ran) continue;
                if (ReferenceEquals(dropOption.itemObject, null)) continue;
                
                uint amount = (uint)UnityEngine.Random.Range(dropOption.lowerAmount,dropOption.upperAmount+1);
                amount = GlobalHelper.MaxUInt(1, amount);
                dropItems.Add(new ItemSlot(dropOption.itemObject,amount,null));
            }

            return dropItems;
        }

        public static List<ItemSlot> FromDict(Dictionary<string, uint> amountDict, uint maxSize)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            List<ItemSlot> slots = new List<ItemSlot>();
            foreach (var (id, amount) in amountDict)
            {
                uint tempAmount = amount;
                ItemObject itemObject = itemRegistry.GetItemObject(id);
                if (!itemObject) continue;
                while (true)
                {
                    if (tempAmount <= maxSize)
                    {
                        slots.Add(new ItemSlot(itemObject,tempAmount,null));
                        break;
                    }
                    tempAmount -= maxSize;
                    slots.Add(new ItemSlot(itemObject,maxSize,null));
                }
            }
            return slots;
        }
        
        public static Dictionary<string, uint> ToDict(List<ItemSlot> slots)
        {
            Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
            foreach (ItemSlot slot in slots)
            {
                string id = slot?.itemObject?.id;
                if (id == null) continue;
                dictionary.TryAdd(id, 0);
                dictionary[id] += slot.amount;
            }
            
            return dictionary;
        }
    }
}
