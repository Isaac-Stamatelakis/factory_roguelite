using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ItemModule.Tags{
    public static class ItemTagFactory
    {
        public static ItemTagCollection initalize(ItemObject itemObject) {
            if (itemObject is ITaggable taggable) {
                return initalizeFromTaggable(taggable);
            }
            return null; 
        }

        public static string serialize(ItemTagCollection tagData) {
            if (tagData == null) {
                return null;
            }
            Dictionary<int, string> seralized = new Dictionary<int, string>();
            foreach (ItemTag tag in tagData.Dict.Keys) {
                seralized[(int) tag] = tag.serialize(tagData);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(seralized);
        }

        public static ItemTagCollection deseralize(string data) {
            if (data == null) {
                return null;
            }
            return JsonConvert.DeserializeObject<ItemTagCollection>(data);
        }
        private static ItemTagCollection initalizeFromTaggable(ITaggable taggable) {
            Dictionary<ItemTag,object> tags = new Dictionary<ItemTag, object>();
            if (taggable is IFluidContainer) {
                tags[ItemTag.FluidContainer] = ItemSlotFactory.createEmptyItemSlot();
            }
            if (taggable is TileItem tileItem) {

            }

            if (tags.Count == 0) {
                return null;
            }
            return new ItemTagCollection(tags);
        }

        private static void initFluidContainer() {

        }
    }
}

