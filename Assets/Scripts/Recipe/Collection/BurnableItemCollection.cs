using System.Collections.Generic;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Recipe.Collection
{
    [CreateAssetMenu(fileName = "RecipeCollection", menuName = "Crafting/Burnables")]
    public class BurnableItemCollection : ScriptableObject
    {
        public List<EditorKVP<ItemObject, uint>> ItemBurnDurations;
        public List<EditorKVP<TransmutableItemMaterial, uint>> MaterialBurnDurations;
    }

    public class BurnableItemRegistry
    {
        private Dictionary<string, uint> itemBurnDurations;
        private Dictionary<TransmutableItemMaterial, uint> materialBurnDurations;
        
        public int MaterialCount => materialBurnDurations.Count;
        public int ItemCount => itemBurnDurations.Count;

        public BurnableItemRegistry(BurnableItemCollection collection)
        {
            itemBurnDurations = new Dictionary<string, uint>();
            foreach (var tuple in collection.ItemBurnDurations)
            {
                itemBurnDurations[tuple.Item1.id] = tuple.Item2;
            }
            materialBurnDurations = new Dictionary<TransmutableItemMaterial, uint>();
            foreach (var tuple in collection.MaterialBurnDurations)
            {
                materialBurnDurations[tuple.Item1] = tuple.Item2;
            }
        }

        public bool IsBurnable(ItemObject itemObject)
        {
            if (itemBurnDurations.ContainsKey(itemObject.id)) return true;
            if (itemObject is not TransmutableItemObject transmutableItemObject) return false;
            var material = transmutableItemObject.getMaterial();
            return materialBurnDurations.ContainsKey(material);
        }

        public uint GetBurnDuration(ItemObject itemObject)
        {
            if (ReferenceEquals(itemObject, null)) return 0;
            if (itemBurnDurations.TryGetValue(itemObject.id, out var duration)) return duration;
            if (itemObject is not TransmutableItemObject transmutableItemObject) return 0;
            var material = transmutableItemObject.getMaterial();
            if (!materialBurnDurations.TryGetValue(material, out var matDuration)) return 0;
            var state = transmutableItemObject.getState();
            float stateRatio = state.getRatio();
            return (uint) (matDuration / stateRatio);
        }
        
        
    
    }
}