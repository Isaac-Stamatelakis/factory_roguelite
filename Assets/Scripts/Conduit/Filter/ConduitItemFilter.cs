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
            return Filter(itemSlot?.itemObject?.id);
        }
        
        public bool Filter(string itemId)
        {
            if (ids == null) return true;
            foreach (string id in ids)
            {
                if (itemId == id) return whitelist;
            }

            return !whitelist;
        }

        public ItemFilter()
        {
            
        }

        public ItemFilter(List<string> ids, bool whitelist)
        {
            this.ids = ids;
            this.whitelist = whitelist;
        }
    }

    
}

