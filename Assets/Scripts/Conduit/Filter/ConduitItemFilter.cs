using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;

namespace Conduits.Ports {
    public class ItemFilter : IFilter
    {
        public List<string> ids;
        public bool whitelist = true;
        public bool Filter(ItemSlot itemSlot)
        {
            if (ids == null) return true;
            foreach (string id in ids)
            {
                if (itemSlot.itemObject.id == id) return whitelist;
            }

            return !whitelist;
        }
        
        
    }

    
}

