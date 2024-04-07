using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;
using Newtonsoft.Json;
using GUIModule;

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

        private List<ItemSlot> upgrades;
        private List<ItemSlot> recipes;


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
                priority = 0;
                upgrades = new List<ItemSlot>{
                    null
                };
                recipes = new List<ItemSlot>(); 
                for (int i = 0; i < 9; i++) {
                    recipes.Add(null);
                }  

                return;
            }
            SeralizedMatrixInterface seralizedMatrixInterface = JsonConvert.DeserializeObject<SeralizedMatrixInterface>(data);
            priority = seralizedMatrixInterface.priority;
            upgrades = ItemSlotFactory.deserialize(seralizedMatrixInterface.upgrades);
            recipes = ItemSlotFactory.deserialize(seralizedMatrixInterface.recipes);
        }

        public void onRightClick()
        {
            MatrixInterfaceUI matrixInterfaceUI = MatrixInterfaceUI.newInstance();
            matrixInterfaceUI.init(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(matrixInterfaceUI.gameObject);
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

