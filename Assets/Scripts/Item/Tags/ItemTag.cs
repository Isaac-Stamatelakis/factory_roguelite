using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using Item.Tags.ItemTagManagers.Instances;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using Items.Tags.FluidContainers;
using RecipeModule;
using TileEntity.Instances.Matrix;
using Items.Inventory;
using RobotModule;

namespace Items.Tags {
    public enum ItemTag {
        FluidContainer = 0,
        EnergyContainer = 1,
        CompactMachine = 2,
        Inventory = 3,
        RobotBlueprint = 4,
        ItemFilter = 5,
        EncodedRecipe = 6,
        StorageDrive = 7,
        RobotData = 8,
        CaveData = 9
    }   
    public static class ItemTagExtension
    {
        private static readonly Dictionary<ItemTag, ItemTagManager> itemTagManagerDict = new()
        {
            { ItemTag.CaveData, new CaveDataTagManager() },
            { ItemTag.FluidContainer, new FluidContainerTagManager()},
            { ItemTag.EnergyContainer, new EnergyItemTagManager()},
            { ItemTag.CompactMachine, new CompactMachineTagManager()},
            { ItemTag.ItemFilter, new ItemFilterTagManager()},
            { ItemTag.EncodedRecipe, new EncodedRecipeTagManager()},
            { ItemTag.StorageDrive, new StorageDriveTagManager()},
            { ItemTag.RobotData, new RobotTagManager()}
        };
        public static string Serialize(this ItemTag tag, ItemTagCollection tagCollection) {
            if (!tagCollection.Dict.TryGetValue(tag, out object obj)) return null;
            
            if (obj == null) return null;
            
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif
            
            return manager.Serialize(obj);
        }
        
        public static GameObject GetUITagElement(this ItemTag tag, ItemSlot itemSlot, object tagData) {
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif
            if (manager is not IItemTagUIViewable tagUIViewable) return null;
            return tagUIViewable.GetUITagObject(tagData, itemSlot?.itemObject);
        }
        
        public static GameObject GetWorldTagElement(this ItemTag tag, ItemSlot itemSlot, object tagData) {
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif
            if (manager is not IItemTagWorldViewable tagWorldViewable) return null;
            return tagWorldViewable.GetWorldTagObject(tagData, itemSlot?.itemObject);
        }

        public static ItemTagVisualLayer GetVisualLayer(this ItemTag tag) {
            return tag switch {
                ItemTag.EncodedRecipe => ItemTagVisualLayer.Front,
                _ => ItemTagVisualLayer.Back
            };
        }
        
        public static object Deserialize(this ItemTag tag, string data) {
            if (data == null) return null;
            
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif
            
            return manager.Deserialize(data);
        }
        
        public static object CopyTagData(this ItemTag tag, object tagData)
        {
            if (tagData == null) return null;
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif

            return manager is not IItemTagReferencedType itemTagReferencedType ? tagData : itemTagReferencedType.CreateDeepCopy(tagData);
        }
        

        public static bool IsEquivalent(this ItemTag tag, object first, object second) {
            if (first == null && second == null) {
                return true;
            }
            
#if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return false;
            }
#endif
            if (first == null || second == null)
            {
                if (manager is not IItemTagNullStackable nullStackable) return false;
                object nonNullObject = first ?? second; // Nice and clean c# syntax
                return nullStackable.IsStackableWithNullObject(nonNullObject);
            }
            
            if (manager is not IItemTagStackable itemTagStackable) return false;
            return itemTagStackable.AreStackable(first, second);
        }

        public static string GetTagText(this ItemTag tag, object tagData)
        {
            #if UNITY_EDITOR
            if (!itemTagManagerDict.TryGetValue(tag, out ItemTagManager manager))
            {
                Debug.LogWarning($"ItemTagManager not implemented for {tag}");
                return null;
            }
            #endif
            if (manager is IToolTipTagViewable tagViewable)
            {
                return tagViewable.GetToolTip(tagData);
            }
            return string.Empty;
        }

    }
}