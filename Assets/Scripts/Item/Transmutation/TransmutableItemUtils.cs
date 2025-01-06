using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Recipe.Objects;
using Recipe.Viewer;
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
        public static (ItemSlot,ItemSlot) Transmute(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState, float efficency = 1f)
        {
            TransmutableItemObject inputItem = GetMaterialItem(material, inputState);
            TransmutableItemObject outputItem = GetMaterialItem(material, outputState);
            uint gcd = GetGcd((uint)(1 / inputState.getRatio()), (uint)(1 / outputState.getRatio()));
            uint inputAmount = (uint)(gcd * inputState.getRatio() / efficency);
            uint outputAmount = (uint)(gcd * outputState.getRatio());
            ItemSlot input = new ItemSlot(inputItem, inputAmount, null);
            ItemSlot output = new ItemSlot(outputItem, outputAmount, null);
            return (input, output);
        }

        public static uint GetTransmutationRatio(TransmutableItemState inputState, TransmutableItemState outputState,
            float efficency = 1f)
        {
            uint gcd = GetGcd((uint)(1 / inputState.getRatio()), (uint)(1 / outputState.getRatio()));
            return (uint)(gcd * inputState.getRatio()*efficency);
        }
        
        public static ItemSlot TransmuteOutput(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState, float efficency = 1f)
        {
            TransmutableItemObject outputItem = GetMaterialItem(material, outputState);
            uint gcd = GetGcd((uint)(1 / inputState.getRatio()), (uint)(1 / outputState.getRatio()));
            uint outputAmount = (uint)(gcd * outputState.getRatio() * efficency);
            ItemSlot output = new ItemSlot(outputItem, outputAmount, null);
            return (output);
        }
        
        private static uint GetGcd(uint a, uint b)
        {
            while (b != 0)
            {
                uint temp = b;
                b = a % b;
                a = temp;
            }

            return a;
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
