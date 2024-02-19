using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports {
    [System.Serializable]
    public class ItemFilter : IFilter
    {
        public List<SerializedItemSlot> items;
        public bool whitelist;
        public bool matchNBT;
        public bool filter(ItemSlot itemSlot) {
            foreach (SerializedItemSlot inFilter in items) {
                if (inFilter == null) {
                    continue;
                }
                if (itemSlot.itemObject.id != inFilter.id) {
                    continue;
                } 
                if (!matchNBT && whitelist) {
                    return true;
                }
                if (itemSlot.nbt == inFilter.nbt) {
                    if (whitelist) {
                        return true;
                    }
                }
            }
            return !whitelist;
        }
    }

    
}

