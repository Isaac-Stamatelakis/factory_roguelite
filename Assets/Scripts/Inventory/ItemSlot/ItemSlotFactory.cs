using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ItemModule;

public enum ItemSlotOption {

}


public static class ItemSlotFactory 
{
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

    public static ItemSlot copy(ItemSlot itemSlot) {
        return new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemSlot.nbt);
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
            nbt: null
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
            nbt: itemSlot.nbt
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
                nbt: null
            );
        } else {
            return new ItemSlot(
                itemObject: itemRegistry.getItemObject(serializedItemSlot.id),
                amount: serializedItemSlot.amount,
                nbt: serializedItemSlot.nbt
            );
        }
    }

    

}

[System.Serializable]
public class SerializedItemSlot {
    public SerializedItemSlot(string id, int amount, Dictionary<string,object> nbt) {
        this.id = id;
        this.amount = amount;
        this.nbt = nbt;
    }
    public string id;
    public int amount;
    public Dictionary<string,object> nbt;
}