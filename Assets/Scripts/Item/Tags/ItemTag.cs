using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using ItemModule.Tags.FluidContainers;

namespace ItemModule.Tags {
    public enum ItemTag {
        FluidContainer,
        EnergyContainer,
        CompactMachine,
        Inventory,
        RobotBlueprint,
        ItemFilter,
        FluidFilter
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

        public static GameObject getVisualElement(this ItemTag tag, ItemSlot itemSlot, object tagData) {
            return tag switch {
                ItemTag.FluidContainer => getFluidContainerVisualElement(itemSlot,tagData),
                ItemTag.EnergyContainer => getEnergyContainerVisualElement(itemSlot,tagData),
                ItemTag.CompactMachine => null,
                _ => visualDefaultSwitchCase(tag)
            };
        }

        private static GameObject visualDefaultSwitchCase(ItemTag tag) {
            Debug.LogError("ItemTagExtension method 'getVisualElement' did not cover case for " + tag);
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
            Vector2Int spriteSize = fluidContainer.getFluidSpriteSize();
            if (spriteSize.Equals(Vector2Int.zero)) {
                return null;
            }
            GameObject fluidObject = ItemSlotUIFactory.getItemImage(fluidItem);
            if (fluidObject != null) {
                RectTransform rectTransform = fluidObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = spriteSize;
            }
            return fluidObject;
        }

        private static GameObject getEnergyContainerVisualElement(ItemSlot itemSlot, object tagData) {
            // TODO slider of energy
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
            if (!ItemSlotHelper.areEqualNoNullCheck(firstSlot,secondSlot)) {
                return false;
            }
            return firstSlot.amount == secondSlot.amount;
        }
    }
}