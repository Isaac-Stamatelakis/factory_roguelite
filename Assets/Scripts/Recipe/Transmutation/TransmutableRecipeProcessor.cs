using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Transmutable;
using TileEntityModule;

namespace RecipeModule.Transmutation {
    [CreateAssetMenu(fileName ="RP~New Transmutable Recipe Processor",menuName="Crafting/Transmutation Processor")]
    public class TransmutableRecipeProcessor : RecipeProcessor, ITransmutableRecipeProcessor, IInitableRecipeProcessor
    {
        [Header("Valid Input States")]
        public List<KeyDoubleValue<int,TransmutableItemState,TransmutableItemState>> transmutableStates;
        private Dictionary<int,List<TransmutablePair>> transmutableDict;

        private bool spaceInOutput(List<ItemSlot> outputs, ItemSlot outputItemSlot) {
            foreach (ItemSlot itemInOutputSlot in outputs) {
                if (itemInOutputSlot == null || itemInOutputSlot.itemObject == null) {
                    return true;
                }
                if (itemInOutputSlot.itemObject.id != outputItemSlot.itemObject.id) {
                    continue;
                }
                if (itemInOutputSlot.amount + outputItemSlot.amount <= Global.MaxSize) {
                    return true;
                }
            }
            return false;
        }
        /// Summary:
        ///     Unity doesn't support serialized field dictionarys so we need to transform input field to dict
        public void init() {
            transmutableDict = new Dictionary<int, List<TransmutablePair>>();
            foreach (KeyDoubleValue<int,TransmutableItemState,TransmutableItemState> keyDoubleValue in transmutableStates) {
                TransmutablePair transmutablePair = new TransmutablePair(
                    keyDoubleValue.input,
                    keyDoubleValue.output
                );
                if (transmutableDict.ContainsKey(keyDoubleValue.key)) {
                    transmutableDict[keyDoubleValue.key].Add(transmutablePair);
                } else {
                    transmutableDict[keyDoubleValue.key] = new List<TransmutablePair>{transmutablePair};
                }
            }
        }

        public TransmutableRecipe getValidRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
        {
            if (transmutableDict.ContainsKey(mode)) {
                foreach (TransmutablePair transmutablePair in transmutableDict[mode]) {
                    for (int n = 0; n < solidInputs.Count; n++) {
                        ItemSlot inputSlot = solidInputs[n];
                        if (inputSlot == null || inputSlot.itemObject == null) {
                            continue;
                        }
                        if (inputSlot.itemObject is not TransmutableItemObject) {
                            continue;
                        }
                        TransmutableItemObject transmutableItem = (TransmutableItemObject) inputSlot.itemObject;
                        if (transmutableItem.getState() != transmutablePair.Input) {
                            continue;
                        }
                        // Recipe is valid, now have to check there is space
                        TransmutableItemObject outputItemObject = transmutableItem.getMaterial().transmute(transmutablePair.Output);
                        int ratio = Mathf.FloorToInt(transmutablePair.Output.getComparedRatio(transmutablePair.Input));
                        ItemSlot outputItem = new ItemSlot(
                            itemObject: outputItemObject,
                            amount: ratio,
                            tags: null
                        );

                        if (!spaceInOutput(solidOutputs,outputItem)) {
                           continue;
                        }
                        // More than one slot left
                        inputSlot.amount--;
                        if (inputSlot.amount <= 0) {
                            solidInputs[n] = null;
                        }
                        Tier tier = outputItemObject.getMaterial().tier;
                        TransmutableItemState state = outputItemObject.getState();
                        float fRatio = state.getComparedRatio(transmutableItem.getState());
                        int energyCost = (int) fRatio*tier.getMaxEnergyUsage()*32;
                        return new TransmutableRecipe(
                            outputItem,
                            energyCost,
                            tier.getMaxEnergyUsage()
                        );
                    }
                }
            }
            return null;
        }

        public override int getRecipeCount()
        {
            return transmutableStates.Count;
        }

        private class TransmutablePair {
            private TransmutableItemState input;
            private TransmutableItemState output;
            public TransmutablePair(TransmutableItemState input, TransmutableItemState output) {
                this.Input = input;
                this.Output = output;
            }

            public TransmutableItemState Input { get => input; set => input = value; }
            public TransmutableItemState Output { get => output; set => output = value; }
        }
        [System.Serializable]
        public class KeyDoubleValue<K,I,O> {
            public K key;
            public I input;
            public O output;
        }
    }
    
}


