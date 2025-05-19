using System;
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
using TileEntity.Instances.CompactMachines;
using TileEntity.Instances.Storage;
using UI;
using UnityEngine;
using World.Cave.Registry;
using Random = System.Random;

namespace TileEntity.Instances {
    public class VoidMinerInstance : TileEntityInstance<VoidMinerObject>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable, IBreakActionTileEntity, ITickableTileEntity, 
        IConduitPortTileEntity, IOnCaveRegistryLoadActionTileEntity, IItemConduitInteractable, IEnergyPortTileEntityAggregator, ICompactMachineInteractable
    {
        public bool DimensionStabilized;
        private const int OUTPUT_SIZE = 6;
        internal VoidMinerData MinerData;
        private CaveTileCollection caveTileCollection;
        private System.Random random;
        private ItemRegistry itemRegistry;
        private EnergyInventory EnergyInventory;
        public int CompactMachineDepth;
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
            SerializedMinerOutput SerializeOutputs(List<ItemSlot> outputs, bool outputActive)
            {
               return new SerializedMinerOutput { Outputs = ItemSlotFactory.serializeList(outputs), Active = outputActive };
            }
            SerializedVoidMinerData serializedVoidMinerData = new SerializedVoidMinerData
            {
                Energy = EnergyInventory.Energy,
                DriveData = ItemSlotFactory.seralizeItemSlot(MinerData.DriveSlot),
                ItemFilter = MinerData.ItemFilter,
                StoneOutputs = SerializeOutputs(MinerData.StoneOutputs,MinerData.StoneActive),
                OreOutputs = SerializeOutputs(MinerData.OreOutputs,MinerData.OreActive),
                FluidOutputs = SerializeOutputs(MinerData.FluidOutputs,MinerData.FluidActive)
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
                StoneOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.StoneOutputs.Outputs),
                StoneActive = serializedVoidMinerData.StoneOutputs.Active,
                OreOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.OreOutputs.Outputs),
                OreActive = serializedVoidMinerData.OreOutputs.Active,
                FluidOutputs = ItemSlotFactory.Deserialize(serializedVoidMinerData.FluidOutputs.Outputs),
                FluidActive = serializedVoidMinerData.FluidOutputs.Active,
            };
            EnergyInventory = new EnergyInventory(serializedVoidMinerData.Energy, 65365);
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
            // TODO GIVE THIS PROPER ENERGY STORAGE
            EnergyInventory = new EnergyInventory(0, 65365);
            CaveRegistry.Instance.AddMiner(this);
        }
        
        public void OnBreak()
        {
            CaveRegistry.Instance.RemoveMiner(this);
            
            if (chunk is not ILoadedChunk loadedChunk) return;
            
            Vector2 position = GetWorldPosition();
            if (MinerData.DriveSlot != null)
            {
                ItemEntityFactory.SpawnItemEntity(position, MinerData.DriveSlot, loadedChunk.GetEntityContainer());
            }

            if (MinerData.ItemFilter != null)
            {
                ItemObject filterItem = ItemRegistry.GetInstance().GetItemObject(ItemTileEntityPort.FILTER_ID);
                ItemSlot filterSlot = new ItemSlot(filterItem, 1, null);
                ItemSlotUtils.AddTag(filterSlot,ItemTag.ItemFilter,MinerData.ItemFilter);
                ItemEntityFactory.SpawnItemEntity(position, filterSlot, loadedChunk.GetEntityContainer());
            }
            ItemEntityFactory.SpawnItemEntities(position, MinerData.StoneOutputs, loadedChunk.GetEntityContainer());
            ItemEntityFactory.SpawnItemEntities(position, MinerData.FluidOutputs, loadedChunk.GetEntityContainer());
            ItemEntityFactory.SpawnItemEntities(position, MinerData.OreOutputs, loadedChunk.GetEntityContainer());
        }

        public void SetCaveTileCollectionFromDriveSlot(CaveRegistry caveRegistry)
        {
            ItemSlot itemSlot = MinerData.DriveSlot;
            caveRegistry.AddMiner(this);
            
            if (itemSlot?.tags?.Dict == null || !itemSlot.tags.Dict.TryGetValue(ItemTag.CaveData, out object caveData))
            {
                caveTileCollection = null;
                return;
            }
            if (caveData is not string caveId) return;
            caveId = caveId.ToLower();
            caveTileCollection = caveRegistry.GetCaveTileCollection(caveId);
            random = new Random();
            itemRegistry = ItemRegistry.GetInstance();
        }
        
        
        
        public void TickUpdate()
        {
            if (!DimensionStabilized || caveTileCollection == null) return;
            float randomFloat = (float)random.NextDouble();
            string id = caveTileCollection.GetId(randomFloat);
            if (!MinerData.ItemFilter?.Filter(id) ?? false) return;
            
            ItemObject itemObject = itemRegistry.GetItemObject(id);
            if (!itemObject) return;
            List<ItemSlot> inputInventory;
            uint maxSize;
            uint insertAmount;
            switch (itemObject)
            {
                case TileItem:
                    if (!MinerData.StoneActive) return;
                    inputInventory = MinerData.StoneOutputs;
                    maxSize = Global.MAX_SIZE;
                    insertAmount = 1;
                    break;
                case FluidTileItem:
                    if (!MinerData.FluidActive) return;
                    inputInventory = MinerData.FluidOutputs;
                    maxSize = 256000;
                    insertAmount = 1000;
                    break;
                default:
                    if (!MinerData.OreActive) return;
                    inputInventory = MinerData.OreOutputs;
                    maxSize = Global.MAX_SIZE;
                    insertAmount = 1;
                    break;
            }

            ItemSlotUtils.InsertOneIdInventory(inputInventory, id, maxSize,insertAmount);
        }
        
        internal class VoidMinerData
        {
            public ItemSlot DriveSlot;
            public ItemFilter ItemFilter;
            public List<ItemSlot> StoneOutputs;
            public bool StoneActive;
            public List<ItemSlot> OreOutputs;
            public bool OreActive;
            public List<ItemSlot> FluidOutputs;
            public bool FluidActive;
        }
        
        

        private class SerializedVoidMinerData
        {
            public ulong Energy;
            public string DriveData;
            public ItemFilter ItemFilter;
            public SerializedMinerOutput StoneOutputs;
            public SerializedMinerOutput OreOutputs;
            public SerializedMinerOutput FluidOutputs;
        }

        private class SerializedMinerOutput
        {
            public string Outputs;
            public bool Active;
        }


        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public void OnCaveRegistryLoaded(CaveRegistry caveRegistry)
        {
            SetCaveTileCollectionFromDriveSlot(caveRegistry);
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            switch (state)
            {
                case ItemState.Solid:
                    if (portPosition.x == 0)
                    {
                        return ItemSlotUtils.ExtractFromInventory(MinerData.OreOutputs);
                    }
                    return ItemSlotUtils.ExtractFromInventory(MinerData.StoneOutputs);
                case ItemState.Fluid:
                    return ItemSlotUtils.ExtractFromInventory(MinerData.FluidOutputs);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            
        }
        
        public IEnergyConduitInteractable GetEnergyConduitInteractable()
        {
            return EnergyInventory;
        }

        public void SyncToCompactMachine(CompactMachineInstance compactMachine)
        {
            CompactMachineDepth = compactMachine.Depth;
        }
    }
    
}

