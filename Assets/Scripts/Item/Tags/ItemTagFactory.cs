using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ItemModule.Tags.FluidContainers;
using System.Linq;

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
            if (tagData == null || tagData.Dict == null) {
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
            Dictionary<int, string> seralized = JsonConvert.DeserializeObject<Dictionary<int, string>>(data);
            Dictionary<ItemTag, object> dict = new Dictionary<ItemTag, object>();
            foreach (KeyValuePair<int,string> kvp in seralized) {
                ItemTag tag = (ItemTag) kvp.Key;
                dict[tag] = tag.deseralize(seralized[kvp.Key]);
            }
            return new ItemTagCollection(dict);
     
        }
        private static ItemTagCollection initalizeFromTaggable(ITaggable taggable) {
            Dictionary<ItemTag,object> tags = new Dictionary<ItemTag, object>();
            if (taggable is IFluidContainer) {
                tags[ItemTag.FluidContainer] = null;
            }
            if (taggable is TileItem tileItem) {

            }
            if (tags.Count == 0) {
                return null;
            }
            return new ItemTagCollection(tags);
        }

        public static bool tagsEqual(ItemTagCollection first, ItemTagCollection second) {
            if (first == null && second == null) {
                return true;
            }
            if (first == null || first.Dict == null) {
                return false;
            }
            if (second== null || second.Dict == null) {
                return false;
            }
            if (first.Dict.Count != second.Dict.Count) {
                return false;
            }
            foreach (ItemTag tag in first.Dict.Keys) {
                if (!second.Dict.ContainsKey(tag)) {
                    return false;
                }
                if (!tag.isEquivalent(first.Dict[tag],second.Dict[tag])) {
                    return false;
                }
            }
            return true;            
        } 
    }
}

