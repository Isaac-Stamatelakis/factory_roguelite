using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Item.Transmutation;
using Recipe.Objects;
using Recipe.Viewer;
using TileEntity;
using UnityEngine;

namespace Items.Transmutable
{
    public static class TransmutableItemUtils
    {
        public const uint TRANSMUTATION_TICKS = 100; // 2 seconds to go from ingot to plate, can change this later
        public static string GetStateId(TransmutableItemMaterial material, TransmutableItemState state)
        {
            return GetStateName(material,state).Replace(" ", "_").ToLower();
        }
        

        public static string GetStateName(TransmutableItemMaterial material, TransmutableItemState state)
        {
            return GetStateName(material.name, state);
        }
        
        public static string GetStateName(string materialName, TransmutableItemState state)
        {
            string stateName = state.ToString();
            string[] split = stateName.Split("_");
            if (split.Length == 1)
            {
                return $"{materialName} {stateName}";
            }
            
            string prefix = split[0];
            string suffix = split[1];
            return $"{prefix} {materialName} {suffix}";
        }
        
        public static TransmutableItemObject GetDefaultObjectOfState(TransmutableItemState transmutableItemState)
        {
            const string DEFAULT_MATERIAL = "Iron";
            string name = GetStateName(DEFAULT_MATERIAL, transmutableItemState);

            string id = name.ToLower().Replace(" ", "_");
            return ItemRegistry.GetInstance().GetTransmutableItemObject(id);
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
            string outputID = TransmutableItemUtils.GetStateId(material, stateOption.state);
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

        public static float GetTransmutationRatio(TransmutableItemState inputState, TransmutableItemState outputState,
            float efficency = 1f)
        {
            uint gcd = GetGcd((uint)(inputState.getRatio()), (uint)(outputState.getRatio()));
            return outputState.getRatio() / inputState.getRatio() * efficency;
        }
        
        public static ItemSlot TransmuteOutput(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState, float efficency = 1f)
        {
            TransmutableItemObject outputItem = GetMaterialItem(material, outputState);
            float ratio = GetTransmutationRatio(inputState, outputState, efficency);
            ItemSlot output = new ItemSlot(outputItem, (uint)ratio, null);
            return output;
        }

        public static ItemSlot TransmuteOutput(TransmutableItemMaterial material, TransmutableRecipeObject recipeObject)
        {
            return TransmuteOutput(material,recipeObject.InputState,recipeObject.OutputState,recipeObject.Efficency.Value());
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

        public static string GetOreId(string baseId, TransmutableItemMaterial material)
        {
            return baseId + "_ore_" + material.name.ToLower();
        }
    }
}
