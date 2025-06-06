using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using Dimensions;
using WorldModule.Caves;
using Conduits.Ports;
using Chunks.Systems;
using System.IO;
using Chunks.IO;
using Chunks;
using PlayerModule;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Security.Cryptography;
using DevTools.Structures;
using Item.Slot;
using Newtonsoft.Json;
using Player;
using TileEntity.Instances.CompactMachine;
using UI.Statistics;

namespace TileEntity.Instances.CompactMachines {
    public static class CompactMachineUtils 
    {
        public const int MAX_DEPTH = 4;
        public const string CONTENT_PATH = "_content";
        public const string COMPACT_MACHINE_PATH = "CompactMachines";
        public const string META_DATA_PATH = "meta.bin";
        
        public static CompactMachineChunkSystemAssembler LoadSystemFromPath(CompactMachineInstance compactMachineInstance, List<Vector2Int> path) {
            string systemPath = Path.Combine(GetPositionFolderPath(path),CONTENT_PATH);
            return LoadSystemFromPath(compactMachineInstance, systemPath);
        }
        
        public static CompactMachineChunkSystemAssembler LoadSystemFromPath(CompactMachineInstance compactMachineInstance, string path) {
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(1,path);
            CompactMachineChunkSystemAssembler systemAssembler = new CompactMachineChunkSystemAssembler(chunks,path,1);
            systemAssembler.SetCompactMachine(compactMachineInstance,null);
            return systemAssembler;
        }
        

        public static void InitalizeCompactMachineSystem(CompactMachineInstance compactMachine, List<Vector2Int> path) {
            string savePath = Path.Combine(GetPositionFolderPath(path),CONTENT_PATH);
            Directory.CreateDirectory(savePath);
            Structure structure = StructureGeneratorHelper.LoadStructure(compactMachine.TileEntityObject.StructurePath);
            if (structure == null) {
                Debug.LogError($"Could not initialize compact compact machine {compactMachine.TileEntityObject.name}: Could not load structure");
                return;
            }
            if (structure.variants.Count == 0) {
                Debug.LogError($"Could not initalize compact compact machine {compactMachine.TileEntityObject.name} as structure has no variant");
                return;
            }
            StructureVariant variant = structure.variants[0];
            WorldTileConduitData systemData = variant.Data;
            Vector2Int chunkSize = new Vector2Int(variant.Size.x/Global.CHUNK_SIZE,variant.Size.y/Global.CHUNK_SIZE);
            WorldGenerationFactory.SaveToJson(systemData,chunkSize,1,savePath);
            Debug.Log($"{compactMachine.GetName()} Closed Chunk System Generated at {savePath}");
        }

        public static string GetPositionFolderPath(List<Vector2Int> path) {
            string systemPath = WorldLoadUtils.GetDimPath(1);
            foreach (Vector2Int position in path) {
                systemPath = Path.Combine(systemPath,$"{position.x},{position.y}");
            }
            return systemPath;
        }


        public static string GetCompactMachineHashFoldersPath()
        {
            return Path.Combine(WorldLoadUtils.GetMainPath(WorldManager.GetInstance().GetWorldName()), COMPACT_MACHINE_PATH);
        }
        public static void TeleportOutOfCompactMachine(CompactMachineInstance compactMachine) {
            DimensionManager dimensionManager = DimensionManager.Instance;
            CompactMachineDimController dimController = (CompactMachineDimController)dimensionManager.GetDimController(Dimension.CompactMachine);
            List<Vector2Int> currentPath = dimController.CurrentSystemPath;
            List<Vector2Int> newPath = new List<Vector2Int>();
            for (int i = 0; i < currentPath.Count-1; i++)
            {
                newPath.Add(currentPath[i]);
            }
            bool locked = dimController.IsLocked(newPath);
            CompactMachineTeleportKey parentKey = new CompactMachineTeleportKey(newPath,locked);
            Dimension dimension = newPath.Count == 0 ? Dimension.OverWorld : Dimension.CompactMachine;
            dimensionManager.SetPlayerSystem(
                PlayerManager.Instance.GetPlayer(),
                dimension,
                compactMachine.GetWorldPosition(),
                key:parentKey
            );
        }
        public static void TeleportIntoCompactMachine(CompactMachineInstance compactMachine) {
            if (!compactMachine.IsActive) return;
            CompactMachineTeleportKey key = compactMachine.GetTeleportKey();
            
            Vector2Int teleportPosition = compactMachine.Teleporter == null
                ? (Global.CHUNK_SIZE / 2 + 1) * Vector2Int.one
                : compactMachine.Teleporter.GetCellPosition() + Vector2Int.one;
            Vector2 vector2Position = new Vector2(teleportPosition.x/2f, teleportPosition.y/2f);
            DimensionManager dimensionManager = DimensionManager.Instance;
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Teleportations_Into_Compact_Machines]++;
            int depth = key.Path.Count;
            int currentFarthestDepth = playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Largest_Compact_Machine_Depth];
            if (depth > currentFarthestDepth)
            {
                playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Largest_Compact_Machine_Depth] = depth;
            }
            dimensionManager.SetPlayerSystem(
                playerScript,
                Dimension.CompactMachine,
                vector2Position,
                key:key
            );
        }

        public static string GenerateHash()
        {
            
        
            const int ATTEMPTS = 64;
            for (int i = 0; i < ATTEMPTS; i++)
            {
                string hashString = GlobalHelper.GenerateHash();
                if (!HashExists(hashString)) return hashString;
                Debug.Log($"Hash collision '{hashString}'");
            }
            // Has to fail 64 attempts of finding a hash which doesn't exist in a 2^64 search space. 
            // Assuming a very 65536 compact machines, the odds of this are astromonically low.
            // 1/(2^64/65536)^64
            throw new Exception("Hash generation failed somehow");
        }

        public static bool HashExists(string hash)
        {
            string compactMachineFolder = GetCompactMachineHashFoldersPath();
            string[] folders = Directory.GetDirectories(compactMachineFolder);
            foreach (string folder in folders)
            {
                string fileName = Path.GetFileName(folder);
                if (hash == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] GetAllHashes()
        {
            string compactMachineFolder = GetCompactMachineHashFoldersPath();
            string[] hashes = Directory.GetDirectories(compactMachineFolder);

            for (int i = 0; i < hashes.Length; i++)
            {
                string fileName = Path.GetFileName(hashes[i]);
                hashes[i] = fileName;
            }

            return hashes;
        }

        public static void ActivateHashSystem(string hash, List<Vector2Int> path)
        {
            string folderPath = GetPositionFolderPath(path);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string hashContentPath = Path.Combine(GetCompactMachineHashFoldersPath(), hash);
            CompactMachineMetaData metaData = GetMetaDataFromHash(hash);
            GlobalHelper.CopyDirectory(hashContentPath, folderPath);
            if (!DevMode.Instance.noPlaceCost && metaData.Instances <= 1)
            {
                Debug.Log($"Deleted hashed content at '{hashContentPath}'");
                Directory.Delete(hashContentPath,true);
            }
        }

        public static void InitializeHashFolder(string hashString, string tileID)
        {
            string compactMachineFolder = GetCompactMachineHashFoldersPath();
            string newPath = Path.Combine(compactMachineFolder, hashString);
            Directory.CreateDirectory(newPath);
            InitializeMetaData(newPath, tileID);
            Debug.Log($"Created Compact Machine Hash folder at '{newPath}'");
        }

        internal static CompactMachineMetaData GetMetaData(string path)
        {
            if (!Directory.Exists(path)) return null;
            if (!Directory.Exists(path)) return null;
            string metaDataPath = Path.Combine(path, META_DATA_PATH);
            if (!File.Exists(metaDataPath))
            {
                return null;
            }
            byte[] binary = File.ReadAllBytes(metaDataPath);
            string json = WorldLoadUtils.DecompressString(binary);
            if (json is null or "null") // Don't know what genius made this return "null". Don't think its me but who knows
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<CompactMachineMetaData>(json);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }

        internal static CompactMachineMetaData GetMetaDataFromHash(string hash)
        {
            if (hash == null) return null;
            try
            {
                string path = Path.Combine(GetCompactMachineHashFoldersPath(), hash);
                return GetMetaData(path);
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning($"Could not load compact machine meta data {e.Message}");
                return null;
            }
        }
        internal static void InitializeMetaData(string path, string tileID)
        {
            SaveMetaDataJson(GetDefaultMetaData(tileID), path);
        }

        internal static CompactMachineMetaData GetDefaultMetaData(string tileID)
        {
            return new CompactMachineMetaData("New Compact Machine", false,0, tileID);
        }

        internal static void SaveMetaDataJson(CompactMachineMetaData metaData, string path)
        {
            string metaDataJson = JsonConvert.SerializeObject(metaData);
            byte[] metaDataBinary = WorldLoadUtils.CompressString(metaDataJson);
            string metaDataPath = Path.Combine(path, META_DATA_PATH);
            File.WriteAllBytes(metaDataPath, metaDataBinary);
        }
        
        internal static void SaveMetaDataJsonFromHash(CompactMachineMetaData metaData, string hash)
        {
            string hashPath = Path.Combine(GetCompactMachineHashFoldersPath(), hash);
            SaveMetaDataJson(metaData, hashPath);
        }
        public static void InitializeCompactMachineFolder()
        {
            string path = GetCompactMachineHashFoldersPath();
            Directory.CreateDirectory(path);
        }
    }
}
