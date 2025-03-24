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
using Conduits.Systems;
using Entities;
using Item.Slot;
using Items;
using Items.Tags;
using Newtonsoft.Json;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.Matrix;
using UI;
using WorldModule;

namespace TileEntity.Instances.CompactMachines {
    public interface ITagPlacementTileEntity
    {
        public ItemTag GetItemTag();
    }
    public class CompactMachineInstance : TileEntityInstance<CompactMachine>, 
        IRightClickableTileEntity, ICompactMachine, 
        IBreakActionTileEntity, ISerializableTileEntity,
        IItemConduitInteractable, ISignalConduitInteractable, IEnergyConduitInteractable, IConduitPortTileEntity, ITextPreviewTileEntity
    {
        private CompactMachineTeleporterInstance teleporter;
        public CompactMachineTeleporterInstance Teleporter { get => teleporter; set => teleporter = value; }
        public string Hash => compactMachineData.Hash;
        public bool IsActive => compactMachineData.Active;
        private CompactMachineData compactMachineData;
        private Dictionary<ConduitType, IConduitInteractable> inputConduitPortMap = new Dictionary<ConduitType, IConduitInteractable>();
        private Dictionary<ConduitType, IConduitInteractable> outputConduitPortMap = new Dictionary<ConduitType, IConduitInteractable>();
        public CompactMachineInstance(CompactMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }

        public CompactMachineTeleportKey GetTeleportKey()
        {
            if (chunk is not ILoadedChunk loadedChunk) return null;
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                Debug.LogError("Tried to create compact machine in invalid dimension");
                return null;
            }
            List<Vector2Int> path = new List<Vector2Int>();
            if (loadedChunk.GetSystem() is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) {
                CompactMachineTeleportKey key = compactMachineClosedChunkSystem.GetCompactMachineKey();
                foreach (Vector2Int vector in key.Path) {
                    path.Add(vector);
                }
            }
            path.Add(GetCellPosition());
            bool locked = compactMachineDimManager.GetCompactMachineDimController().IsLocked(path);
            return new CompactMachineTeleportKey(path,locked);
        }
        

        public void SaveSystem()
        {
            CompactMachineTeleportKey key = GetTeleportKey();
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager)
            {
                return;
            }
            compactMachineDimManager.GetCompactMachineDimController().SaveTree(key.Path);
        }
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }
        
        public void OnRightClick()
        {
            CompactMachineUtils.TeleportIntoCompactMachine(this);
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
            ItemEntityFactory.SpawnItemEntity(GetWorldPosition(), itemSlot, loadedChunk.GetEntityContainer());

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
            compactMachineData = new CompactMachineData(true, newHash);
            
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

            if (dimController.HasSystem(thisKey)) return;
            
            dimController.AddNewSystem(thisKey, this, newHash, true);
            if (newHash == null || !CompactMachineUtils.HashExists(newHash) || DevMode.Instance.noPlaceCost)
            {
                compactMachineData.Hash = CompactMachineUtils.GenerateHash();
                CompactMachineUtils.InitializeHashFolder(compactMachineData.Hash, tileItem?.id);
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

        public void SetHash(string hash)
        {
            compactMachineData.Hash = hash;
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
                compactMachineDimManager.GetCompactMachineDimController().AddNewSystem(key, this, compactMachineData.Hash,false);
            }
            else
            {
                compactMachineDimManager.GetCompactMachineDimController().RemoveCompactMachineSystem(key, compactMachineData.Hash);
            }
        }

        public bool AddPort(CompactMachinePortType portType, ConduitType conduitType, IConduitInteractable interactable)
        {
            var map = GetConduitMap(portType);
            if (map.TryAdd(conduitType, interactable))
            {
                return true;
            }
            map[conduitType] = null;
            return false;
        }

        public void RemovePort(CompactMachinePortType portType, ConduitType conduitType, Vector2Int portPosition)
        {
            var map = GetConduitMap(portType);
            if (!map.Remove(conduitType))
            {
                return;
            }
            // If is not removed then there must be multiple of the port within the system.
            if (DimensionManager.Instance is not ICompactMachineDimManager compactMachineDimManager) {
                return;
            }

            CompactMachineTeleportKey key = GetTeleportKey();
            compactMachineDimManager.GetCompactMachineDimController().ReSyncConduitPorts(key.Path,portType,conduitType, portPosition);
        }

        private Dictionary<ConduitType, IConduitInteractable> GetConduitMap(CompactMachinePortType portType)
        {
            switch (portType)
            {
                case CompactMachinePortType.Input:
                    return inputConduitPortMap;
                case CompactMachinePortType.Output:
                    return outputConduitPortMap;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            ConduitType conduitType;
            switch (state)
            {
                case ItemState.Solid:
                    conduitType = ConduitType.Item;
                    break;
                case ItemState.Fluid:
                    conduitType = ConduitType.Fluid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            IConduitInteractable interactable = outputConduitPortMap.GetValueOrDefault(conduitType);
            return ((IItemConduitInteractable)interactable)?.ExtractItem(state, portPosition, filter);
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            ConduitType conduitType;
            switch (state)
            {
                case ItemState.Solid:
                    conduitType = ConduitType.Item;
                    break;
                case ItemState.Fluid:
                    conduitType = ConduitType.Fluid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            IConduitInteractable interactable = inputConduitPortMap.GetValueOrDefault(conduitType);
            ((IItemConduitInteractable)interactable)?.InsertItem(state, toInsert, portPosition);
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            IConduitInteractable interactable = outputConduitPortMap.GetValueOrDefault(ConduitType.Signal);
            return ((ISignalConduitInteractable)interactable)?.ExtractSignal(portPosition) ?? false;
        }

        public void InsertSignal(bool active, Vector2Int portPosition)
        {
            IConduitInteractable interactable = inputConduitPortMap.GetValueOrDefault(ConduitType.Signal);
            ((ISignalConduitInteractable)interactable)?.InsertSignal(active, portPosition);
        }

        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            IConduitInteractable interactable = outputConduitPortMap.GetValueOrDefault(ConduitType.Energy);
            return ((IEnergyConduitInteractable)interactable)?.InsertEnergy(energy, portPosition) ?? 0;
        }

        public ulong GetEnergy(Vector2Int portPosition)
        {
            IConduitInteractable interactable = inputConduitPortMap.GetValueOrDefault(ConduitType.Energy);
            return ((IEnergyConduitInteractable)interactable)?.GetEnergy(portPosition) ?? 0;
        }

        public void SetEnergy(ulong energy, Vector2Int portPosition)
        {
            IConduitInteractable interactable = outputConduitPortMap.GetValueOrDefault(ConduitType.Energy);
            ((IEnergyConduitInteractable)interactable)?.SetEnergy(energy, portPosition);
        }

        public string GetTextPreview()
        {
            CompactMachineMetaData machineMetaData = CompactMachineUtils.GetMetaDataFromHash(compactMachineData.Hash);
            return machineMetaData?.Name;
        }
    }
}

