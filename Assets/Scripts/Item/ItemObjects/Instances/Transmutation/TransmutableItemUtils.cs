using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using TileEntity;
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
        
        public static TransmutableItemObject GetMaterialItem(TransmutableItemMaterial material, TransmutableItemState state) {
            if (!CanTransmute(material, state)) {
                return null;
            }

            TransmutableStateOptions stateOption = material.GetOptionStateDict()[state];
            string outputID = TransmutableItemUtils.GetStateId(material, stateOption);
            return ItemRegistry.GetInstance().GetTransmutableItemObject(outputID);
        }

        public static uint GetTransmutationRatio(TransmutableItemState from, TransmutableItemState to) {
            return (uint)(from.getRatio()/to.getRatio());
        }

        public static ItemSlot Transmute(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState)
        {
            TransmutableItemObject transmutableItemObject = TransmutableItemUtils.GetMaterialItem(material, outputState);
            return new ItemSlot(transmutableItemObject, 
                TransmutableItemUtils.GetTransmutationRatio(inputState,outputState), 
                null
            );
        }

        public static string FormatChemicalFormula(string chemicalFormula)
        {
            string returnText = "";
            bool sub = false;
            foreach (char c in chemicalFormula)
            {
                if (IsSubscript(c) && !sub)
                {
                    sub = true;
                    returnText += "<sub>";
                }

                if (!IsSubscript(c) && sub)
                {
                    sub = false;
                    returnText += "</sub>";
                }
                returnText += c;
            }
            if (sub) return returnText + "</sub>";
            return returnText;
        }

        private static bool IsSubscript(char c)
        {
            return char.IsDigit(c) || c == '*';
        }
    }
}
