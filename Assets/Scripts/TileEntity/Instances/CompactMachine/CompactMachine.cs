using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using UnityEngine.Tilemaps;
using ChunkModule;
using GUIModule;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine")]
    public class CompactMachine : TileEntity, IClickableTileEntity, IConduitInteractable, IEnergyConduitInteractable, IItemConduitInteractable, IFluidConduitInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        [SerializeField] public GameObject tilemapContainer;
        [SerializeField] public GameObject uiPrefab;
        private CompactMachineInventory inventory;

        public CompactMachineInventory Inventory { get => inventory; set => inventory = value; }
        public ItemSlot extractItem()
        {
            throw new System.NotImplementedException();
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public ref int getEnergy()
        {
            throw new System.NotImplementedException();
        }


        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
            if (!CompactMachineHelper.isCreated(this)) {
                CompactMachineHelper.initalizeCompactMachineSystem(this);
            }
        }

        public int insertEnergy(int energy)
        {
            throw new System.NotImplementedException();
        }

        public void insertItem(ItemSlot itemSlot)
        {
            throw new System.NotImplementedException();
        }

        public void onClick()
        {
            if (Input.GetKey(KeyCode.LeftShift)) {
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
    }

    public class CompactMachineInventory {
        private CompactMachine compactMachine;
        private Dictionary<Vector2Int, IEnergyConduitInteractable> energyPorts;
        private Dictionary<Vector2Int, IItemConduitInteractable> itemPorts;
        private Dictionary<Vector2Int, IFluidConduitInteractable> fluidPorts;
        private Dictionary<Vector2Int, ISignalConduitInteractable> signalPorts;

        public void addPort(TileEntity tileEntity, ConduitType type) {
            Vector2Int positionInCompactMachine = tileEntity.getCellPosition()-CompactMachineHelper.getPositionInNextRing(compactMachine.getCellPosition()); 
            switch (type) {
                case ConduitType.Item:
                    if (itemPorts.ContainsKey(positionInCompactMachine)) {
                        duplicateWarning(positionInCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not IItemConduitInteractable itemConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not item conduit interactable to compact machine '" + compactMachine.name + "'");
                        return;
                    }
                    itemPorts[positionInCompactMachine] = itemConduitInteractable;
                    break;
                case ConduitType.Energy:
                    if (itemPorts.ContainsKey(positionInCompactMachine)) {
                        duplicateWarning(positionInCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not energy conduit interactable to compact machine '" + compactMachine.name + "'");
                        return;
                    }
                    energyPorts[positionInCompactMachine] = energyConduitInteractable;
                    break;
                case ConduitType.Fluid:
                    if (itemPorts.ContainsKey(positionInCompactMachine)) {
                        duplicateWarning(positionInCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not IFluidConduitInteractable fluidConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not fluid conduit interactable to compact machine '" + compactMachine.name + "'");
                        return;
                    }
                    fluidPorts[positionInCompactMachine] = fluidConduitInteractable;
                    break;
                case ConduitType.Signal:
                    if (itemPorts.ContainsKey(positionInCompactMachine)) {
                        duplicateWarning(positionInCompactMachine,type);
                        return;
                    }
                    if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                        Debug.LogWarning("Attempted to add " + tileEntity.name + " which is not signal conduit interactable to compact machine '" + compactMachine.name + "'");
                        return;
                    }
                    signalPorts[positionInCompactMachine] = signalConduitInteractable;
                    break;
            }
        }

        private void duplicateWarning(Vector2Int key, ConduitType type) {
            Debug.LogWarning("Duplicate key " + key + " of type " + type + " for compact machine " + compactMachine.name);
        }
    }
}

