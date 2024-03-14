using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
                _ => defaultSwitchCase(tag)
            };
        }

        private static string defaultSwitchCase(ItemTag tag) {
            Debug.LogError("ItemTagExtension method 'seralize' did not cover case for " + tag);
            return null;
        }

        private static void logInvalidType(ItemTag tag) {
            Debug.LogError(tag + " had invalid type in dict");
        }
        private static string serializeFluidContainer(object tagData) {
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
    }
}