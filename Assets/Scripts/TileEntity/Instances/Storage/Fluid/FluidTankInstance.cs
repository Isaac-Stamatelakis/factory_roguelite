using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;


namespace TileEntityModule.Instances.Storage {
    public class FluidTankInstance : TileEntityInstance<FluidTank>, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ILoadableTileEntity, IFluidConduitInteractable
    {
        private ItemSlot itemSlot;
        public int FillAmount {get => itemSlot.amount;}
        public float FillRatio {get => ((float)FillAmount)/getStorage();}
        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        private SpriteRenderer visualElement;

        public FluidTankInstance(FluidTank tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public int getStorage() {
            return tileEntity.Tier.getFluidStorage();
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
            if (itemSlot == null || itemSlot.itemObject == null) {
                visualElement.gameObject.SetActive(false);
                return;
            }
            visualElement.gameObject.SetActive(true);
            visualElement.sprite = itemSlot.itemObject.getSprite();
            float height = FillRatio*0.5f;
            visualElement.transform.localScale = new Vector3(0.5f,height,1);
            Vector2 position = getWorldPosition();
            visualElement.transform.position = new Vector3(position.x,position.y-0.25f+height/2,1.5f);

        }

        public void onRightClick()
        {
            tileEntity.UIManager.display<FluidTankInstance,FluidTankUI>(this);
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

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            updateVisual();
            return itemSlot;
        }

        public void insertFluidItem(ItemSlot toInsert, Vector2Int portPosition)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.copy(toInsert);
                toInsert.amount = 0;
                return;
            }
            if (!ItemSlotHelper.areEqual(itemSlot,toInsert)) {
                return;
            }
            ItemSlotHelper.insertIntoSlot(itemSlot,toInsert,getStorage());
            updateVisual();
        }
    }
}

