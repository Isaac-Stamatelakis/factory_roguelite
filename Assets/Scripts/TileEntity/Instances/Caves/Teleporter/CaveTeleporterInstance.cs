using System.Collections;
using System.Collections.Generic;
using Chunks;
using Entities;
using Item.Slot;
using UI;
using UnityEngine;

namespace TileEntity.Instances {
    public class CaveTeleporterInstance : TileEntityInstance<CaveTeleporter>, ISerializableTileEntity, IPlaceInitializable, IBreakActionTileEntity
    {
        public List<ItemSlot> CaveStorageDrives;
        private const int INVENTORY_SIZE = 10;
        public CaveTeleporterInstance(CaveTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        

        public string Serialize()
        {
            return ItemSlotFactory.serializeList(CaveStorageDrives);
        }

        public void Unserialize(string data)
        {
            CaveStorageDrives = ItemSlotFactory.Deserialize(data);
            ItemSlotFactory.ClampList(CaveStorageDrives, INVENTORY_SIZE);
        }

        public void PlaceInitialize()
        {
            CaveStorageDrives = ItemSlotFactory.createEmptyInventory(INVENTORY_SIZE);
        }
        
        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) return;
            Vector2 position = GetWorldPosition();
            foreach (ItemSlot itemSlot in CaveStorageDrives)
            {
                ItemEntityFactory.SpawnItemEntity(position, itemSlot, loadedChunk.GetEntityContainer());
            }
        }
    }
}

