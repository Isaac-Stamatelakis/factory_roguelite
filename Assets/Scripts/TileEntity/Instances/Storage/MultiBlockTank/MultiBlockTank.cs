using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using UnityEngine;

namespace TileEntity.Instances.Storage.MultiBlockTank
{
    [CreateAssetMenu(fileName = "New MultiBlockTank", menuName = "Tile Entity/Storage/Fluid/MultiBlock")]
    public class MultiBlockTank : TileEntityObject
    {
        public TileItem TankTile;
        public uint SpacePerTank;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MultiBlockTankInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class MultiBlockTankInstance : TileEntityInstance<MultiBlockTank>, IMultiBlockTileEntity, IItemConduitInteractable, ISerializableTileEntity, ILoadableTileEntity
    {
        private ItemSlot fluidSlot;
        private List<Vector2Int> tiles;
        public MultiBlockTankInstance(MultiBlockTank tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void AssembleMultiBlock()
        {
            tiles = TileEntityUtils.BFSTile(this, tileEntityObject.TankTile);
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            if (state != ItemState.Fluid) return null;
            return fluidSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (state != ItemState.Fluid) return;
            
            if (ItemSlotUtils.IsItemSlotNull(fluidSlot))
            {
                fluidSlot = new ItemSlot(toInsert.itemObject, toInsert.amount, null);
                toInsert.amount = 0;
                return;
            }
            
            if (!ItemSlotUtils.AreEqual(fluidSlot, toInsert)) return;
            uint size = (uint)tiles.Count;
            uint space = size * tileEntityObject.SpacePerTank;
            ItemSlotUtils.InsertIntoSlot(fluidSlot,toInsert,space);
        }

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(fluidSlot);
        }

        public void Unserialize(string data)
        {
            fluidSlot = ItemSlotFactory.DeserializeSlot(data);
        }

        public void Load()
        {
            
        }

        public void Unload()
        {
            
        }
    }
}
