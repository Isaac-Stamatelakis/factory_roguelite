using System.Collections.Generic;
using System.IO;
using Item.Slot;
using Items;
using Items.Tags;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;
using TileEntity.Instances.Matrix;
using UnityEngine;
using UnityEngine.UI;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class EncodedRecipeTagManager : ItemTagManager, IItemTagUIViewable
    {
        public override string Serialize(object obj)
        {
            return obj is not EncodedRecipe encodedRecipe ? null : EncodedRecipeFactory.seralize(encodedRecipe);
        }

        public override object Deserialize(string data)
        {
            return EncodedRecipeFactory.deseralize(data);
        }

        public ItemTagVisualLayer GetLayer()
        {
            return ItemTagVisualLayer.Front;
        }

        public GameObject GetUITagObject(object obj, ItemObject containerObject)
        {
            if (obj is not EncodedRecipe encodedRecipe) {
                return null;
            }
            if (encodedRecipe.Outputs.Count > 0)
            {
                GameObject imageObject = new GameObject();
                Image image = imageObject.AddComponent<Image>();
                RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(64, 64);
                // TODO Update this to include tag data
                image.sprite = encodedRecipe.Outputs[0].itemObject.getSprite();
                return imageObject;
            }
            return null;
        }
    }
}
