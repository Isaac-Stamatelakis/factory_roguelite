using System;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Items;
using Player;
using TileEntity.Instances.Storage.Fluid;
using UI;
using UnityEngine;

namespace TileEntity.Instances.Creative.CreativeChest
{
    [CreateAssetMenu(fileName = "New Creative Tank", menuName = "Tile Entity/Creative/Tank")]
    public class CreativeTankObject : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CreativeTankInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class CreativeTankInstance : TileEntityInstance<CreativeTankObject>, IItemConduitInteractable, ISerializableTileEntity, IRightClickableTileEntity, IConduitPortTileEntity, ILoadableTileEntity
    {
        public ItemSlot ItemSlot;
        private FluidTankVisualManager visualManager;
        public CreativeTankInstance(CreativeTankObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            if (state != ItemState.Fluid) return null;
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

        public void OnRightClick()
        {
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            ItemSlot selectedSlot = playerScript.PlayerInventory.getSelectedItemSlot();
            if (selectedSlot?.itemObject is not FluidTileItem fluidTileItem) return;
            ItemSlot = new ItemSlot(fluidTileItem, uint.MaxValue, null);
            visualManager?.UpdateVisual(ItemSlot,1f,GetWorldPosition());
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public void Load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;   
            }
            GameObject fluid = new GameObject();
            visualManager = fluid.AddComponent<FluidTankVisualManager>();
            fluid.name = "Fluid";
            fluid.transform.SetParent(loadedChunk.GetTileEntityContainer(),false);
            visualManager.Initialize();
            visualManager.UpdateVisual(ItemSlot,1f,GetWorldPosition());
        }

        public void Unload()
        {
            if (!visualManager) return;
            GameObject.Destroy(visualManager.gameObject);
            visualManager = null;
        }
    }
}
