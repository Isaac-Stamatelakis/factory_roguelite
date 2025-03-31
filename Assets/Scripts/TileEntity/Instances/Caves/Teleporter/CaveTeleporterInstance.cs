using System.Collections;
using System.Collections.Generic;
using Chunks;
using Entities;
using Item.Slot;
using Newtonsoft.Json;
using UI;
using UnityEngine;

namespace TileEntity.Instances {
    public class CaveTeleporterInstance : TileEntityInstance<CaveTeleporter>, ISerializableTileEntity, IPlaceInitializable, IBreakActionTileEntity, ITickableTileEntity
    {
        public float Delay;
        public List<ItemSlot> CaveStorageDrives;
        private const int INVENTORY_SIZE = 10;
        public const float TELEPORT_DELAY = 60f;
        public CaveTeleporterInstance(CaveTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        

        public string Serialize()
        {
            SerializedCaveTeleporterData serializedCaveTeleporterData = new SerializedCaveTeleporterData
            {
                Delay = Delay,
                Items = ItemSlotFactory.serializeList(CaveStorageDrives)
            };
            return JsonConvert.SerializeObject(serializedCaveTeleporterData);
        }

        public void Unserialize(string data)
        {
            SerializedCaveTeleporterData serializedCaveTeleporterData = JsonConvert.DeserializeObject<SerializedCaveTeleporterData>(data);
            Delay = serializedCaveTeleporterData.Delay;
            CaveStorageDrives = ItemSlotFactory.Deserialize(serializedCaveTeleporterData.Items);
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

        private class SerializedCaveTeleporterData
        {
            public string Items;
            public float Delay;
        }

        public void TickUpdate()
        {
            Delay -= Time.fixedDeltaTime; // TODO change this if tick rate changes
        }
    }
}

