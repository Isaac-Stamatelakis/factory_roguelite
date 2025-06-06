using System;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Creative.CreativeChest
{
    [CreateAssetMenu(fileName = "New Creative Chest", menuName = "Tile Entity/Creative/Chest")]
    public class CreativeChest : TileEntityObject, IUITileEntity
    {
        public AssetReference AssetReference;
        public ConduitPortLayout ConduitPortLayout;
        public CreativeChestUI CreativeChestUIPrefab;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CreativeChestInstance(this, tilePosition, tileItem, chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }

    public class CreativeChestInstance : TileEntityInstance<CreativeChest>, IItemConduitInteractable, ISerializableTileEntity, IConduitPortTileEntity
    {
        public ItemSlot ItemSlot;
        public CreativeChestInstance(CreativeChest tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            if (ItemSlot == null) return null;
            ItemSlot.amount = UInt32.MaxValue;
            return ItemSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            
        }

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(ItemSlot);
        }

        public void Unserialize(string data)
        {
            ItemSlot = ItemSlotFactory.DeserializeSlot(data);
        }

        

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }
    }
}
