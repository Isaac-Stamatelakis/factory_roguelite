using System.Collections.Generic;
using System.Linq;
using Item.Burnables;
using Item.Slot;
using Item.Transmutation;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Recipe.Collection
{
    [CreateAssetMenu(fileName = "Burnable Item Collection", menuName = "Crafting/Burnables")]
    public class BurnableItemCollection : ScriptableObject
    {
        public ItemObject BurnableRegistryImage;
        public List<EditorKVP<ItemObject, uint>> ItemBurnDurations;
        public List<EditorKVP<TransmutableItemMaterial, uint>> MaterialBurnDurations;
    }
}