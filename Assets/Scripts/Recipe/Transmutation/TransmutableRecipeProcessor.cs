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
        private Dictionary<int,List<TransmutablePair>> transmutableDict;

        protected override IRecipe getValidRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs,int mode)
        {
            if (transmutableDict == null) {
                initDict();
            }   
            if (transmutableDict.ContainsKey(mode)) {
                foreach (TransmutablePair transmutablePair in transmutableDict[mode]) {
                    for (int n = 0; n < inputs.Count; n++) {
                        //Debug.Log(n);
                        ItemSlot inputSlot = inputs[n];
                        if (inputSlot == null || inputSlot.itemObject == null) {
                            continue;
                        }
                        if (inputSlot.itemObject is not TransmutableItemObject) {
                            continue;
                        }
                        TransmutableItemObject transmutableItem = (TransmutableItemObject) inputSlot.itemObject;
                        if (transmutableItem.state != transmutablePair.Input) {
                            continue;
                        }
                        // Recipe is valid, now have to check there is space
                        TransmutableItemObject outputItemObject = transmutableItem.material.transmute(transmutablePair.Output);
                        int ratio = Mathf.FloorToInt(transmutablePair.Output.getComparedRatio(transmutablePair.Input));
                        ItemSlot outputItem = new ItemSlot(
                            itemObject: outputItemObject,
                            amount: ratio,
                            nbt: null
                        );

                        if (!spaceInOutput(outputs,outputItem)) {
                           continue;
                        }
                        // More than one slot left
                        inputSlot.amount--;
                        if (inputSlot.amount <= 0) {
                            inputs[n] = null;
                        }

                        return new TransmutableRecipe(
                            outputItem,
                            0, // TODO energy
                            0
                        );
                    }
                }
            }
            // Check recipes
            return base.getValidRecipe(inputs, outputs, mode);
        }

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
        private void initDict() {
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


