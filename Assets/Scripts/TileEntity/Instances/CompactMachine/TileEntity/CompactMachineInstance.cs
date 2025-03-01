using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;
using Chunks.Systems;
using Conduits;
using Entities;
using Item.Slot;
using Items;
using Items.Tags;
using Newtonsoft.Json;
using TileEntity.Instances.CompactMachine;
using UI;
using WorldModule;

namespace TileEntity.Instances.CompactMachines {
    public interface ITagPlacementTileEntity
    {
        public ItemTag GetItemTag();
    }
    public class CompactMachineInstance : TileEntityInstance<CompactMachine>, 
        IRightClickableTileEntity, IConduitPortTileEntity, IEnergyConduitInteractable, IItemConduitInteractable, ISignalConduitInteractable, ICompactMachine, 
        IBreakActionTileEntity, ISerializableTileEntity
    {
        private CompactMachinePortInventory inventory;
        private CompactMachineTeleporterInstance teleporter;
        public CompactMachinePortInventory Inventory { get => inventory; set => inventory = value; }
        public CompactMachineTeleporterInstance Teleporter { get => teleporter; set => teleporter = value; }
        public string Hash { get => compactMachineData.Hash; set =>  compactMachineData.Hash = value; }
        public bool IsActive => compactMachineData.Active;
        private CompactMachineData compactMachineData;

        public CompactMachineInstance(CompactMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            this.inventory = new CompactMachinePortInventory(this);
        }

        public CompactMachineTeleportKey GetTeleportKey()
        {
            if (chunk is not ILoadedChunk loadedChunk) return null;
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return null;
            }
            List<Vector2Int> path = new List<Vector2Int>();
            if (loadedChunk.getSystem() is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) {
                CompactMachineTeleportKey key = compactMachineClosedChunkSystem.GetCompactMachineKey();
                foreach (Vector2Int vector in key.Path) {
                    path.Add(vector);
                }
            }
            path.Add(GetCellPosition());
            bool locked = compactMachineDimManager.GetCompactMachineDimController().IsLocked(path);
            return new CompactMachineTeleportKey(path,locked);
        }
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            if (inventory.EnergyPorts.ContainsKey(portPosition)) {
                return inventory.EnergyPorts[portPosition].InsertEnergy(energy,portPosition);
            }
            return 0;
        }

        public void OnRightClick()
        {
            if (Input.GetKey(KeyCode.LeftShift)) {
                CompactMachineUtils.TeleportIntoCompactMachine(this);
                return;
            }
            TileEntityObject.UIManager.Display<CompactMachineInstance,CompactMachineUIController>(this);
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return false;
        }

        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            
        }
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            switch (state)
            {
                case ItemState.Solid:
                    if (!inventory.ItemPorts.ContainsKey(portPosition)) return null;
                    return inventory.ItemPorts[portPosition].ExtractItem(state,portPosition,filter);
                case ItemState.Fluid:
                    if (!inventory.FluidPorts.ContainsKey(portPosition)) return null;
                    return inventory.FluidPorts[portPosition].ExtractItem(state,portPosition,filter);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(compactMachineData);
        }

        public void Unserialize(string data)
        {
            compactMachineData = JsonConvert.DeserializeObject<CompactMachineData>(data);
        }

        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) return;
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return;
            }
            // Drops itself with hash
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(tileItem?.id);
            ItemSlot itemSlot = new ItemSlot(itemObject, 1, null);
            ItemSlotUtils.AddTag(itemSlot,ItemTag.CompactMachine,compactMachineData.Hash);
            ItemEntityFactory.SpawnItemEntity(GetWorldPosition(), itemSlot, loadedChunk.getEntityContainer());

            CompactMachineTeleportKey key = GetTeleportKey();
            compactMachineDimManager.GetCompactMachineDimController().RemoveCompactMachineSystem(key, compactMachineData.Hash);
        }

        public bool IsParentLocked()
        {
            CompactMachineTeleportKey key = GetTeleportKey();
            if (key.Path.Count <= 1) return false;
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                return false;
            }
            key.Path.RemoveAt(key.Path.Count - 1);
            return compactMachineDimManager.GetCompactMachineDimController().IsLocked(key.Path);
        }
        

        public void PlaceInitializeWithHash(string newHash)
        {
            compactMachineData.Active = true;
            bool hashNull = newHash == null;
            if (hashNull)
            {
                compactMachineData = new CompactMachineData(true, CompactMachineUtils.GenerateHash());
                CompactMachineUtils.InitializeHashFolder(compactMachineData.Hash, tileItem?.id);
            }
            else
            {
                compactMachineData.Hash = newHash;
            }
            
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return;
            }
            CompactMachineDimController dimController = compactMachineDimManager.GetCompactMachineDimController();
            
            CompactMachineTeleportKey thisKey = GetTeleportKey();
            if (thisKey == null)
            {
                Debug.LogError("Tried to load compact machine with null key");
                return;
            }
            if (!dimController.HasSystem(thisKey)) {
                if (hashNull)
                {
                    dimController.AddNewSystem(thisKey,this, null);
                }
                else
                {
                    dimController.AddNewSystem(thisKey,this, compactMachineData.Hash);
                }
                
            }
        }

        public int GetSubSystems()
        {
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return 0;
            }

            return compactMachineDimManager.GetCompactMachineDimController().GetSubSystems(GetTeleportKey());
        }

        private struct CompactMachineData
        {
            public bool Active;
            public string Hash;

            public CompactMachineData(bool active, string hash)
            {
                Active = active;
                Hash = hash;
            }
        }

        public void SetActive(bool active)
        {
            if (active == compactMachineData.Active) return;
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                return;
            }

            compactMachineData.Active = active;
            CompactMachineTeleportKey key = GetTeleportKey();
            if (active)
            {
                compactMachineDimManager.GetCompactMachineDimController().AddNewSystem(key, this, compactMachineData.Hash);
            }
            else
            {
                compactMachineDimManager.GetCompactMachineDimController().RemoveCompactMachineSystem(key, compactMachineData.Hash);
            }
            
        }
    }

    public class CompactMachinePortInventory {
        private CompactMachineInstance compactMachine;
        private Dictionary<Vector2Int, IEnergyConduitInteractable> energyPorts;
        private Dictionary<Vector2Int, IItemConduitInteractable> itemPorts;
        private Dictionary<Vector2Int, IItemConduitInteractable> fluidPorts;
        private Dictionary<Vector2Int, ISignalConduitInteractable> signalPorts;

        public CompactMachinePortInventory(CompactMachineInstance compactMachine) {
            this.compactMachine = compactMachine;
            this.energyPorts = new Dictionary<Vector2Int, IEnergyConduitInteractable>();
            this.itemPorts = new Dictionary<Vector2Int, IItemConduitInteractable>();
            this.fluidPorts = new Dictionary<Vector2Int, IItemConduitInteractable>();
            this.signalPorts = new Dictionary<Vector2Int, ISignalConduitInteractable>();
        }

        public Dictionary<Vector2Int, IEnergyConduitInteractable> EnergyPorts { get => energyPorts; set => energyPorts = value; }
        public Dictionary<Vector2Int, IItemConduitInteractable> ItemPorts { get => itemPorts; set => itemPorts = value; }
        public Dictionary<Vector2Int, IItemConduitInteractable> FluidPorts { get => fluidPorts; set => fluidPorts = value; }
        public Dictionary<Vector2Int, ISignalConduitInteractable> SignalPorts { get => signalPorts; set => signalPorts = value; }

        private void SetPortInteractable<T>(Dictionary<Vector2Int, T> ports, IConduitInteractable interactable, Vector2Int position, ConduitType type) where T : IConduitInteractable
        {
            if (ports.ContainsKey(position)) {
                Debug.LogWarning("Duplicate key " + position + " of type " + type + " for compact machine " + compactMachine.GetName());
                return;
            }
            ports[position] = (T)interactable;
        }
        public void addPort(ITileEntityInstance tileEntity, ConduitType type)
        {
            Vector2Int positionInCompactMachine = tileEntity.GetCellPosition(); 
            Vector2Int positionOutsideCompactMachine = CompactMachineUtils.GetPortPositionInLayout(positionInCompactMachine,compactMachine.TileEntityObject.ConduitPortLayout,type);
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntity, type);
            if (interactable == null) return;
            switch (type) {
                case ConduitType.Item:
                    SetPortInteractable(ItemPorts,interactable,positionOutsideCompactMachine,type);
                    break;
                case ConduitType.Energy:
                    SetPortInteractable(EnergyPorts,interactable,positionOutsideCompactMachine,type);
                    break;
                case ConduitType.Fluid:
                    SetPortInteractable(FluidPorts,interactable,positionOutsideCompactMachine,type);
                    break;
                case ConduitType.Signal:
                    SetPortInteractable(SignalPorts,interactable,positionOutsideCompactMachine,type);
                    break;
            }
        }
    }
}

