using System.Collections.Generic;
using Items;
using Items.Tags;

namespace Item.Slot
{
    public static class ItemSlotUtils
    {
    
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
            for (int i = 0; i < contained.Count; i++) {
                ItemSlot inputSlot = contained[i];
                if (IsItemSlotNull(inputSlot)) {
                    contained[i] = new ItemSlot(toInsert.itemObject,toInsert.amount,toInsert.tags);
                    toInsert.amount=0;
                    return true;
                }
                if (!AreEqual(inputSlot,toInsert)) {
                    continue;
                }
            
                if (inputSlot.amount >= maxSize) {
                    continue;
                }
                // Success
                InsertIntoSlot(inputSlot,toInsert,maxSize);
                return true;
            }
            return false;
        }

        public static bool IsItemSlotNull(ItemSlot itemSlot)
        {
            return ReferenceEquals(itemSlot?.itemObject,null) || itemSlot.amount == 0;
        }

        public static ItemSlot ExtractFromInventory(List<ItemSlot> inventory) {
            foreach (ItemSlot itemSlot in inventory) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return itemSlot;
                }
            }
            return null;
        }

        public static List<ItemSlot> CreateNullInventory(int count) {
            List<ItemSlot> inventory = new List<ItemSlot>();
            for (int i = 0; i < count; i++) {
                inventory.Add(null);
            }
            return inventory;
        }

        public static void InsertIntoSlot(ItemSlot toCombineInto, ItemSlot toTakeFrom, uint size) {
            uint sum = toCombineInto.amount + toTakeFrom.amount;
            if (sum > size) {
                toCombineInto.amount = size;
                toTakeFrom.amount = sum-size;
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
            if (!ItemTagFactory.tagsEqual(first.tags,second.tags)) {
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
            if (inputSlot.amount >= Global.MaxSize) {
                return;
            }
            // Success
            uint sum = inputSlot.amount + toInsert.amount;
            if (sum > Global.MaxSize) {
                toInsert.amount = sum - Global.MaxSize;
                inputSlot.amount = Global.MaxSize;
            } else {
                inputSlot.amount = sum;
                toInsert.amount = 0;
            }
        }

        public static void sortInventoryByState(List<ItemSlot> inputs,out List<ItemSlot> solidRecipeInputs,out List<ItemSlot> fluidRecipeInputs) {
            solidRecipeInputs = new List<ItemSlot>();
            fluidRecipeInputs = new List<ItemSlot>();
            foreach (ItemSlot itemSlot in inputs) {
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
    }
}
