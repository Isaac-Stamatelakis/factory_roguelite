using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using Chunks;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixRecipeEncoderInstance : TileEntityInstance<MatrixRecipeEncoder>, 
        IRightClickableTileEntity, IMatrixConduitInteractable, ISerializableTileEntity, ILoadableTileEntity
    {
        public MatrixRecipeEncoderInstance(MatrixRecipeEncoder tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private List<ItemSlot> recipeOutputs;
        private List<ItemSlot> recipeInputs;
        private List<ItemSlot> blankRecipes;
        private List<ItemSlot> encodedRecipes;
        public List<ItemSlot> RecipeOutputs { get => recipeOutputs; }
        public List<ItemSlot> RecipeInputs { get => recipeInputs;  }
        public List<ItemSlot> BlankRecipes { get => blankRecipes;  }
        public List<ItemSlot> EncodedRecipes { get => encodedRecipes;  }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.Layout;
        }

        public void onRightClick()
        {
            GameObject uiPrefab = tileEntity.UIManager.getUIElement();
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            RecipeEncoderUI recipeEncoderUI = instantiated.GetComponent<RecipeEncoderUI>();
            recipeEncoderUI.init(this); 
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);

        }

        private void initInventories() {
            if (recipeOutputs == null || recipeOutputs.Count != tileEntity.RecipeOutputCount) {
                recipeOutputs = new List<ItemSlot>();
                ItemSlotFactory.clampList(recipeOutputs,tileEntity.RecipeOutputCount);
            }
            if (recipeInputs == null || recipeInputs.Count != tileEntity.RecipeInputCount) {
                recipeInputs = new List<ItemSlot>();
                ItemSlotFactory.clampList(recipeInputs,tileEntity.RecipeInputCount);
            }
            if (blankRecipes == null || blankRecipes.Count != tileEntity.BlankRecipeCount) {
                blankRecipes = new List<ItemSlot>();
                ItemSlotFactory.clampList(blankRecipes,tileEntity.BlankRecipeCount);
            }
            if (encodedRecipes == null || encodedRecipes.Count != tileEntity.EncodedRecipeCount) {
                encodedRecipes = new List<ItemSlot>();
                ItemSlotFactory.clampList(encodedRecipes,tileEntity.EncodedRecipeCount);
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

        public void syncToController(ItemMatrixControllerInstance matrixController)
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
                ItemSlotFactory.clampList(blankRecipes,tileEntity.BlankRecipeCount);

                encodedRecipes = ItemSlotFactory.deserialize(seralizedMatrixRecipeEncoder.encodedRecipes);
                ItemSlotFactory.clampList(encodedRecipes,tileEntity.EncodedRecipeCount);
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

        public void load()
        {
            initInventories();
        }

        public void unload()
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
