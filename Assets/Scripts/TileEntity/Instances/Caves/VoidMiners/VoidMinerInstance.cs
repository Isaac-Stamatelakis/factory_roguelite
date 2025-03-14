using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Entities;
using Item.Slot;
using Items;
using Items.Tags;
using LibNoise.Operator;
using Newtonsoft.Json;
using TileEntity.Instances.Caves.VoidMiners;
using UI;
using UnityEngine;
using World.Cave.Registry;
using Random = System.Random;

namespace TileEntity.Instances {
    public class VoidMinerInstance : TileEntityInstance<VoidMinerObject>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable, IBreakActionTileEntity, ITickableTileEntity, IConduitPortTileEntity, IOnCaveRegistryLoadActionTileEntity
    {
        private const int OUTPUT_SIZE = 6;
        internal VoidMinerData MinerData;
        private CaveTileCollection caveTileCollection;
        private System.Random random;
        private ItemRegistry itemRegistry;
        public VoidMinerInstance(VoidMinerObject tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            VoidMinerUI voidMinerUI = GameObject.Instantiate(tileEntityObject.VoidMinerUI);
            voidMinerUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(voidMinerUI.gameObject);
        }

        public string Serialize()
        {
            SerializedVoidMinerData serializedVoidMinerData = new SerializedVoidMinerData
            {
                DriveData = ItemSlotFactory.seralizeItemSlot(MinerData.DriveSlot),
                ItemFilter = MinerData.ItemFilter,
                StoneOutputs = ItemSlotFactory.serializeList(MinerData.StoneOutputs),
                OreOutputs = ItemSlotFactory.serializeList(MinerData.OreOutputs),
                FluidOutputs = ItemSlotFactory.serializeList(MinerData.FluidOutputs),
            };
            return JsonConvert.SerializeObject(serializedVoidMinerData);
        }

        public void Unserialize(string data)
        {
            SerializedVoidMinerData serializedVoidMinerData = JsonConvert.DeserializeObject<SerializedVoidMinerData>(data);
            MinerData = new VoidMinerData
            {
                DriveSlot = ItemSlotFactory.DeserializeSlot(serializedVoidMinerData.DriveData),
                ItemFilter = serializedVoidMinerData.ItemFilter,
                StoneOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.StoneOutputs),
                OreOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.OreOutputs),
                FluidOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.FluidOutputs),
            };
        }

        public void PlaceInitialize()
        {
            MinerData = new VoidMinerData
            {
                DriveSlot = null,
                ItemFilter = null,
                StoneOutputs = ItemSlotFactory.createEmptyInventory(OUTPUT_SIZE),
                OreOutputs = ItemSlotFactory.createEmptyInventory(OUTPUT_SIZE),
                FluidOutputs = ItemSlotFactory.createEmptyInventory(OUTPUT_SIZE),
            };
        }
        
        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) return;
            Vector2 position = GetWorldPosition();
            if (MinerData.DriveSlot != null)
            {
                ItemEntityFactory.SpawnItemEntity(position, MinerData.DriveSlot, loadedChunk.getEntityContainer());
            }

            if (MinerData.ItemFilter != null)
            {
                ItemObject filterItem = ItemRegistry.GetInstance().GetItemObject(ItemTileEntityPort.FILTER_ID);
                ItemSlot filterSlot = new ItemSlot(filterItem, 1, null);
                ItemSlotUtils.AddTag(filterSlot,ItemTag.ItemFilter,MinerData.ItemFilter);
                ItemEntityFactory.SpawnItemEntity(position, filterSlot, loadedChunk.getEntityContainer());
            }
            ItemEntityFactory.SpawnItemEntities(position, MinerData.StoneOutputs, loadedChunk.getEntityContainer());
            ItemEntityFactory.SpawnItemEntities(position, MinerData.FluidOutputs, loadedChunk.getEntityContainer());
            ItemEntityFactory.SpawnItemEntities(position, MinerData.OreOutputs, loadedChunk.getEntityContainer());
        }

        public void SetCaveTileCollectionFromDriveSlot(CaveRegistry caveRegistry)
        {
            ItemSlot itemSlot = MinerData.DriveSlot;
            if (itemSlot?.tags?.Dict == null || !itemSlot.tags.Dict.TryGetValue(ItemTag.CaveData, out object caveData)) return;
            if (caveData is not string caveId) return;
            caveId = caveId.ToLower();
            caveTileCollection = caveRegistry.GetCaveTileCollection(caveId);
            random = new Random();
            itemRegistry = ItemRegistry.GetInstance();
        }
        
        
        
        public void TickUpdate()
        {
            if (caveTileCollection == null) return;
            float randomFloat = (float)random.NextDouble();
            string id = caveTileCollection.GetId(randomFloat);
            ItemObject itemObject = itemRegistry.GetItemObject(id);
            if (!itemObject) return;
            List<ItemSlot> inputInventory;
            uint maxSize;
            uint insertAmount;
            switch (itemObject)
            {
                case TileItem:
                    inputInventory = MinerData.StoneOutputs;
                    maxSize = Global.MAX_SIZE;
                    insertAmount = 1;
                    break;
                case FluidTileItem:
                    inputInventory = MinerData.FluidOutputs;
                    maxSize = Global.MAX_SIZE;
                    insertAmount = 1;
                    break;
                default:
                    inputInventory = MinerData.OreOutputs;
                    maxSize = 256000;
                    insertAmount = 1000;
                    break;
            }

            ItemSlotUtils.InsertOneIdInventory(inputInventory, id, maxSize,insertAmount);
        }

        internal class VoidMinerData
        {
            public ItemSlot DriveSlot;
            public ItemFilter ItemFilter;
            public List<ItemSlot> StoneOutputs;
            public List<ItemSlot> OreOutputs;
            public List<ItemSlot> FluidOutputs;
        }

        private class SerializedVoidMinerData
        {
            public string DriveData;
            public ItemFilter ItemFilter;
            public string StoneOutputs;
            public string OreOutputs;
            public string FluidOutputs;
        }


        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public void OnCaveRegistryLoaded(CaveRegistry caveRegistry)
        {
            SetCaveTileCollectionFromDriveSlot(caveRegistry);
        }
    }
    
}

