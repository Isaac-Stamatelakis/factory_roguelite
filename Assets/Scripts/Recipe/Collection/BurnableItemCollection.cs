using System.Collections.Generic;
using System.Linq;
using Item.Burnables;
using Item.Slot;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Recipe.Collection
{
    [CreateAssetMenu(fileName = "RecipeCollection", menuName = "Crafting/Burnables")]
    public class BurnableItemCollection : ScriptableObject
    {
        public Sprite BurnableRegistryImage;
        public List<EditorKVP<ItemObject, uint>> ItemBurnDurations;
        public List<EditorKVP<TransmutableItemMaterial, uint>> MaterialBurnDurations;
    }

    public class BurnableItemRegistry
    {
        public static readonly int RANDOM_SAMPLE_AMOUNT = 10;
        public Sprite BurnableRegistryImage;
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

            BurnableRegistryImage = collection.BurnableRegistryImage;
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

        public List<BurnableItemDisplay> GetAllItemsToDisplay()
        {
            List<BurnableItemDisplay> displayList = new List<BurnableItemDisplay>();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (string id in itemBurnDurations.Keys)
            {
                var item = itemRegistry.GetItemObject(id);
                if (ReferenceEquals(item, null)) continue;
                displayList.Add(new BurnableItemDisplay(new ItemSlot(item,1,null)));
            }

            return displayList;
        }

        public List<BurnableMaterialDisplay> GetAllMaterialsToDisplay()
        {
            List<BurnableMaterialDisplay> displayList = new List<BurnableMaterialDisplay>();
            foreach (var material in materialBurnDurations.Keys)
            {
                displayList.Add(new BurnableMaterialDisplay(material));
            }

            return displayList;
        }

        public List<BurnableDisplay> GetRandomDisplay()
        {
            List<BurnableDisplay> displayList = new List<BurnableDisplay>();
            int itemAmount = Random.Range(0, RANDOM_SAMPLE_AMOUNT+1);
            int materialAmount = RANDOM_SAMPLE_AMOUNT - itemAmount;
            
            var ids = itemBurnDurations.Keys.ToList();
            System.Random random = new System.Random();
            var shuffledKeys = ids.OrderBy(_ => random.Next()).Take(itemAmount);
            
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (string id in shuffledKeys)
            {
                var item = itemRegistry.GetItemObject(id);
                if (ReferenceEquals(item, null)) continue;
                displayList.Add(new BurnableItemDisplay(new ItemSlot(item, 1, null)));
            }
            
            var materials = materialBurnDurations.Keys.ToList();
            var shuffledMaterials = materials.OrderBy(_ => random.Next()).Take(materialAmount);
            foreach (var material in shuffledMaterials)
            {
                displayList.Add(new BurnableMaterialDisplay(material));
            }
            
            return displayList;
            
        }
        
        
    
    }
}