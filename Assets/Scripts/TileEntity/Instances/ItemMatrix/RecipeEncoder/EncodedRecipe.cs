using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Newtonsoft.Json;
using Items;

namespace TileEntity.Instances.Matrix {
    public class EncodedRecipe 
    {
        private List<ItemSlot> inputs;
        private List<ItemSlot> outputs;

        public List<ItemSlot> Inputs { get => inputs; set => inputs = value; }
        public List<ItemSlot> Outputs { get => outputs; set => outputs = value; }

        public EncodedRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs) {
            this.inputs = inputs;
            this.outputs = outputs;
        }

        public ItemSlot getOutput(string id, ItemTagKey tagKey) {
            foreach (ItemSlot output in outputs) {
                if (output == null || output.itemObject == null) {
                    continue;
                }
                string outputId = output.itemObject.id;
                ItemTagKey outputTagKey = new ItemTagKey(output.tags);
                if (id.Equals(outputId) && tagKey.Equals(outputTagKey)) {
                    return ItemSlotFactory.Copy(output);
                }
            }
            return null;
        }
    }

    public class EncodedRecipeFactory {
        private class SeralizedEncodedRecipe {
            public string inputs;
            public string outputs;
            public SeralizedEncodedRecipe(string inputs, string outputs) {
                this.inputs = inputs;
                this.outputs = outputs;
            }
        }
        public static string seralize(EncodedRecipe encodedRecipe) {
            if (encodedRecipe == null) {
                return null;
            }
            SeralizedEncodedRecipe seralizedEncodedRecipe = new SeralizedEncodedRecipe(
                inputs: ItemSlotFactory.serializeList(encodedRecipe.Inputs),
                outputs: ItemSlotFactory.serializeList(encodedRecipe.Outputs)
            );
            return JsonConvert.SerializeObject(seralizedEncodedRecipe);
        }

        public static EncodedRecipe deseralize(string data) {
            if (data == null) {
                return null;
            }
            try
            {
                SeralizedEncodedRecipe seralizedEncodedRecipe = JsonConvert.DeserializeObject<SeralizedEncodedRecipe>(data);
                return new EncodedRecipe(
                    inputs: ItemSlotFactory.Deserialize(seralizedEncodedRecipe.inputs),
                    outputs: ItemSlotFactory.Deserialize(seralizedEncodedRecipe.outputs)
                );
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
}

