using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;


namespace TileEntity.Instances.Storage {
    public class FluidTankInstance : TileEntityInstance<FluidTank>, IRightClickableTileEntity, ISerializableTileEntity, IConduitPortTileEntity, ILoadableTileEntity, IItemConduitInteractable
    {
        private ItemSlot itemSlot;
        public uint FillAmount {get => itemSlot.amount;}
        public float FillRatio {get => ((float)FillAmount)/GetStorage();}
        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        private SpriteRenderer visualElement;

        public FluidTankInstance(FluidTank tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public uint GetStorage() {
            return TileEntityObject.Tier.GetFluidStorage();
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
            TileEntityObject.UIManager.display<FluidTankInstance,FluidTankUI>(this);
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
        
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            updateVisual();
            return itemSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.Copy(toInsert);
                toInsert.amount = 0;
                return;
            }
            if (!ItemSlotUtils.AreEqual(itemSlot,toInsert)) {
                return;
            }
            ItemSlotUtils.InsertIntoSlot(itemSlot,toInsert,GetStorage());
            updateVisual();
        }
    }
}

