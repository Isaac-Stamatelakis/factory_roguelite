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
        public List<EditorKvp<ItemObject, float>> ItemBurnDurations;
        public List<EditorKvp<TransmutableItemMaterial, float>> MaterialBurnDurations;
    }
}