using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Compact Machine")]
    public class CompactMachine : TileEntity, IRightClickableTileEntity, IConduitInteractable, IEnergyConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, ISignalConduitInteractable, ICompactMachine
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        [SerializeField] public GameObject tilemapContainer;
        [SerializeField] public GameObject uiPrefab;
        private CompactMachinePortInventory inventory;
        private CompactMachineTeleporter teleporter;

        public CompactMachinePortInventory Inventory { get => inventory; set => inventory = value; }
        public CompactMachineTeleporter Teleporter { get => teleporter; set => teleporter = value; }


        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }


        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
            this.inventory = new CompactMachinePortInventory(this);
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in with dimension manager");
                return;
            }
            CompactMachineDimController dimController = compactMachineDimManager.GetCompactMachineDimController();
            dimController.activateSystem(this);
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
            if (uiPrefab == null) {
                Debug.LogError(name + " has uiprefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            CompactMachineUIController uIController = instantiated.GetComponent<CompactMachineUIController>();
            if (uIController == null) {
                Debug.LogError(name + "ui prefab doesn't have controller");
                return;
            }
            uIController.display(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
        }

        public Vector2Int getTeleporterPosition() {
            if (teleporter == null) {
                Debug.LogError(name +  " Attempted to get teleporter position which was null");
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
        private CompactMachine compactMachine;
        private Dictionary<Vector2Int, IEnergyConduitInteractable> energyPorts;
        private Dictionary<Vector2Int, ISolidItemConduitInteractable> itemPorts;
        private Dictionary<Vector2Int, IFluidConduitInteractable> fluidPorts;
        private Dictionary<Vector2Int, ISignalConduitInteractable> signalPorts;

        public CompactMachinePortInventory(CompactMachine compactMachine) {
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

        public void addPort(TileEntity tileEntity, ConduitType type) {
            Vector2Int positionInCompactMachine = tileEntity.getCellPosition()-CompactMachineHelper.getPositionInNextRing(compactMachine.getCellPosition()); 
            Vector2Int positionOutsideCompactMachine = CompactMachineHelper.getPortPositionInLayout(positionInCompactMachine,compactMachine.conduitPortLayout,type);
            switch (type) {
                case ConduitType.Item:
                    if (itemPorts.ContainsKey(positionOutsideCompactMachine)) {
                        duplicateWarning(positionOutsideCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not ISolidItemConduitInteractable itemConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not item conduit interactable to compact machine '" + compactMachine.name + "'");
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
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not energy conduit interactable to compact machine '" + compactMachine.name + "'");
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
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not fluid conduit interactable to compact machine '" + compactMachine.name + "'");
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
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not signal conduit interactable to compact machine '" + compactMachine.name + "'");
                        return;
                    }
                    signalPorts[positionOutsideCompactMachine] = signalConduitInteractable;
                    break;
            }
        }

        private void duplicateWarning(Vector2Int key, ConduitType type) {
            Debug.LogWarning("Duplicate key " + key + " of type " + type + " for compact machine " + compactMachine.name);
        }
    }
}

