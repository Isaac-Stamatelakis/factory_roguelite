using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public enum ItemSlotOption {

}


public class ItemSlotFactory 
{
    public static List<ItemSlot> deserialize(object json) {
        ItemRegistry itemRegister = ItemRegistry.getInstance();
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        List<SerializedItemSlot> serializedItems = JsonConvert.DeserializeObject<List<SerializedItemSlot>>(json.ToString());
        foreach (SerializedItemSlot serializedItemSlot in serializedItems) {
            itemSlots.Add(new ItemSlot(
                itemObject: itemRegister.getItemObject(serializedItemSlot.id),
                amount: serializedItemSlot.amount,
                nbt: new Dictionary<ItemSlotOption, object>()
            ));   
        }
        return itemSlots;
    }

    private class SerializedItemSlot {
        public string id;
        public int amount;
        public Dictionary<string,object> nbt;
    }

}
