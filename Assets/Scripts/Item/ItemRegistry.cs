using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using RobotModule;
using Items;
using TileEntity;
using Conduits.Ports;
using RecipeModule;
using Items.Transmutable;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using PlayerModule;
using UI.JEI;
using Dimensions;
using Item.GameStage;
using Item.ItemObjects.Instances.Tile.Chisel;
using Item.Slot;
using Player;
using Recipe.Objects;
using Recipe.Processor;
using TileEntity.Instances.WorkBenchs;
using UnityEngine.Analytics;
using WorldModule;

namespace Items {
    public class ItemRegistry {
        private static Dictionary<string,ItemObject> items;
        private static List<TransmutableItemMaterial> materials;
        
        private static ItemRegistry instance;
        private ItemRegistry() {
            items = new Dictionary<string, ItemObject>();
        }
        

        public static bool IsLoaded => instance!=null;

        public static IEnumerator LoadItems() {
            if (instance != null && items.Count > 0) {
                yield break;
            }
            instance = new ItemRegistry();
            var handle = Addressables.LoadAssetsAsync<ItemObject>("item", null);
            yield return handle;
            HashSet<TransmutableItemMaterial> materialSet = new HashSet<TransmutableItemMaterial>();
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                IList<ItemObject> loadedAssets = handle.Result;
                foreach (ItemObject asset in loadedAssets)
                {
                    AddToDict(asset, materialSet);
                }
            }
            else {
                Debug.LogError("Failed to load assets from group: " + handle.OperationException);
            }
            materials = materialSet.ToList();
            Debug.Log($"Loaded {items.Count} Items and {materials.Count} Materials");
            yield return null;
        }

        private static bool AddToDict(ItemObject itemObject, HashSet<TransmutableItemMaterial> materialSet) {
            if (itemObject is TransmutableItemObject transmutableItemObject)
            {
                var material = transmutableItemObject.getMaterial();
                if (!ReferenceEquals(material, null))
                {
                    materialSet.Add(material);
                }
            }
            if (!items.TryGetValue(itemObject.id, out var contained)) {
                items[itemObject.id] = itemObject;
                return true;
            } else {
                Debug.LogWarning("Duplicate id for objects " + contained.name + " and " + itemObject.name + " with id: " + itemObject.id);
                return false;
            }
        }
        public static ItemRegistry GetInstance() {
            if (instance == null) {
                throw new NullReferenceException("Tried to access null item registry");
            }
            return instance;
        }
        

        public List<ItemObject> GetAllItems() {
            return items.Values.ToList();
        }

        public List<ItemObject> GetAllKnownItems()
        {
            if (!DevMode.Instance.EnableGameStages) return GetAllItems();
            List<ItemObject> knownItems = new List<ItemObject>();
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            
            foreach (ItemObject itemObject in items.Values)
            {
                if (!playerScript.GameStageCollection.HasStage(itemObject.gameStage)) continue;
                knownItems.Add(itemObject);
            }

            return knownItems;
        }

        public List<ItemObject> GetAllItemsWithPrefix(string idPrefix) {
            List<ItemObject> list = items.Values.ToList();
            return list.Where(item => item.id.StartsWith(idPrefix)).ToList();
        }

        public List<TransmutableItemMaterial> GetAllMaterials()
        {
            HashSet<TransmutableItemMaterial> materials = new HashSet<TransmutableItemMaterial>();
            foreach (ItemObject itemObject in items.Values)
            {
                if (itemObject is not TransmutableItemObject transmutableItemObject) continue;
                materials.Add(transmutableItemObject.getMaterial());
            }

            return materials.ToList();
        }
        ///
        /// Returns tileItem if id maps to tile item, null otherwise
        ///
        public TileItem GetTileItem(string id) {
            if (id == null) {
                return null;
            }
            if (!items.TryGetValue(id, value: out var itemObject)) {
                return null;
            }
            
            if (itemObject is TileItem item) {
                return item;
            }

            return null;
        }

        public RobotItem GetRobotItem(string id) {
            if (id == null) {
                return null;
            }
            if (!items.TryGetValue(id, value: out var itemObject)) {
                return null;
            }

            if (itemObject is RobotItem item) {
                return item;
            }
            return null;
        }

        public TransmutableItemObject GetTransmutableItemObject(string id) {
            if (id == null) {
                return null;
            }
            if (!items.TryGetValue(id, value: out var itemObject)) {
                return null;
            }

            if (itemObject is TransmutableItemObject value) {
                return value;
            }
            return null;
        }
        public ItemObject GetItemObject(string id)
        {
            return id == null ? null : items.GetValueOrDefault(id);
        }
        public FluidTileItem GetFluidTileItem(string id) {
            if (id == null || !items.TryGetValue(id, value: out var itemObject)) {
                return null;
            }

            if (itemObject is FluidTileItem) {
                return (FluidTileItem) itemObject;
            }
            return null;
        }
        ///
        /// Returns ConduitItem if id maps to ConduitItem, null otherwise
        ///
        public ConduitItem GetConduitItem(string id) {
            if (!items.TryGetValue(id, value: out var itemObject)) {
                return null;
            }

            if (itemObject is ConduitItem) {
                return (ConduitItem) itemObject;
            } else {
                return null;
            }
        }

        public List<ItemObject> Query(string search, int limit) {
            List<ItemObject> queried = new List<ItemObject>();
            int i = 0;
            foreach (ItemObject itemObject in items.Values) {
                if (i >= limit) {
                    break;
                }
                if (itemObject.name.ToLower().Contains(search.ToLower())) {
                    queried.Add(itemObject);
                    i ++;
                }
                
            }
            return queried;
        }

        public List<T> Query<T>(string search, int limit) where T : ItemObject {
            List<T> queried = new List<T>();
            int i = 0;
            foreach (ItemObject itemObject in items.Values) {
                if (i >= limit) {
                    break;
                }
                if (itemObject is not T value) {
                    continue;
                }
                if (itemObject.name.ToLower().Contains(search.ToLower())) {
                    queried.Add(value);
                    i ++;
                }
                
            }
            return queried;
        }
        public List<ItemSlot> QuerySlots(string search, int limit) {
            // TODO options to put certain tag items in here
            List<ItemSlot> queried = new List<ItemSlot>();
            int i = 0;
            bool enforceGameStages = DevMode.Instance.EnableGameStages;
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            foreach (ItemObject itemObject in items.Values) {
                if (i >= limit) {
                    break;
                }

                if (!itemObject.name.ToLower().Contains(search.ToLower())) continue;
                
                if (enforceGameStages && (ReferenceEquals(itemObject.gameStage,null) || !playerScript.GameStageCollection.HasStage(itemObject.gameStage))) continue;
                queried.Add(new ItemSlot(itemObject,1,null));
                i ++;
            }
            return queried;
        }
    }
}

