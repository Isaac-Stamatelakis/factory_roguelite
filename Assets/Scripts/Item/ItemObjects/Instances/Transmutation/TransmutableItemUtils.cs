using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Transmutable
{
    public static class TransmutableItemUtils
    {
        public static string GetStateId(TransmutableItemMaterial material, TransmutableStateOptions options)
        {
            return GetStateName(material,options).Replace(" ", "_").ToLower();
        }

        public static string GetStateName(TransmutableItemMaterial material, TransmutableStateOptions options)
        {
            string stateName = options.state.ToString();
            string[] split = stateName.Split("_");
            if (split.Length == 1)
            {
                return $"{material.name} {stateName}";
            }
            
            string prefix = split[0];
            string suffix = split[1];
            return $"{prefix} {material.name} {suffix}";
        }

        public static bool CanTransmute(TransmutableItemMaterial material, TransmutableItemState state)
        {
            return material.GetOptionStateDict().ContainsKey(state);
        }
        
        public static TransmutableItemObject Transmute(TransmutableItemMaterial material, TransmutableItemState state) {
            if (!CanTransmute(material, state)) {
                return null;
            }

            TransmutableStateOptions stateOption = material.GetOptionStateDict()[state];
            string outputID = TransmutableItemUtils.GetStateId(material, stateOption);
            return ItemRegistry.getInstance().getTransmutableItemObject(outputID);
        }
    }
}
