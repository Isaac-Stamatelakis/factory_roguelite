using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Recipe Encoder")]
    public class MatrixRecipeEncoder : TileEntity, IRightClickableTileEntity, IMatrixConduitInteractable, ISerializableTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private List<ItemSlot> recipeOutputs;
        [SerializeField] private List<ItemSlot> recipeInputs;
        [SerializeField] private List<ItemSlot> blankRecipes;
        [SerializeField] private List<ItemSlot> encodedRecipes;
        [SerializeField] private int recipeOutputCount;
        [SerializeField] private int recipeInputCount;
        [SerializeField] private int blankRecipeCount;
        [SerializeField] private int encodedRecipeCount;

        public List<ItemSlot> RecipeOutputs { get => recipeOutputs; }
        public List<ItemSlot> RecipeInputs { get => recipeInputs;  }
        public List<ItemSlot> BlankRecipes { get => blankRecipes;  }
        public List<ItemSlot> EncodedRecipes { get => encodedRecipes;  }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void onRightClick()
        {
            initInventories();
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            RecipeEncoderUI recipeEncoderUI = instantiated.GetComponent<RecipeEncoderUI>();
            recipeEncoderUI.init(this); 
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);

        }

        private void initInventories() {
            if (recipeOutputs == null || recipeOutputs.Count != recipeOutputCount) {
                recipeOutputs = new List<ItemSlot>();
                ItemSlotFactory.clampList(recipeOutputs,recipeOutputCount);
            }
            if (recipeInputs == null || recipeInputs.Count != recipeInputCount) {
                recipeInputs = new List<ItemSlot>();
                ItemSlotFactory.clampList(recipeInputs,recipeInputCount);
            }
            if (blankRecipes == null || blankRecipes.Count != blankRecipeCount) {
                blankRecipes = new List<ItemSlot>();
                ItemSlotFactory.clampList(blankRecipes,blankRecipeCount);
            }
            if (encodedRecipes == null || encodedRecipes.Count != encodedRecipeCount) {
                encodedRecipes = new List<ItemSlot>();
                ItemSlotFactory.clampList(encodedRecipes,encodedRecipeCount);
            }
        }

        public string serialize()
        {
            SeralizedMatrixRecipeEncoder seralizedMatrixRecipeEncoder = new SeralizedMatrixRecipeEncoder(
                ItemSlotFactory.serializeList(blankRecipes),
                ItemSlotFactory.serializeList(encodedRecipes)
            );
            return JsonConvert.SerializeObject(seralizedMatrixRecipeEncoder);
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            
        }

        public void unserialize(string data)
        {
            if (data == null) {
                initInventories();
                return;
            }
            try
            {
                SeralizedMatrixRecipeEncoder seralizedMatrixRecipeEncoder = JsonConvert.DeserializeObject<SeralizedMatrixRecipeEncoder>(data);
                blankRecipes = ItemSlotFactory.deserialize(seralizedMatrixRecipeEncoder.blankRecipes);
                ItemSlotFactory.clampList(blankRecipes,blankRecipeCount);

                encodedRecipes = ItemSlotFactory.deserialize(seralizedMatrixRecipeEncoder.encodedRecipes);
                ItemSlotFactory.clampList(encodedRecipes,encodedRecipeCount);
            }  
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                initInventories();
            }

        }

        public void removeFromSystem()
        {
            
        }

        private class SeralizedMatrixRecipeEncoder {
            public string blankRecipes;
            public string encodedRecipes;
            public SeralizedMatrixRecipeEncoder(string blankRecipes, string encodedRecipe) {
                this.blankRecipes = blankRecipes;
                this.encodedRecipes = encodedRecipe;
            }
        }
    }

}
