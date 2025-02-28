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
        private Vector2Int positionInSystem;
        private CompactMachinePortInventory inventory;
        private CompactMachineTeleporterInstance teleporter;
        public CompactMachinePortInventory Inventory { get => inventory; set => inventory = value; }
        public CompactMachineTeleporterInstance Teleporter { get => teleporter; set => teleporter = value; }
        public Vector2Int PositionInSystem { get => positionInSystem; set => positionInSystem = value; }
        private string hash;
        public string Hash => hash;

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
            path.Add(getCellPosition());
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
        public string Serialize(SerializationMode mode)
        {
            return hash;
        }

        public void Unserialize(string data)
        {
            hash = data;
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
            ItemSlotUtils.AddTag(itemSlot,ItemTag.CompactMachine,Serialize(SerializationMode.Standard));
            ItemEntityFactory.SpawnItemEntity(getWorldPosition(), itemSlot, loadedChunk.getEntityContainer());

            CompactMachineTeleportKey key = GetTeleportKey();
            compactMachineDimManager.GetCompactMachineDimController().RemoveCompactMachineSystem(key, hash);
        }
        

        public void PlaceInitializeWithHash(string newHash)
        {
            bool hashNull = newHash == null;
            this.hash = newHash;
            if (hashNull)
            {
                hash = CompactMachineUtils.GenerateHash();
                CompactMachineUtils.InitializeHashFolder(Serialize(SerializationMode.Standard));
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
                    dimController.AddNewSystem(thisKey,this, hash);
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
                Debug.LogWarning("Duplicate key " + position + " of type " + type + " for compact machine " + compactMachine.getName());
                return;
            }
            ports[position] = (T)interactable;
        }
        public void addPort(ITileEntityInstance tileEntity, ConduitType type)
        {
            Vector2Int positionInCompactMachine = tileEntity.getCellPosition(); 
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

