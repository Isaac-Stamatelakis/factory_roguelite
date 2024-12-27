using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Conduits.Systems;
using Newtonsoft.Json;
using Conduits.Ports;
using Items.Tags;
using UI;

namespace TileEntity.Instances.Matrix {
    public class MatrixInterfaceInstance : TileEntityInstance<MatrixInterface>,
        IConduitTileEntity, ISolidItemConduitInteractable, IFluidConduitInteractable, 
        IMatrixConduitInteractable, ISerializableTileEntity, IRightClickableTileEntity
    {
        public MatrixInterfaceInstance(MatrixInterface tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private ItemMatrixControllerInstance controller;
        private int priority;
        public int Priority {get => priority;}
        public List<ItemSlot> Upgrades { get => upgrades; }
        public List<ItemSlot> Recipes { get => recipes; }
        public ItemMatrixControllerInstance Controller { get => controller;}

        private List<ItemSlot> upgrades;
        private List<ItemSlot> recipes;
        private MatrixConduitSystem system;


        public ItemSlot ExtractFluidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.Layout;
        }

        public void InsertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void InsertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void SyncToController(ItemMatrixControllerInstance matrixController)
        {
            this.controller = matrixController;
        }

        public void SyncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            this.system = matrixConduitSystem;
            matrixConduitSystem.addInterface(this);
        }

        public void iteratePriority(int amount) {
            priority += amount;
        }

        public string serialize()
        {
            SeralizedMatrixInterface seralizedMatrixInterface = new SeralizedMatrixInterface(
                priority,
                ItemSlotFactory.serializeList(upgrades),
                ItemSlotFactory.serializeList(recipes)
            );
            return JsonConvert.SerializeObject(seralizedMatrixInterface);
        }

        public void unserialize(string data)
        {
            if (data == null) {
                initInventories();
                return;
            }
            SeralizedMatrixInterface seralizedMatrixInterface = JsonConvert.DeserializeObject<SeralizedMatrixInterface>(data);
            priority = seralizedMatrixInterface.priority;
            upgrades = ItemSlotFactory.Deserialize(seralizedMatrixInterface.upgrades);
            recipes = ItemSlotFactory.Deserialize(seralizedMatrixInterface.recipes);
        }

        private void initInventories() {
            priority = 0;

            upgrades = new List<ItemSlot>{
                null
            };
            int recipeCount = 9;
            recipes = new List<ItemSlot>(); 
            for (int i = 0; i < recipeCount; i++) {
                recipes.Add(null);
            }  
            return;
        }


        public List<EncodedRecipe> getRecipes() {
            List<EncodedRecipe> encodedRecipes = new List<EncodedRecipe>();
            if (recipes == null) {
                return encodedRecipes;
            }
            foreach (ItemSlot itemSlot in recipes) {
                encodedRecipes.Add(getRecipe(itemSlot));
            }
            return encodedRecipes;
        }

        public EncodedRecipe getRecipe(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null || itemSlot.tags == null || !itemSlot.tags.Dict.ContainsKey(ItemTag.EncodedRecipe)) {
                return null;
            }
            object data = itemSlot.tags.Dict[ItemTag.EncodedRecipe];
            if (data == null || data is not EncodedRecipe encodedRecipe) {
                return null;
            }
            return encodedRecipe;
        }
        public void onRightClick()
        {
            if (upgrades == null || recipes == null) {
                initInventories();
            }
            MatrixInterfaceUI matrixInterfaceUI = MatrixInterfaceUI.newInstance();
            matrixInterfaceUI.init(this);
            MainCanvasController.Instance.DisplayObject(matrixInterfaceUI.gameObject);
        }

        public void RemoveFromSystem()
        {
            system.removeInterface(this);
        }

        private class SeralizedMatrixInterface {
            public int priority;
            public string upgrades;
            public string recipes;
            public SeralizedMatrixInterface(int priority, string upgrades, string recipes) {
                this.priority = priority;
                this.upgrades = upgrades;
                this.recipes = recipes;
            }
        }
    }
}
