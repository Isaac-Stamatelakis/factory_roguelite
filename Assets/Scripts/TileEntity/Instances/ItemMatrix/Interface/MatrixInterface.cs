using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;
using Newtonsoft.Json;
using ItemModule.Tags;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Interface")]
    public class MatrixInterface : TileEntity, 
        IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IMatrixConduitInteractable,
        ISerializableTileEntity, IRightClickableTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        private ItemMatrixController controller;
        private int priority;
        public int Priority {get => priority;}
        public List<ItemSlot> Upgrades { get => upgrades; }
        public List<ItemSlot> Recipes { get => recipes; }
        public ItemMatrixController Controller { get => controller;}

        private List<ItemSlot> upgrades;
        private List<ItemSlot> recipes;
        private MatrixConduitSystem system;


        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            this.controller = matrixController;
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
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
            upgrades = ItemSlotFactory.deserialize(seralizedMatrixInterface.upgrades);
            recipes = ItemSlotFactory.deserialize(seralizedMatrixInterface.recipes);
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
            GlobalUIContainer.getInstance().getUiController().setGUI(matrixInterfaceUI.gameObject);
        }

        public void removeFromSystem()
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

