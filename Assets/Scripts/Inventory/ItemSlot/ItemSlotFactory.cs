using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public enum ItemSlotOption {

}


public class ItemSlotFactory 
{
    public static List<ItemSlot> deserialize(string json) {
        ItemRegistry itemRegister = ItemRegistry.getInstance();
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        if (json == null) {
            return null;
        }
        List<SerializedItemSlot> serializedItems = JsonConvert.DeserializeObject<List<SerializedItemSlot>>(json);
        foreach (SerializedItemSlot serializedItemSlot in serializedItems) {
            if (serializedItemSlot.id == null) {
                itemSlots.Add(new ItemSlot(
                    itemObject: null,
                    amount: 0,
                    nbt: null
                ));
            } else {
                itemSlots.Add(new ItemSlot(
                    itemObject: itemRegister.getItemObject(serializedItemSlot.id),
                    amount: serializedItemSlot.amount,
                    nbt: serializedItemSlot.nbt
                ));
            }
               
        }
        return itemSlots;
    }

    public static string createEmptyInventory(int size) {
        List<SerializedItemSlot> itemSlots = new List<SerializedItemSlot>();
        for (int n = 0; n < size; n ++) {
            itemSlots.Add(initEmptyItemSlot());
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(itemSlots);
    }
    private static SerializedItemSlot initEmptyItemSlot() {
        return new SerializedItemSlot(
            null,
            0,
            null
        );
    }
    public static string serializeList(List<ItemSlot> items) {
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

    private class SerializedItemSlot {
        public SerializedItemSlot(string id, int amount, Dictionary<string,object> nbt) {
            this.id = id;
            this.amount = amount;
            this.nbt = nbt;
        }
        public string id;
        public int amount;
        public Dictionary<string,object> nbt;
    }

}
