using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items.Tags.FluidContainers;
using System.Linq;
using Items.Tags.Matrix;

namespace Items.Tags{
    public static class ItemTagFactory
    {
        public static ItemTagCollection Initalize(ItemObject itemObject) {
            if (itemObject is ITaggableItem taggable) {
                return initalizeFromTaggable(taggable);
            }
            return null; 
        }

        public static string Serialize(ItemTagCollection tagData) {
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
        private static ItemTagCollection initalizeFromTaggable(ITaggableItem taggable) {
            List<ItemTag> tags = taggable.getTags();
            if (tags.Count == 0) {
                return null;
            }
            Dictionary<ItemTag,object> tagsDict = new Dictionary<ItemTag, object>();
            foreach (ItemTag tag in taggable.getTags()) {
                tagsDict[tag] = null;
            }
            /*
            if (taggable is IFluidContainer) {
                tagsDict[ItemTag.FluidContainer] = null;
            }
            if (taggable is TileItem tileItem) {

            }
            if (taggable is EncodedRecipeItem encodedRecipeItem) {
                tagsDict[ItemTag.EncodedRecipe] = null;
            }
            if (taggable is MatrixDriveItem matrixDriveItem) {
                List<ItemSlot> inventory = new List<ItemSlot>();
                for (int i = 0; i < matrixDriveItem.MaxItems; i++) {
                    inventory.Add(null);
                }
                tagsDict[ItemTag.StorageDrive] = inventory;
            }
            if (tagsDict.Count == 0) {
                return null;
            }
            */
            return new ItemTagCollection(tagsDict);
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

