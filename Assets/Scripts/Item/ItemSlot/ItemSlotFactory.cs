using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ItemModule;
using ItemModule.Tags;
using System;
using System.Linq;

public enum ItemSlotOption {

}


public static class ItemSlotFactory 
{

    public static void clampList(List<ItemSlot> itemSlots, int count) {
        if (count < 0) {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
        }
        int currentCount = itemSlots.Count;
        if (itemSlots.Count > count) {
            itemSlots.RemoveRange(count, currentCount - count);
        } else if (itemSlots.Count < count) {
            itemSlots.AddRange(Enumerable.Repeat<ItemSlot>(null, count - currentCount));
        }
    }
    public static List<ItemSlot> copyList(List<ItemSlot> itemSlots) {
        List<ItemSlot> listCopy = new List<ItemSlot>();
        foreach (ItemSlot itemSlot in itemSlots) {
            listCopy.Add(copy(itemSlot));
        }
        return listCopy;
    }
    public static List<ItemSlot> deserialize(string json) {
        ItemRegistry itemRegister = ItemRegistry.getInstance();
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        if (json == null) {
            return null;
        }
        List<SerializedItemSlot> serializedItems = JsonConvert.DeserializeObject<List<SerializedItemSlot>>(json);
        foreach (SerializedItemSlot serializedItemSlot in serializedItems) {
            itemSlots.Add(deseralizeItemSlot(serializedItemSlot));
               
        }
        return itemSlots;
    }

    public static ItemSlot createNewItemSlot(ItemObject itemObject, int amount) {
        ItemTagCollection itemTagData = ItemTagFactory.initalize(itemObject);
        return new ItemSlot(itemObject,amount,itemTagData);
    }

    public static ItemSlot copy(ItemSlot itemSlot) {
        if (itemSlot == null) {
            return null;
        }
        if (itemSlot.tags == null || itemSlot.tags.Dict == null) {
            return new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemSlot.tags);
        } 
        Dictionary<ItemTag, object> tagCopy = new Dictionary<ItemTag, object>();
        
        foreach (KeyValuePair<ItemTag,object> kvp in itemSlot.tags.Dict) {
            tagCopy[kvp.Key] = kvp.Key.copyData(kvp.Value);
        }
        ItemTagCollection itemTagCollection = new ItemTagCollection(tagCopy);
        return new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemTagCollection);
    }

    public static ItemSlot splice(ItemSlot itemSlot, int amount) {
        ItemSlot clone = copy(itemSlot);
        if (clone == null) {
            return null;
        }
        clone.amount = amount;
        return clone;
    }

    public static ItemSlot deepSlice(ItemSlot itemSlot, int amount) {
        int dif = itemSlot.amount - amount;
        if (dif <= 0) {
            ItemSlot copy = ItemSlotFactory.copy(itemSlot);
            itemSlot.amount = 0;
            itemSlot.itemObject = null;
            return copy;
        } else {
            ItemSlot spliced = ItemSlotFactory.splice(itemSlot,amount);
            itemSlot.amount -= amount;
            return spliced;
        }
        
        
    }
    public static string createEmptySerializedInventory(int size) {
        List<SerializedItemSlot> itemSlots = new List<SerializedItemSlot>();
        for (int n = 0; n < size; n ++) {
            itemSlots.Add(initEmptyItemSlot());
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(itemSlots);
    }
    public static ItemSlot createEmptyItemSlot() {
        return new ItemSlot(
            itemObject: null,
            amount: 0,
            tags: null
        );
    }
    public static List<ItemSlot> createEmptyInventory(int size) {
        List<ItemSlot> inventory = new List<ItemSlot>();
        for (int n = 0; n < size; n++) {
            inventory.Add(createEmptyItemSlot());
        }
        return inventory;
    }
    private static SerializedItemSlot initEmptyItemSlot() {
        return new SerializedItemSlot(
            null,
            0,
            null
        );
    }
    public static string serializeList(List<ItemSlot> items) {
        if (items == null) {
            return null;
        }
        List<SerializedItemSlot> serializedItemSlots = new List<SerializedItemSlot>();
        foreach (ItemSlot itemSlot in items) {
            serializedItemSlots.Add(serialize(itemSlot));
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(serializedItemSlots);
    }

    private static SerializedItemSlot serialize(ItemSlot itemSlot) {
        if (itemSlot == null || itemSlot.itemObject == null) {
            return new SerializedItemSlot(
                null,
                0,
                null
            );
        }
        return new SerializedItemSlot(
            id: itemSlot.itemObject.id,
            amount: itemSlot.amount,
            tags: ItemTagFactory.serialize(itemSlot.tags)
        );
    }

    public static string seralizeItemSlot(ItemSlot itemSlot) {
        return JsonConvert.SerializeObject(serialize(itemSlot));
    }
    public static ItemSlot deseralizeItemSlotFromString(string data) {
        if (data == null) {
            return ItemSlotFactory.createEmptyItemSlot();
        }
        SerializedItemSlot serializedItemSlot = JsonConvert.DeserializeObject<SerializedItemSlot>(data);
        return deseralizeItemSlot(serializedItemSlot);
    }
    public static ItemSlot deseralizeItemSlot(SerializedItemSlot serializedItemSlot) {
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        if (serializedItemSlot.id == null) {
            return new ItemSlot(
                itemObject: null,
                amount: 0,
                tags: null
            );
        } else {
            return new ItemSlot(
                itemObject: itemRegistry.getItemObject(serializedItemSlot.id),
                amount: serializedItemSlot.amount,
                tags: ItemTagFactory.deseralize(serializedItemSlot.tags)
            );
        }
    }

    

}

[System.Serializable]
public class SerializedItemSlot {
    public SerializedItemSlot(string id, int amount, string tags) {
        this.id = id;
        this.amount = amount;
        this.tags = tags;
    }
    public string id;
    public int amount;
    public string tags;
}