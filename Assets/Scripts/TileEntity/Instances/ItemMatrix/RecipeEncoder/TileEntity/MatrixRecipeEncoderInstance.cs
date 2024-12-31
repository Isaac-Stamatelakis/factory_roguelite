using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using Chunks;
using Item.Slot;
using UI;

namespace TileEntity.Instances.Matrix {
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
            return TileEntityObject.Layout;
        }

        public void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.UIManager.getUIElement();
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            RecipeEncoderUI recipeEncoderUI = instantiated.GetComponent<RecipeEncoderUI>();
            recipeEncoderUI.init(this); 
            MainCanvasController.Instance.DisplayObject(instantiated);
        }

        private void initInventories() {
            if (recipeOutputs == null || recipeOutputs.Count != TileEntityObject.RecipeOutputCount) {
                recipeOutputs = new List<ItemSlot>();
                ItemSlotFactory.ClampList(recipeOutputs,TileEntityObject.RecipeOutputCount);
            }
            if (recipeInputs == null || recipeInputs.Count != TileEntityObject.RecipeInputCount) {
                recipeInputs = new List<ItemSlot>();
                ItemSlotFactory.ClampList(recipeInputs,TileEntityObject.RecipeInputCount);
            }
            if (blankRecipes == null || blankRecipes.Count != TileEntityObject.BlankRecipeCount) {
                blankRecipes = new List<ItemSlot>();
                ItemSlotFactory.ClampList(blankRecipes,TileEntityObject.BlankRecipeCount);
            }
            if (encodedRecipes == null || encodedRecipes.Count != TileEntityObject.EncodedRecipeCount) {
                encodedRecipes = new List<ItemSlot>();
                ItemSlotFactory.ClampList(encodedRecipes,TileEntityObject.EncodedRecipeCount);
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

        public void SyncToController(ItemMatrixControllerInstance matrixController)
        {
            
        }

        public void SyncToSystem(MatrixConduitSystem matrixConduitSystem)
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
                blankRecipes = ItemSlotFactory.Deserialize(seralizedMatrixRecipeEncoder.blankRecipes);
                ItemSlotFactory.ClampList(blankRecipes,TileEntityObject.BlankRecipeCount);

                encodedRecipes = ItemSlotFactory.Deserialize(seralizedMatrixRecipeEncoder.encodedRecipes);
                ItemSlotFactory.ClampList(encodedRecipes,TileEntityObject.EncodedRecipeCount);
            }  
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                initInventories();
            }

        }

        public void RemoveFromSystem()
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
