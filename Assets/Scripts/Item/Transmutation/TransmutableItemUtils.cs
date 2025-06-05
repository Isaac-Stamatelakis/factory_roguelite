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
            return GetStateName(material, material.name, state);
        }
        
        public static string GetStateName(TransmutableItemMaterial material, string materialName, TransmutableItemState state)
        {
            TransmutableMaterialState materialState = material?.MaterialOptions?.transmutableMaterialState ?? TransmutableMaterialState.None;
            if (materialState == TransmutableMaterialState.Metal && state == TransmutableItemState.Liquid)
            {
                return $"Molten {materialName}";
            }
            
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
        
        public static ITransmutableItem GetDefaultObjectOfState(TransmutableItemState transmutableItemState)
        {
            const string DEFAULT_MATERIAL = "Iron";
            string name = GetStateName(null,DEFAULT_MATERIAL, transmutableItemState);

            string id = name.ToLower().Replace(" ", "_");
            return ItemRegistry.GetInstance().GetTransmutableItemObject(id);
        }

        
        public static ITransmutableItem GetMaterialItem(TransmutableItemMaterial material, TransmutableItemState state) {
            string outputID = GetStateId(material, state);
            return ItemRegistry.GetInstance().GetTransmutableItemObject(outputID);
        }
        public static (ItemSlot,ItemSlot) Transmute(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState, float efficency = 1f)
        {
            ITransmutableItem inputItem = GetMaterialItem(material, inputState);
            ITransmutableItem outputItem = GetMaterialItem(material, outputState);
            var (inputAmount, outputAmount) = GetInputOutputAmount(inputState, outputState, efficency);
            ItemSlot input = new ItemSlot((ItemObject)inputItem, inputAmount, null);
            ItemSlot output = new ItemSlot((ItemObject)outputItem, outputAmount, null);
            return (input, output);
        }

        public static float GetTransmutationRatio(TransmutableItemState inputState, TransmutableItemState outputState,
            float efficency = 1f)
        {
            return outputState.GetRatio() / inputState.GetRatio() * efficency;
        }

        public static (uint, uint) GetInputOutputAmount(TransmutableItemState inputState,  TransmutableItemState outputState, float efficency = 1f)
        {
            float transmutationRatio = GetTransmutationRatio(inputState, outputState);
            uint inputAmount = (uint)(transmutationRatio * inputState.GetRatio());
            uint outputAmount = (uint)(transmutationRatio * outputState.GetRatio());
            uint gcd = GetGcd(inputAmount,outputAmount);
            inputAmount /= gcd;
            outputAmount /= gcd;
            return (inputAmount, outputAmount);
        }
        
        public static ItemSlot TransmuteOutput(TransmutableItemMaterial material, TransmutableItemState inputState, TransmutableItemState outputState, float efficency = 1f)
        {
            ITransmutableItem outputItem = GetMaterialItem(material, outputState);
            float ratio = GetTransmutationRatio(inputState, outputState, efficency);
            
            uint inputAmount = (uint)(ratio * inputState.GetRatio());
            uint outputAmount = (uint)(ratio * outputState.GetRatio());
            uint gcd = GetGcd(inputAmount,outputAmount);
            ItemSlot output = new ItemSlot((ItemObject)outputItem, outputAmount/gcd, null);
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
