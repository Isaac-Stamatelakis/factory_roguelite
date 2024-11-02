using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using RobotModule;
using Items;
using TileEntityModule;
using Conduits.Ports;
using RecipeModule;
using Items.Transmutable;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using PlayerModule;
using UI.JEI;
using Dimensions;

namespace Items {
    public class ItemRegistry {
        private static Dictionary<string,ItemObject> items;
        private static ItemRegistry instance;
        private ItemRegistry() {
            items = new Dictionary<string, ItemObject>();
        }

        public static bool IsLoaded => instance!=null;

        public static IEnumerator loadItems() {
            if (instance != null) {
                yield break;
            }
            instance = new ItemRegistry();
            var handle = Addressables.LoadAssetsAsync<ItemObject>("item", null);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                IList<ItemObject> loadedAssets = handle.Result;
                foreach (ItemObject asset in loadedAssets)
                {
                    addToDict(asset);
                }
            }
            else {
                Debug.LogError("Failed to load assets from group: " + handle.OperationException);
            }
            Addressables.Release(handle);
            Debug.Log($"Loaded {items.Count} Items");
            yield return null;
        }

        private static bool addToDict(ItemObject itemObject) {
            if (!items.ContainsKey(itemObject.id)) {
                items[itemObject.id] = itemObject;
                return true;
            } else {
                ItemObject contained = items[itemObject.id];
                Debug.LogError("Duplicate id for objects " + contained.name + " and " + itemObject.name + " with id: " + itemObject.id);
                return false;
            }
        }
        public static ItemRegistry getInstance() {
            if (instance == null) {
                Debug.LogError("Accessed unitalized item registry");
                return null;
            }
            return instance;
        }

        

        public List<ItemObject> getAllItems() {
            return items.Values.ToList();
        }

        public List<ItemObject> getAllItemsWithPrefix(string idPrefix) {
            List<ItemObject> list = items.Values.ToList();
            return list.Where(item => item.id.StartsWith(idPrefix)).ToList();
        }
        ///
        /// Returns tileItem if id maps to tile item, null otherwise
        ///
        public TileItem getTileItem(string id) {
            if (id == null) {
                return null;
            }
            if (!items.ContainsKey(id)) {
                return null;
            }
            ItemObject itemObject = items[id];
            if (itemObject is TileItem) {
                return (TileItem) itemObject;
            } else {
                return null;
            }
        }

        public RobotItem GetRobotItem(string id) {
            if (id == null) {
                return null;
            }
            if (!items.ContainsKey(id)) {
                return null;
            }
            ItemObject itemObject = items[id];
            if (itemObject is RobotItem) {
                return (RobotItem) itemObject;
            }
            return null;
        }

        public TransmutableItemObject getTransmutableItemObject(string id) {
            if (id == null) {
                return null;
            }
            if (!items.ContainsKey(id)) {
                return null;
            }
            ItemObject itemObject = items[id];
            if (itemObject is TransmutableItemObject) {
                return (TransmutableItemObject) itemObject;
            }
            return null;
        }
        public ItemObject getItemObject(string id) {
            if (id == null) {
                return null;
            }
            if (!items.ContainsKey(id)) {
                return null;
            }
            return items[id];   
        }
        public FluidTileItem getFluidTileItem(string id) {
            if (id == null || !items.ContainsKey(id)) {
                return null;
            }
            ItemObject itemObject = items[id];
            if (itemObject is FluidTileItem) {
                return (FluidTileItem) itemObject;
            }
            return null;
        }
        ///
        /// Returns ConduitItem if id maps to ConduitItem, null otherwise
        ///
        public ConduitItem GetConduitItem(string id) {
            if (!items.ContainsKey(id)) {
                return null;
            }
            ItemObject itemObject = items[id];
            if (itemObject is ConduitItem) {
                return (ConduitItem) itemObject;
            } else {
                return null;
            }
        }

        public List<ItemObject> query(string search, int limit) {
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

        public List<T> query<T>(string search, int limit) where T : ItemObject {
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
        public List<ItemSlot> querySlots(string search, int limit) {
            // TODO options to put certain tag items in here
            List<ItemSlot> queried = new List<ItemSlot>();
            int i = 0;
            foreach (ItemObject itemObject in items.Values) {
                if (i >= limit) {
                    break;
                }
                if (itemObject.name.ToLower().Contains(search.ToLower())) {
                    queried.Add(ItemSlotFactory.createNewItemSlot(itemObject,1));
                    i ++;
                }
            }
            return queried;
        }

        public List<TileItem> getTileEntitiesOfProcessor(RecipeProcessor processor) {
            List<TileItem> tileItems = new List<TileItem>();
            foreach (ItemObject itemObject in items.Values) {
                if (itemObject is not TileItem tileItem) {
                    continue;
                }
                if (tileItem.tileEntity == null) {
                    continue;
                }
                if (tileItem.tileEntity is not IProcessorTileEntity tileEntityProcess) {
                    continue;
                }
                RecipeProcessor entityProcessor = tileEntityProcess.getRecipeProcessor();
                if (!processor.Equals(entityProcessor)) {
                    continue;
                }
                tileItems.Add(tileItem);
            }
            return tileItems;
        }
    }
}

