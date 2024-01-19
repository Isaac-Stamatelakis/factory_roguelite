using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public enum ItemSlotOption {

}


public class ItemSlotFactory 
{
    public static List<ItemSlot> deserialize(Dictionary<string,object> json) {
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        foreach (string key in json.Keys) {
            var jsonVal = json[key];
            try {
                List<ItemSlot> items = JsonConvert.DeserializeObject<List<ItemSlot>>(jsonVal.ToString());
                itemSlots.AddRange(items);
            } catch (JsonException e) {
                Debug.LogError(e);
            }
        }
        return itemSlots;
    }

    private class SerializedItemSlot {
        public string id;
        public int amount;
        public Dictionary<string,object> nbt;
    }

}
