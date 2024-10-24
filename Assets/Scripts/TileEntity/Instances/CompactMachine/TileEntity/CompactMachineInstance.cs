using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;
using Chunks.Systems;

namespace TileEntityModule.Instances.CompactMachines {
    public class CompactMachineInstance : TileEntityInstance<CompactMachine>, IRightClickableTileEntity, IConduitInteractable, IEnergyConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, ISignalConduitInteractable, ICompactMachine
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
            if (!dimController.hasSystem(thisKey)) {
                dimController.addNewSystem(thisKey,this);
            }
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitPortLayout;
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int insertEnergy(int energy,Vector2Int portPosition)
        {
            if (inventory.EnergyPorts.ContainsKey(portPosition)) {
                return inventory.EnergyPorts[portPosition].insertEnergy(energy,portPosition);
            }
            return 0;
        }

        public void onRightClick()
        {
            if (Input.GetKey(KeyCode.LeftShift)) {
                CompactMachineHelper.teleportIntoCompactMachine(this);
                return;
            }
            GameObject uiPrefab = tileEntity.UIManager.getUIElement();
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
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
        }

        public Vector2Int getTeleporterPosition() {
            if (teleporter == null) {
                Debug.LogError(getName() +  " Attempted to get teleporter position which was null");
                return Vector2Int.zero;
            }
            return teleporter.getCellPosition();
        }

        public int extractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public void insertSignal(int signal, Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                return inventory.ItemPorts[portPosition].extractSolidItem(portPosition);
            }
            return null;
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                inventory.ItemPorts[portPosition].insertSolidItem(itemSlot,portPosition);
            }
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                return inventory.FluidPorts[portPosition].extractFluidItem(portPosition);
            }
            return null;
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            if (inventory.ItemPorts.ContainsKey(portPosition)) {
                inventory.FluidPorts[portPosition].insertFluidItem(itemSlot,portPosition);
            }
        }
    }

    public class CompactMachinePortInventory {
        private CompactMachineInstance compactMachine;
        private Dictionary<Vector2Int, IEnergyConduitInteractable> energyPorts;
        private Dictionary<Vector2Int, ISolidItemConduitInteractable> itemPorts;
        private Dictionary<Vector2Int, IFluidConduitInteractable> fluidPorts;
        private Dictionary<Vector2Int, ISignalConduitInteractable> signalPorts;

        public CompactMachinePortInventory(CompactMachineInstance compactMachine) {
            this.compactMachine = compactMachine;
            this.energyPorts = new Dictionary<Vector2Int, IEnergyConduitInteractable>();
            this.itemPorts = new Dictionary<Vector2Int, ISolidItemConduitInteractable>();
            this.fluidPorts = new Dictionary<Vector2Int, IFluidConduitInteractable>();
            this.signalPorts = new Dictionary<Vector2Int, ISignalConduitInteractable>();
        }

        public Dictionary<Vector2Int, IEnergyConduitInteractable> EnergyPorts { get => energyPorts; set => energyPorts = value; }
        public Dictionary<Vector2Int, ISolidItemConduitInteractable> ItemPorts { get => itemPorts; set => itemPorts = value; }
        public Dictionary<Vector2Int, IFluidConduitInteractable> FluidPorts { get => fluidPorts; set => fluidPorts = value; }
        public Dictionary<Vector2Int, ISignalConduitInteractable> SignalPorts { get => signalPorts; set => signalPorts = value; }

        public void addPort(ITileEntityInstance tileEntity, ConduitType type) {
            Vector2Int positionInCompactMachine = tileEntity.getCellPosition()-CompactMachineHelper.getPositionInNextRing(compactMachine.getCellPosition()); 
            Vector2Int positionOutsideCompactMachine = CompactMachineHelper.getPortPositionInLayout(positionInCompactMachine,compactMachine.TileEntity.ConduitPortLayout,type);
            switch (type) {
                case ConduitType.Item:
                    if (itemPorts.ContainsKey(positionOutsideCompactMachine)) {
                        duplicateWarning(positionOutsideCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not ISolidItemConduitInteractable itemConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.getName() + " which is not item conduit interactable to compact machine '" + compactMachine.getName() + "'");
                        return;
                    }
                    itemPorts[positionOutsideCompactMachine] = itemConduitInteractable;
                    break;
                case ConduitType.Energy:
                    if (itemPorts.ContainsKey(positionOutsideCompactMachine)) {
                        duplicateWarning(positionOutsideCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.getName() + " which is not energy conduit interactable to compact machine '" + compactMachine.getName() + "'");
                        return;
                    }
                    energyPorts[positionOutsideCompactMachine] = energyConduitInteractable;
                    break;
                case ConduitType.Fluid:
                    if (itemPorts.ContainsKey(positionOutsideCompactMachine)) {
                        duplicateWarning(positionOutsideCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not IFluidConduitInteractable fluidConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.getName() + " which is not fluid conduit interactable to compact machine '" + compactMachine.getName() + "'");
                        return;
                    }
                    fluidPorts[positionOutsideCompactMachine] = fluidConduitInteractable;
                    break;
                case ConduitType.Signal:
                    if (itemPorts.ContainsKey(positionOutsideCompactMachine)) {
                        duplicateWarning(positionOutsideCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.getName() + " which is not signal conduit interactable to compact machine '" + compactMachine.getName() + "'");
                        return;
                    }
                    signalPorts[positionOutsideCompactMachine] = signalConduitInteractable;
                    break;
            }
        }

        private void duplicateWarning(Vector2Int key, ConduitType type) {
            Debug.LogWarning("Duplicate key " + key + " of type " + type + " for compact machine " + compactMachine.getName());
        }
    }
}

