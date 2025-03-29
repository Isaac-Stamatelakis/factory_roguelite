using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Tags;
using System.Linq;

namespace Items {
    public class ItemSlotKey
    {
        public Dictionary<string, Dictionary<int, GameObject>> idTagItemSlotDict;
    }

    public class ItemTagKey {
        public readonly ItemTagCollection itemTagCollection;
        public ItemTagKey(ItemTagCollection itemTagCollection) {
            this.itemTagCollection = itemTagCollection;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ItemTagKey itemTagKey) {
                return false;
            }
            return ItemTagFactory.TagsEqual(itemTagKey.itemTagCollection,itemTagCollection);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                if (itemTagCollection?.Dict == null) {
                    return hash;    
                }
                foreach (var kvp in itemTagCollection.Dict.OrderBy(kvp => kvp.Key.GetHashCode()))
                {
                    hash = hash * 23 + kvp.Key.GetHashCode();
                    if (kvp.Value != null) {
                        hash = hash * 23 + kvp.Value.GetHashCode();
                    }
                    
                }
                return hash;
            }
        }
    }
}

