using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;
using Chunks.Systems;
using Conduits;
using Item.Slot;
using UI;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineInstance : TileEntityInstance<CompactMachine>, IRightClickableTileEntity, IConduitPortTileEntity, IEnergyConduitInteractable, IItemConduitInteractable, ISignalConduitInteractable, ICompactMachine
    {
        private Vector2Int positionInSystem;
        private CompactMachinePortInventory inventory;
        private CompactMachineTeleporterInstance teleporter;
        public CompactMachinePortInventory Inventory { get => inventory; set => inventory = value; }
        public CompactMachineTeleporterInstance Teleporter { get => teleporter; set => teleporter = value; }
        public Vector2Int PositionInSystem { get => positionInSystem; set => positionInSystem = value; }

        public CompactMachineInstance(CompactMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            this.inventory = new CompactMachinePortInventory(this);
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return;
            }
            CompactMachineDimController dimController = compactMachineDimManager.GetCompactMachineDimController();
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            List<Vector2Int> path = new List<Vector2Int>();
            if (loadedChunk.getSystem() is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) {
                CompactMachineTeleportKey key = compactMachineClosedChunkSystem.getCompactMachineKey();
                foreach (Vector2Int vector in key.Path) {
                    path.Add(vector);
                }
            }
            path.Add(getCellPosition());
            CompactMachineTeleportKey thisKey = new CompactMachineTeleportKey(path);
            if (!dimController.HasSystem(thisKey)) {
                dimController.AddNewSystem(thisKey,this);
            }
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
                CompactMachineHelper.TeleportIntoCompactMachine(this);
                return;
            }
            GameObject uiPrefab = TileEntityObject.UIManager.getUIElement();
            if (uiPrefab == null) {
                Debug.LogError(getName() + " has uiprefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            CompactMachineUIController uIController = instantiated.GetComponent<CompactMachineUIController>();
            if (uIController == null) {
                Debug.LogError(getName() + "ui prefab doesn't have controller");
                return;
            }
            uIController.display(this);
            MainCanvasController.Instance.DisplayObject(instantiated);
        }

        public Vector2Int getTeleporterPosition() {
            if (teleporter == null) {
                Debug.LogError(getName() +  " Attempted to get teleporter position which was null");
                return Vector2Int.zero;
            }
            return teleporter.getCellPosition();
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return false;
        }

        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            
        }
        /*
        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                return inventory.ItemPorts[portPosition].ExtractSolidItem(portPosition);
            }
            return null;
        }

        public void InsertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                inventory.ItemPorts[portPosition].InsertSolidItem(itemSlot,portPosition);
            }
        }

        public ItemSlot ExtractFluidItem(Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                return inventory.FluidPorts[portPosition].ExtractFluidItem(portPosition);
            }
            return null;
        }

        public void InsertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                inventory.FluidPorts[portPosition].InsertFluidItem(itemSlot,portPosition);
            }
        }
        */
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
        public void addPort(ITileEntityInstance tileEntity, ConduitType type) {
            Vector2Int positionInCompactMachine = tileEntity.getCellPosition()-CompactMachineHelper.getPositionInNextRing(compactMachine.getCellPosition()); 
            Vector2Int positionOutsideCompactMachine = CompactMachineHelper.getPortPositionInLayout(positionInCompactMachine,compactMachine.TileEntityObject.ConduitPortLayout,type);
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

