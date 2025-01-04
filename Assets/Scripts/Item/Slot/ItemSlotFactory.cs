using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items;
using Items.Tags;
using System;
using System.Linq;
using Item.Slot;
using Recipe.Objects;
using TileEntity;
using Random = UnityEngine.Random;

public enum ItemSlotOption {

}


public static class ItemSlotFactory 
{

    public static void ClampList(List<ItemSlot> itemSlots, int count) {
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
    public static List<ItemSlot> CopyList(List<ItemSlot> itemSlots) {
        List<ItemSlot> listCopy = new List<ItemSlot>();
        foreach (ItemSlot itemSlot in itemSlots) {
            listCopy.Add(Copy(itemSlot));
        }
        return listCopy;
    }
    public static List<ItemSlot> Deserialize(string json) {
        ItemRegistry itemRegister = ItemRegistry.GetInstance();
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

    public static ItemSlot CreateNewItemSlot(ItemObject itemObject, uint amount) {
        ItemTagCollection itemTagData = ItemTagFactory.initalize(itemObject);
        return new ItemSlot(itemObject,amount,itemTagData);
    }

    public static ItemSlot CreateNewItemSlot(string id, uint amount) {
        ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
        if (itemObject == null) {
            return null;
        }
        ItemTagCollection itemTagData = ItemTagFactory.initalize(itemObject);
        return new ItemSlot(itemObject,amount,itemTagData);
    }

    public static ItemSlot Copy(ItemSlot itemSlot) {
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

    public static ItemSlot Splice(ItemSlot itemSlot, uint amount) {
        ItemSlot clone = Copy(itemSlot);
        if (clone == null) {
            return null;
        }
        clone.amount = amount;
        return clone;
    }

    public static ItemSlot DeepSlice(ItemSlot itemSlot, uint amount) {
        uint dif = itemSlot.amount - amount;
        if (dif <= 0) {
            ItemSlot copy = ItemSlotFactory.Copy(itemSlot);
            itemSlot.amount = 0;
            itemSlot.itemObject = null;
            return copy;
        } else {
            ItemSlot spliced = ItemSlotFactory.Splice(itemSlot,amount);
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
        ItemRegistry itemRegistry = ItemRegistry.GetInstance();
        if (serializedItemSlot.id == null) {
            return new ItemSlot(
                itemObject: null,
                amount: 0,
                tags: null
            );
        } else {
            return new ItemSlot(
                itemObject: itemRegistry.GetItemObject(serializedItemSlot.id),
                amount: serializedItemSlot.amount,
                tags: ItemTagFactory.deseralize(serializedItemSlot.tags)
            );
        }
    }

    public static List<ItemSlot> FromEditorObjects<T>(List<T> itemSlotObjects) where T : EditorItemSlot
    {
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        foreach (var itemSlotObject in itemSlotObjects)
        {
            itemSlots.Add(FromEditorObject(itemSlotObject));
        }
        return itemSlots;
    }

    public static List<ItemSlot> FromRandomEditorObjects(List<RandomEditorItemSlot> randomEditorItemSlots)
    {
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        foreach (var randomEditorSlot in randomEditorItemSlots)
        {
            float ran = Random.value; // random value in interval [0,1]
            if (ran >= randomEditorSlot.Chance) continue;
            itemSlots.Add(FromEditorObject(randomEditorSlot));
        }

        return itemSlots;
    }
    public static ItemSlot FromEditorObject(EditorItemSlot editorItemSlot)
    {
        if (editorItemSlot.Tags.Count == 0)
        {
            return new ItemSlot(editorItemSlot.ItemObject,editorItemSlot.Amount,null);
        }
        Dictionary<ItemTag, object> tagData = new Dictionary<ItemTag, object>();
        return null;
    }

    

}

[System.Serializable]
public class SerializedItemSlot {
    public SerializedItemSlot(string id, uint amount, string tags) {
        this.id = id;
        this.amount = amount;
        this.tags = tags;
    }
    public string id;
    public uint amount;
    public string tags;
}