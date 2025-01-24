using System.Collections;
using System.Collections.Generic;
using Chunks;
using Item.Slot;
using UI;
using UnityEngine;

namespace TileEntity.Instances {
    public class CaveTeleporterInstance : TileEntityInstance<CaveTeleporter>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable
    {
        public List<ItemSlot> CaveStorageDrives;
        private const int INVENTORY_SIZE = 10;
        public CaveTeleporterInstance(CaveTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onRightClick()
        {
            CaveTeleporterUIController caveTeleporterUIController = GameObject.Instantiate(TileEntityObject.uIManager.getUIElement()).GetComponent<CaveTeleporterUIController>();
            caveTeleporterUIController.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(caveTeleporterUIController.gameObject);
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(CaveStorageDrives);
        }

        public void unserialize(string data)
        {
            CaveStorageDrives = ItemSlotFactory.Deserialize(data);
            ItemSlotFactory.ClampList(CaveStorageDrives, INVENTORY_SIZE);
        }

        public void PlaceInitialize()
        {
            CaveStorageDrives = ItemSlotFactory.createEmptyInventory(INVENTORY_SIZE);
        }
    }
}

