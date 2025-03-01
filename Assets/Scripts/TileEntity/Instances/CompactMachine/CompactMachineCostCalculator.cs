using System.Collections.Generic;
using System.IO;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;
using Chunks.Systems;
using DevTools.Structures;
using Item.Slot;
using Items;
using TileEntity.Instances.CompactMachines;
using UnityEngine;
using WorldModule.Caves;

namespace TileEntity.Instances.CompactMachine
{
    public class CompactMachineCostCalculator
    {
        
        public List<ItemSlot> GetCost(string hash)
        {
            Dictionary<string, uint> idAmountDict = new Dictionary<string, uint>();
            string path = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), hash);
            CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
            string structurePath = GetStructurePath(metaData.TileID);
            Accumulate(idAmountDict, path, structurePath);
            List<ItemSlot> itemSlots = ItemSlotUtils.FromDict(idAmountDict,Global.MAX_SIZE);
            return itemSlots;
        }

        private void Accumulate(Dictionary<string, uint> idAmountDict, string path, string structureName)
        {
            string contentPath = Path.Combine(path, CompactMachineUtils.CONTENT_PATH);
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(1,contentPath);
            foreach (SoftLoadedConduitTileChunk chunk in chunks)
            {
                foreach (IChunkPartition partition in chunk.Partitions)
                {
                    SeralizedWorldData data = partition.GetData();
                    AddIDsToDict(data.baseData.ids, idAmountDict);
                    
                    // Accumulate nested compact machines
                    for (var x = 0; x < data.baseData.ids.GetLength(0); x++)
                    {
                        for (var y = 0; y < data.baseData.ids.GetLength(1); y++)
                        {
                            var id = data.baseData.ids[x, y];
                            TileItem tileItem = ItemRegistry.GetInstance().GetTileItem(id);
                            if (tileItem?.tileEntity is not CompactMachines.CompactMachine compactMachine) continue;
                            string newStructurePath = compactMachine.StructurePath;
                            Vector2Int cellPosition = partition.GetRealPosition() + new Vector2Int(x, y);
                            string newPath = Path.Combine(path, $"{cellPosition.x}, {cellPosition.y}]"); // Should probably generalize this
                            Accumulate(idAmountDict, newPath, newStructurePath);
                        }
                    }

                    AddIDsToDict(data.backgroundData.ids, idAmountDict);
                    
                    if (data is not WorldTileConduitData worldTileConduitData) continue;

                    AddIDsToDict(worldTileConduitData.itemConduitData.ids, idAmountDict);
                    AddIDsToDict(worldTileConduitData.fluidConduitData.ids, idAmountDict);
                    AddIDsToDict(worldTileConduitData.energyConduitData.ids, idAmountDict);
                    AddIDsToDict(worldTileConduitData.signalConduitData.ids, idAmountDict);
                    AddIDsToDict(worldTileConduitData.matrixConduitData.ids, idAmountDict);
                }
            }

            Structure structure = StructureGeneratorHelper.LoadStructure(structureName);
            if (structure == null || structure.variants.Count == 0) return;
            WorldTileConduitData structureData = structure.variants[0].Data;
            RemoveIDsFromDict(structureData.baseData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.backgroundData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.itemConduitData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.fluidConduitData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.energyConduitData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.signalConduitData.ids, idAmountDict);
            RemoveIDsFromDict(structureData.matrixConduitData.ids, idAmountDict);
        }

        private void AddIDsToDict(string[,] idArray, Dictionary<string, uint> idAmountDict)
        {
            foreach (string id in idArray)
            {
                if (id == null) continue;
                idAmountDict.TryAdd(id, 0);
                idAmountDict[id]++;
            }
        }
        
        private void RemoveIDsFromDict(string[,] idArray, Dictionary<string, uint> idAmountDict)
        {
            foreach (string id in idArray)
            {
                if (id == null) continue;
                if (!idAmountDict.ContainsKey(id)) continue;
                idAmountDict[id]--;
                if (idAmountDict[id] == 0) idAmountDict.Remove(id);
            }
        }

        private void AddItemConduitData(string[,] conduitData)
        {
            
        }

        private string GetStructurePath(string tileId)
        {
            TileItem tileItem = ItemRegistry.GetInstance().GetTileItem(tileId);
            if (tileItem?.tileEntity is not CompactMachines.CompactMachine compactMachine) return null;
            return compactMachine.StructurePath;

        }
    }
}
