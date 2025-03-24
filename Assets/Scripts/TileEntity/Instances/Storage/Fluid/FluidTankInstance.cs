using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;
using TileEntity.Instances.Storage.Fluid;


namespace TileEntity.Instances.Storage {
    public class FluidTankInstance : TileEntityInstance<FluidTank>, ISerializableTileEntity, IConduitPortTileEntity, ILoadableTileEntity, IItemConduitInteractable
    {
        private ItemSlot itemSlot;
        public uint FillAmount {get => itemSlot?.amount ?? 0;}
        public float FillRatio {get => ((float)FillAmount)/GetStorage();}
        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        private FluidTankVisualManager visualManager;

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

        public void Load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;   
            }
            GameObject fluid = new GameObject("Fluid");
            fluid.transform.SetParent(loadedChunk.GetTileEntityContainer(),false);
            visualManager = fluid.AddComponent<FluidTankVisualManager>();
            visualManager.Initialize();
            visualManager?.UpdateVisual(itemSlot,FillRatio,GetWorldPosition());
        }
        

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void Unload()
        {
            if (!visualManager) return;
            GameObject.Destroy(visualManager?.gameObject);
            visualManager = null;
        }

        public void Unserialize(string data)
        {
            this.itemSlot = ItemSlotFactory.DeserializeSlot(data);
        }
        
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            visualManager?.UpdateVisual(itemSlot,FillRatio,GetWorldPosition());
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
            visualManager?.UpdateVisual(itemSlot,FillRatio,GetWorldPosition());
        }
    }
}

