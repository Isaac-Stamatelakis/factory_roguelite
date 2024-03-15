using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Tags {
    public class ItemTagCollection
    {
        private Dictionary<ItemTag, object> tagData;

        public Dictionary<ItemTag, object> Dict { get => tagData; set => tagData = value; }
        public ItemTagCollection(Dictionary<ItemTag, object> tagData) {
            this.tagData = tagData;
        }
    }
}

