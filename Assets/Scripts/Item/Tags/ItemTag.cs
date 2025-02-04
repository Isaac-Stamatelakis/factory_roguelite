using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
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
        FluidContainer,
        EnergyContainer,
        CompactMachine,
        Inventory,
        RobotBlueprint,
        ItemFilter,
        EncodedRecipe,
        StorageDrive,
        RobotData,
        CaveData
    }   
    public static class ItemTagExtension {
        private static readonly Dictionary<ItemTag, Func<object, string>> serializationFunctions = new()
        {
            { ItemTag.FluidContainer, serializeFluidContainer },
            { ItemTag.EnergyContainer, serializeEnergyContainer },
            { ItemTag.CompactMachine, serializeCompactMachineTag },
            { ItemTag.StorageDrive, serializeStorageDriver },
            { ItemTag.EncodedRecipe, seralizeEncodedRecipe },
            { ItemTag.RobotData, seralizeRobot },
            { ItemTag.CaveData, SerializeCaveData},
            { ItemTag.ItemFilter, SerializeItemFilter}
        };
        
        private static readonly Dictionary<ItemTag, Func<string, object>> deserializationMap = new()
        {
            { ItemTag.FluidContainer, data => ItemSlotFactory.DeserializeSlot(data) },
            { ItemTag.EnergyContainer, data => JsonConvert.DeserializeObject<int>(data) },
            { ItemTag.CompactMachine, data => data }, // If no deserialization is needed, return data as-is
            { ItemTag.StorageDrive, data => ItemSlotFactory.Deserialize(data) },
            { ItemTag.EncodedRecipe, data => EncodedRecipeFactory.deseralize(data) },
            { ItemTag.RobotData, data => RobotDataFactory.Deserialize(data) },
            { ItemTag.CaveData, data => data},
            { ItemTag.ItemFilter, DeserializeItemFilter}
        };
        public static string serialize(this ItemTag tag, ItemTagCollection tagCollection) {
            if (!tagCollection.Dict.ContainsKey(tag)) {
                Debug.LogError("Attempted to serialize " + tag + " which was not in TagCollection");
                return null;
            }

            if (!serializationFunctions.ContainsKey(tag))
            {
                Debug.LogWarning($"Attempted to serialize tag '{tag.ToString()}' with no serialization function");
                return null;
            }
            
            object tagData = tagCollection.Dict[tag];
            if (tagData == null) return null;
            
            return serializationFunctions[tag].Invoke(tagData);
        }

        private static string serializeDefaultSwitchCase(ItemTag tag) {
            Debug.LogError("ItemTagExtension method 'seralize' did not cover case for " + tag);
            return null;
        }

        private static string SerializeItemFilter(object filter)
        {
            ItemFilter itemFilter = filter as ItemFilter;
            return JsonConvert.SerializeObject(itemFilter);
        }

        private static object DeserializeItemFilter(string value)
        {
            if (value == null) return null;
            return JsonConvert.DeserializeObject<ItemFilter>(value);
        }
        
        private static void logInvalidType(ItemTag tag) {
            Debug.LogError(tag + " had invalid type in dict");
        }
        private static string serializeFluidContainer(object tagData) {
            return ItemSlotFactory.seralizeItemSlot(tagData as ItemSlot);
        }
        private static string serializeEnergyContainer(object tagData)
        {
            return tagData is not int energy ? null : JsonConvert.SerializeObject(energy);
        }

        private static string serializeCompactMachineTag(object tagData) {
            if (tagData is not string id) {
                logInvalidType(ItemTag.CompactMachine);
                return null;
            }
            return id;
        }

        private static string SerializeCaveData(object tagData)
        {
            return tagData as string;
        }

        private static string serializeStorageDriver(object tagData) {
            if (tagData is not List<ItemSlot> inventory) {
                logInvalidType(ItemTag.StorageDrive);
                return null;
            }
            return ItemSlotFactory.serializeList(inventory);
        }

        private static string seralizeRobot(object tagData) {
            return RobotDataFactory.Serialize(tagData as RobotItemData);
        }

        private static string seralizeEncodedRecipe(object tagData) {
            if (tagData is not EncodedRecipe encodedRecipe) {
                logInvalidType(ItemTag.EncodedRecipe);
                return null;
            }
            return EncodedRecipeFactory.seralize(encodedRecipe);
        }

        public static GameObject getVisualElement(this ItemTag tag, ItemSlot itemSlot, object tagData) {
            return tag switch {
                ItemTag.FluidContainer => getFluidContainerVisualElement(itemSlot,tagData),
                ItemTag.EnergyContainer => getEnergyContainerVisualElement(itemSlot,tagData),
                ItemTag.EncodedRecipe => getRecipeVisualElement(itemSlot, tagData),
                _ => visualDefaultSwitchCase(tag)
            };
        }

        public static bool getVisualLayer(this ItemTag tag) {
            return tag switch {
                ItemTag.EncodedRecipe => true,
                _ => false
            };
        }

        private static GameObject visualDefaultSwitchCase(ItemTag tag) {
            return null;
        }

        private static GameObject getRecipeVisualElement(ItemSlot itemSlot, object tagData) {
            if (tagData == null || tagData is not EncodedRecipe encodedRecipe) {
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
            }
            return null;
        }

        private static GameObject getFluidContainerVisualElement(ItemSlot itemSlot, object tagData) {
            if (tagData is not ItemSlot fluidItem) {
                return null;
            }
            
            if (itemSlot.itemObject is not IFluidContainerData fluidContainer) {
                return null;
            }  
            Vector2Int spriteSize = fluidContainer.GetFluidSpriteSize();
            if (spriteSize.Equals(Vector2Int.zero)) {
                return null;
            }
            GameObject fluidObject = new GameObject();
            Image image = fluidObject.AddComponent<Image>();
            image.sprite = fluidItem.itemObject.getSprite();
            RectTransform rectTransform = fluidObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = spriteSize;
            return fluidObject;
        }

        private static GameObject getEnergyContainerVisualElement(ItemSlot itemSlot, object tagData) {
            // TODO slider of Energy
            return null;
        }

        public static object deseralize(this ItemTag tag, string data) {
            if (data == null) {
                return null;
            }

            if (!deserializationMap.ContainsKey(tag))
            {
                Debug.LogError("ItemTagExtension method 'Deserialize' did not cover case for " + tag);
                return null;
            }

            return deserializationMap[tag].Invoke(data);
        }

        public static object copyData(this ItemTag tag, object data) {
            if (data == null) {
                return null;
            }
            // ? This is probably supposed to create deep copies
            return tag switch  {
                ItemTag.FluidContainer => data,
                ItemTag.EnergyContainer => data,
                ItemTag.CompactMachine => data,
                ItemTag.StorageDrive => data,
                ItemTag.EncodedRecipe => data,
                _ => data
            };
        }

        public static bool isEquivalent(this ItemTag tag, object first, object second) {
            if (first == null && second == null) {
                return true;
            }
            if (first == null) {
                return false;
            }
            if (second == null) {
                return false;
            }
            return tag switch  {
                ItemTag.FluidContainer => fluidContainerEqual(first,second),
                _ => first.Equals(second)
            };
        }

        private static bool fluidContainerEqual(object first, object second) {
            if (first == null && second == null) {
                return true;
            }
            if (first == null) {
                return false;
            }
            if (second == null) {
                return false;
            }
            if (first is not ItemSlot firstSlot) {
                return false;
            }
            if (second is not ItemSlot secondSlot) {
                return false;
            }
            if (!ItemSlotUtils.AreEqualNoNullCheck(firstSlot,secondSlot)) {
                return false;
            }
            return firstSlot.amount == secondSlot.amount;
        }
    }
}