using System.Collections;
using System.Collections.Generic;
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
        FluidFilter,
        EncodedRecipe,
        StorageDrive,
        RobotData
    }   
    public static class ItemTagExtension {
        public static string serialize(this ItemTag tag, ItemTagCollection tagCollection) {
            if (!tagCollection.Dict.ContainsKey(tag)) {
                Debug.LogError("Attempted to deseralize " + tag + " which was not in TagCollection");
                return null;
            }
            
            object tagData = tagCollection.Dict[tag];
            return tag switch {
                ItemTag.FluidContainer => serializeFluidContainer(tagData),
                ItemTag.EnergyContainer => serializeEnergyContainer(tagData),
                ItemTag.CompactMachine => serializeCompactMachineTag(tagData),
                ItemTag.StorageDrive => serializeStorageDriver(tagData),
                ItemTag.EncodedRecipe => seralizeEncodedRecipe(tagData),
                ItemTag.RobotData => seralizeRobot(tagData),
                _ => serializeDefaultSwitchCase(tag)
            };
        }

        private static string serializeDefaultSwitchCase(ItemTag tag) {
            Debug.LogError("ItemTagExtension method 'seralize' did not cover case for " + tag);
            return null;
        }

        private static void logInvalidType(ItemTag tag) {
            Debug.LogError(tag + " had invalid type in dict");
        }
        private static string serializeFluidContainer(object tagData) {
            if (tagData == null) {
                return null;
            }
            if (tagData is not ItemSlot fluidItem) {
                logInvalidType(ItemTag.FluidContainer);
                return null;
            }
            return ItemSlotFactory.seralizeItemSlot(fluidItem);
        }
        private static string serializeEnergyContainer(object tagData) {
            if (tagData is not int energy) {
                logInvalidType(ItemTag.EnergyContainer);
                return null;
            }
            return JsonConvert.SerializeObject(energy);
        }

        private static string serializeCompactMachineTag(object tagData) {
            if (tagData is not string id) {
                logInvalidType(ItemTag.CompactMachine);
                return null;
            }
            return id;
        }

        private static string serializeStorageDriver(object tagData) {
            if (tagData is not List<ItemSlot> inventory) {
                logInvalidType(ItemTag.StorageDrive);
                return null;
            }
            return ItemSlotFactory.serializeList(inventory);
        }

        private static string seralizeRobot(object tagData) {
            if (tagData == null) {
                return null;
            }
            if (tagData is not RobotItemData robotItemData) {
                logInvalidType(ItemTag.RobotData);
                return null;
            }
            return RobotDataFactory.seralize(robotItemData);
        }

        private static string seralizeEncodedRecipe(object tagData) {
            if (tagData == null) {
                return null;
            }
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
            if (tagData == null) {
                return null;
            }
            if (tagData is not ItemSlot fluidItem) {
                return null;
            }
            
            if (itemSlot.itemObject == null || itemSlot.itemObject is not IFluidContainer fluidContainer) {
                return null;
            }  
            Vector2Int spriteSize = fluidContainer.GetFluidSpriteSize();
            if (spriteSize.Equals(Vector2Int.zero)) {
                return null;
            }
            GameObject fluidObject = new GameObject();
            Image image = fluidObject.AddComponent<Image>();
            image.sprite = fluidItem.itemObject.getSprite();
            RectTransform rectTransform = fluidObject.AddComponent<RectTransform>();
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
            return tag switch  {
                ItemTag.FluidContainer => ItemSlotFactory.deseralizeItemSlotFromString(data),
                ItemTag.EnergyContainer => JsonConvert.DeserializeObject<int>(data),
                ItemTag.CompactMachine => data,
                ItemTag.StorageDrive => ItemSlotFactory.Deserialize(data),
                ItemTag.EncodedRecipe => EncodedRecipeFactory.deseralize(data),
                ItemTag.RobotData => RobotDataFactory.deseralize(data),
                _ => deserializeDefaultSwitchCase(tag)
            };
        }

        public static object copyData(this ItemTag tag, object data) {
            if (data == null) {
                return null;
            }
            return tag switch  {
                ItemTag.FluidContainer => data,
                ItemTag.EnergyContainer => data,
                ItemTag.CompactMachine => data,
                ItemTag.StorageDrive => data,
                ItemTag.EncodedRecipe => data,
                _ => deserializeDefaultSwitchCase(tag)
            };
        }
        private static object deserializeDefaultSwitchCase(ItemTag tag) {
            Debug.LogError("ItemTagExtension method 'deseralize' did not cover case for " + tag);
            return null;
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
            if (!ItemSlotHelper.AreEqualNoNullCheck(firstSlot,secondSlot)) {
                return false;
            }
            return firstSlot.amount == secondSlot.amount;
        }
    }
}