using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RecipeModule.Transmutation {
    [CreateAssetMenu(fileName ="RP~New Transmutable Recipe Processor",menuName="Crafting/Transmutation Processor")]
    public class TransmutableRecipeProcessor : RecipeProcessor
    {
        [Header("Minimum Accepted Tier (Inclusive)")]
        public int minimumTier = -9999;
        [Header("Maximum Accepted Tier (Inclusive)")]
        public int maximumTier = 9999;
        [Header("Valid Input States")]
        public List<KeyDoubleValue<int,TransmutableItemState,TransmutableItemState>> transmutableStates;
        private Dictionary<int,TransmutablePair> transmutableDict;
        [Header("Output State")]
        public TransmutableItemState outputState;

        protected override IRecipe getValidRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs,int firstAvaiableOutputIndex, int mode)
        {
            if (transmutableDict == null) {
                initDict();
            }
            if (transmutableDict.ContainsKey(mode)) {
                TransmutablePair transmutablePair = transmutableDict[mode];
                int successIndex = -1;
                foreach (ItemSlot itemSlot in inputs) {
                    successIndex ++;
                    if (itemSlot == null || itemSlot.itemObject == null) {
                        continue;
                    }
                    if (itemSlot.itemObject is not TransmutableItemObject) {
                        continue;
                    }
                    TransmutableItemObject transmutableItem = (TransmutableItemObject) itemSlot.itemObject;
                    if (transmutableItem.state != transmutablePair.Input) {
                        continue;
                    }
                    // Match
                    if (firstAvaiableOutputIndex >= outputs.Count) {
                        // No space
                        continue;
                    }
                    // Only one slot left
                    if (firstAvaiableOutputIndex == outputs.Count-1) {
                        ItemSlot lastItemSlot = outputs[firstAvaiableOutputIndex];
                        if (lastItemSlot == null || lastItemSlot.itemObject == null || lastItemSlot.itemObject.id == transmutableItem.id) {
                            return success(inputs,successIndex,transmutableItem,transmutablePair.Output);
                        }
                    }
                    // More than one slot left
                    return success(inputs,successIndex,transmutableItem,transmutablePair.Output);
                }
            }
            // Check recipes
            return base.getValidRecipe(inputs, outputs, firstAvaiableOutputIndex, mode);
        }

        private IRecipe success(List<ItemSlot> inputs, int successIndex, TransmutableItemObject transmutableItem, TransmutableItemState outputState) {
            ItemSlot input = inputs[successIndex];
            input.amount-=1; // TODO change ratio
            if (input.amount <= 0) {    
                inputs[successIndex] = null;
            }
            TransmutableItemObject outputItemObject = transmutableItem.material.transmute(outputState);
            ItemSlot outputItem = new ItemSlot(
                itemObject: outputItemObject,
                amount: 1, // TODO change ratio
                nbt: null
            );
            return new TransmutableRecipe(
                outputItem,
                0, // TODO energy
                0
            );
        }
        /// Summary:
        ///     Unity doesn't support serialized field dictionarys so we need to transform input field to dict
        private void initDict() {
            transmutableDict = new Dictionary<int, TransmutablePair>();
            foreach (KeyDoubleValue<int,TransmutableItemState,TransmutableItemState> keyDoubleValue in transmutableStates) {
                transmutableDict[keyDoubleValue.key] = new TransmutablePair(
                    keyDoubleValue.input,
                    keyDoubleValue.output
                );
            }
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


