using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;
using Newtonsoft.Json;

namespace RecipeModule {
    public class MatrixRecipe
    {
        private List<ItemSlot> inputs;
        private List<ItemSlot> outputs;

        public MatrixRecipe(List<ItemSlot> inputs, List<ItemSlot> outputs) {
            this.inputs = inputs;
            this.outputs = outputs;
        }
        public List<ItemSlot> Inputs { get => inputs; }
        public List<ItemSlot> Outputs { get => outputs;  }
    }

    public class SerializedRecipe {
        public string inputs;
        public string outputs;

        public SerializedRecipe(string inputs, string outputs) {
            this.inputs = inputs;
            this.outputs = outputs;
        }
    }

    public class RecipeSeralizationFactory {
        public static string seralizeRecipe(MatrixRecipe matrixRecipe) {
            SerializedRecipe serializedRecipe = new SerializedRecipe(
                inputs: ItemSlotFactory.serializeList(matrixRecipe.Inputs),
                outputs: ItemSlotFactory.serializeList(matrixRecipe.Outputs)
            );
            return JsonConvert.SerializeObject(serializedRecipe);
        }

        public static MatrixRecipe deseralizeRecipe(string json) {
            if (json == null) {
                return null;
            }
            SerializedRecipe serializedRecipe = JsonConvert.DeserializeObject<SerializedRecipe>(json);
            return new MatrixRecipe(
                inputs: ItemSlotFactory.deserialize(serializedRecipe.inputs),
                outputs: ItemSlotFactory.deserialize(serializedRecipe.outputs)
            );
        }
    }
}

