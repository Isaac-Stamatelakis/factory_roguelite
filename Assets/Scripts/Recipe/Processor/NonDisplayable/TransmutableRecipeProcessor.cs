using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Transmutable;
using TileEntityModule;
using Items;

namespace RecipeModule.Transmutation {
    [CreateAssetMenu(fileName ="RP~New Transmutable Recipe Processor",menuName="Crafting/Processor/Transmutable")]
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

        public IMachineRecipe getRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
        {
            if (transmutableDict.ContainsKey(mode)) {
                foreach (TransmutablePair transmutablePair in transmutableDict[mode]) {
                    for (int n = 0; n < solidInputs.Count; n++) {
                        ItemSlot inputSlot = solidInputs[n];
                        if (inputSlot == null || inputSlot.itemObject == null) {
                            continue;
                        }
                        if (inputSlot.itemObject is not TransmutableItemObject transmutableItem) {
                            continue;
                        }
                        if (transmutableItem.getState() != transmutablePair.Input) {
                            continue;
                        }
                        TransmutableItemObject outputItemObject = transmutableItem.getMaterial().transmute(transmutablePair.Output);
                        if (outputItemObject == null) {
                            continue;
                        }
                        
                        float ratio = transmutablePair.Output.getComparedRatio(transmutablePair.Input);
                        
                        (int outputAmount,int inputAmount) = calculateAmounts(ratio);
                        if (inputSlot.amount < inputAmount) {
                            continue;
                        }

                        // Recipe is valid, now have to check there is space
                        ItemSlot outputItem = new ItemSlot(
                            itemObject: outputItemObject,
                            amount: outputAmount,
                            tags: null
                        );

                        if (!spaceInOutput(solidOutputs,outputItem)) {
                           continue;
                        }
                        // More than one slot left
                        inputSlot.amount -= inputAmount;
                        if (inputSlot.amount <= 0) {
                            solidInputs[n] = null;
                        }
                        
                        (int totalCost, int costPerTick) = calculateTickCost(
                            outputItemObject.getMaterial().tier,
                            transmutableItem.getState(),
                            outputItemObject.getState()
                        );
                        // For this case we do not require input item as its already consumed, required for recipe viewer
                        return new TransmutableRecipe(
                            input: null,
                            output: outputItem,
                            requiredEnergy: totalCost,
                            energyPerTick: costPerTick
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

        private (int outputAmount, int inputAmount) calculateAmounts(float ratio) {
            // EG: 16 ingots = 1 block, so ratio = 1/16. output amount = 1, input amount = 16
            //  1 ingot = 2 wires, so ratio = 2. output amount = 2, input amount = 1
            if (ratio < 1)
            {
                return (outputAmount: 1, inputAmount: Mathf.CeilToInt(1 / ratio));
            }
            else
            {
                return (outputAmount: Mathf.FloorToInt(ratio), inputAmount: 1);
            }
        }
        private (int totalCost, int costPerTick) calculateTickCost(Tier tier, TransmutableItemState inputState, TransmutableItemState outputState) {
            float fRatio = inputState.getComparedRatio(outputState);
            int energyCost = (int) fRatio*tier.getMaxEnergyUsage()*32;
            return (energyCost, tier.getMaxEnergyUsage());
        }

        public override List<IRecipe> getRecipes()
        {
            List<IRecipe> recipes = new List<IRecipe>();
            foreach (ItemObject itemObject in ItemRegistry.getInstance().getAllItems()) {
                if (itemObject is ITransmutableItem transmutableItem) {
                    TransmutableItemState inputState = transmutableItem.getState();
                    TransmutableItemMaterial material = transmutableItem.getMaterial();
                    foreach (List<TransmutablePair> transmutablePairs in transmutableDict.Values) {
                        foreach (TransmutablePair pair in transmutablePairs) {
                            if (pair.Input != inputState) {
                                continue;
                            } 
                            if (!material.canTransmute(pair.Output)) {
                                continue;
                            }
                            ItemObject outputItem = material.transmute(pair.Output);
                            // Can be transmuted by this processor
                            float ratio = pair.Output.getComparedRatio(pair.Input);
                            (int outputAmount, int inputAmount) = calculateAmounts(ratio);
                            ItemSlot input = ItemSlotFactory.createNewItemSlot(itemObject,inputAmount);
                            ItemSlot output = ItemSlotFactory.createNewItemSlot(outputItem,outputAmount);
                            (int totalCost, int costPerTick) = calculateTickCost(
                                transmutableItem.getMaterial().tier,
                                pair.Input,
                                pair.Output
                            );
                            recipes.Add(new TransmutableRecipe(
                                input: input,
                                output: output,
                                requiredEnergy: totalCost,
                                energyPerTick: costPerTick
                            ));
                            
                        }
                        
                    }
                }
            }
            return recipes;
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


