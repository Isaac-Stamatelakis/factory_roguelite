using System.Collections;
using System.Collections.Generic;
using ChunkModule;
using ConduitModule.Ports;
using UnityEngine;
using UnityEngine.Tilemaps;
using GUIModule;
using UI;
using ItemModule;


namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Fluid Tank",menuName="Tile Entity/Storage/Fluid/Standard")]
    public class FluidTank : TileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ILoadableTileEntity, IFluidConduitInteractable
    {
        [SerializeField] public Tier tier;
        [SerializeField] public ConduitPortLayout layout;
        private ItemSlot itemSlot;
        public int FillAmount {get => itemSlot.amount;}
        public float FillRatio {get => FillAmount/getStorage();}
        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        private SpriteRenderer visualElement;

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public int getStorage() {
            return tier.getFluidStorage();
        }

        public void load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;   
            }
            GameObject fluid = new GameObject();
            fluid.name = "Fluid";
            fluid.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            fluid.transform.position = getWorldPosition();
            visualElement = fluid.AddComponent<SpriteRenderer>();
            updateVisual();
        }

        private void updateVisual() {
            if (visualElement == null) {
                return;
            }
            if (itemSlot.itemObject == null) {
                visualElement.gameObject.SetActive(false);
                return;
            }
            visualElement.gameObject.SetActive(true);
            visualElement.sprite = itemSlot.itemObject.getSprite();
            visualElement.transform.localScale = ItemSlotUIFactory.getItemScale(visualElement.sprite);
        }

        public void onClick()
        {
            FluidTankUI fluidTankUI = FluidTankUI.newInstance();
            fluidTankUI.init(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(fluidTankUI.gameObject);
        }

        public string serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void unload()
        {
            GameObject.Destroy(visualElement.gameObject);
        }

        public void unserialize(string data)
        {
            this.itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
        }

        public ItemSlot extractItem(Vector2Int portPosition)
        {
            updateVisual();
            return itemSlot;
        }

        public void insertItem(ItemSlot toInsert, Vector2Int portPosition)
        {
            Debug.Log(toInsert.itemObject.name);
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.copy(toInsert);
                toInsert.amount = 0;
                return;
            }
            if (!ItemSlotHelper.areEqual(itemSlot,toInsert)) {
                return;
            }
            ItemSlotHelper.combineItems(itemSlot,toInsert);
            updateVisual();
        }
    }
}

