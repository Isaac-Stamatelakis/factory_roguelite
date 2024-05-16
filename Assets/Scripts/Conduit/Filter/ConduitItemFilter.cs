using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conduits.Ports {
    [System.Serializable]
    public class ItemFilter : IFilter
    {
        public List<ItemSlot> items;
        public bool whitelist;
        public bool matchNBT;
        public bool filter(ItemSlot itemSlot) {
            foreach (ItemSlot inFilter in items) {
                if (inFilter == null || inFilter.itemObject == null) {
                    continue;
                }
                if (itemSlot.itemObject.id != inFilter.itemObject.id) {
                    continue;
                } 
                if (!matchNBT && whitelist) {
                    return true;
                }
                if (itemSlot.tags.Equals(inFilter.tags)) {
                    if (whitelist) {
                        return true;
                    }
                }
            }
            return !whitelist;
        }
    }

    
}

