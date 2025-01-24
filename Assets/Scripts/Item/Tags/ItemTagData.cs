using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags {
    public class ItemTagCollection
    {
        private Dictionary<ItemTag, object> tagData;

        public Dictionary<ItemTag, object> Dict { get => tagData; set => tagData = value; }
        public ItemTagCollection(Dictionary<ItemTag, object> tagData) {
            this.tagData = tagData;
        }
        public override bool Equals(object obj)
        {
            if (obj == null && tagData != null) {
                return false;
            } 
            if (obj is not ItemTagCollection itemTagCollection) {
                return false;
            }
            if (itemTagCollection.Dict.Count != tagData.Count) {
                return false;
            }
            foreach (KeyValuePair<ItemTag,object> kvp in tagData) {
                if (!itemTagCollection.Dict.ContainsKey(kvp.Key)) {
                    return false;
                }
                if (
                    (kvp.Value != null && itemTagCollection.Dict[kvp.Key] == null) || 
                    ((kvp.Value == null && itemTagCollection.Dict[kvp.Key] != null))
                ) {
                    return false;
                }
                if (kvp.Value == null && itemTagCollection.Dict[kvp.Key] == null) {
                    continue;
                }
                if (!kvp.Value.Equals(itemTagCollection.Dict[kvp.Key])) {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public bool HasTags(HashSet<ItemTag> tags)
        {
            foreach (ItemTag tag in tags)
            {
                if (tagData.ContainsKey(tag)) return true;
            }

            return false;
        }
    }
}

