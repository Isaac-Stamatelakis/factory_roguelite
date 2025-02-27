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
using Newtonsoft.Json;

namespace TileEntity.Instances.CompactMachines {
    public static class CompactMachineUtils 
    {
        public const int MAX_DEPTH = 4;
        public const string CONTENT_PATH = "_content";
        public const string COMPACT_MACHINE_PATH = "CompactMachines";
        
        public static SoftLoadedClosedChunkSystem LoadSystemFromPath(List<Vector2Int> path) {
            string systemPath = Path.Combine(GetPositionFolderPath(path),CONTENT_PATH);
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(1,systemPath);
            SoftLoadedClosedChunkSystem system = new SoftLoadedClosedChunkSystem(chunks,systemPath);
            system.SoftLoad();
            return system;
        }
        

        /// </summary>
        /// Maps a port inside a compact machine to its port on the compact machine tile entity
        /// <summary>
        public static Vector2Int GetPortPositionInLayout(Vector2Int relativePortPosition, ConduitPortLayout layout, ConduitType type) {
            List<TileEntityPortData> possiblePorts = null;
            switch (type) {
                case ConduitType.Item:
                    possiblePorts = layout.itemPorts;
                    break;
                case ConduitType.Energy:
                    possiblePorts = layout.energyPorts;
                    break;
                case ConduitType.Fluid:
                    possiblePorts = layout.fluidPorts;
                    break;
                case ConduitType.Signal:
                    possiblePorts = layout.signalPorts;
                    break;
            }
            float smallestDistance = float.PositiveInfinity;
            TileEntityPortData closestPortData = null;
            foreach (TileEntityPortData port in possiblePorts) {
                // maps portData position to the center of its relative chunk (eg (1,1) -> (36,36))
                Vector2 positionInSideCompactMachine =  (port.position + Vector2.one/2f) * (Global.CHUNK_SIZE); 
                float dist = Vector2.Distance(positionInSideCompactMachine,relativePortPosition);
                if (dist < smallestDistance) {
                    smallestDistance = dist;
                    closestPortData = port;
                }
            }
            if (closestPortData == null) {
                Debug.LogError("Could not find portData to map compact machine to");
                return Vector2Int.zero;
            }
            return closestPortData.position;
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
            Debug.Log($"{compactMachine.getName()} Closed Chunk System Generated at {savePath}");
        }

        public static string GetPositionFolderPath(List<Vector2Int> path) {
            string systemPath = WorldLoadUtils.GetDimPath(1);
            foreach (Vector2Int position in path) {
                systemPath = Path.Combine(systemPath,$"{position.x},{position.y}");
            }
            return systemPath;
        }


        public static string GetHashedPath()
        {
            return Path.Combine(WorldLoadUtils.GetMainPath(WorldManager.getInstance().GetWorldName()), COMPACT_MACHINE_PATH);
        }
        public static void TeleportOutOfCompactMachine(CompactMachineInstance compactMachine) {
            DimensionManager dimensionManager = DimensionManager.Instance;
            IChunk chunk = compactMachine.getChunk();
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            List<Vector2Int> parentPath = new List<Vector2Int>();
            if (closedChunkSystem is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) {
                CompactMachineTeleportKey key = compactMachineClosedChunkSystem.getCompactMachineKey();
                for (int i = 0; i < key.Path.Count; i++) {
                    parentPath.Add(key.Path[i]);
                }   
            }
            CompactMachineTeleportKey parentKey = new CompactMachineTeleportKey(parentPath);
            dimensionManager.SetPlayerSystem(
                PlayerManager.Instance.GetPlayer(),
                1,
                compactMachine.getCellPosition(),
                key:parentKey
            );
        }
        public static void TeleportIntoCompactMachine(CompactMachineInstance compactMachine) {
            DimensionManager dimensionManager = DimensionManager.Instance;
            IChunk chunk = compactMachine.getChunk();
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            List<Vector2Int> path = new List<Vector2Int>();
            if (closedChunkSystem is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) {
                foreach (Vector2Int vector in compactMachineClosedChunkSystem.getCompactMachineKey().Path) {
                    path.Add(vector);
                }
            }
            path.Add(compactMachine.getCellPosition());
            CompactMachineTeleportKey key = new CompactMachineTeleportKey(path);

            if (compactMachine.Teleporter == null)
            {
                Debug.LogError("Cannot teleport into compact machine as teleporter is null");
                return;
            }
            
            dimensionManager.SetPlayerSystem(
                PlayerManager.Instance.GetPlayer(),
                1,
                compactMachine.Teleporter.getCellPosition() + Vector2Int.one,
                key:key
            );
        }

        /// <summary>
        /// Generates a random hash to be used for a compact machine
        /// </summary>
        /// <returns></returns>
        public static string GenerateHash()
        {
            const int ATTEMPTS = 64;
            for (int i = 0; i < ATTEMPTS; i++)
            {
                byte[] hash = new byte[8]; 
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(hash);
                }

                string hashString = BitConverter.ToString(hash).Replace("-","");
                if (!HashExists(hashString)) return hashString;
                Debug.Log($"Hash collision '{hash}'");
            }
            // Has to fail 64 attempts of finding a hash which doesn't exist in a 2^64 search space. 
            // Assuming a very 65536 compact machines, the odds of this are astromonically low.
            // 1/(2^64/65536)^64
            throw new Exception("Hash generation failed somehow");
        }

        public static bool HashExists(string hash)
        {
            string compactMachineFolder = GetHashedPath();
            string[] folders = Directory.GetDirectories(compactMachineFolder);
            foreach (string folder in folders)
            {
                string fileName = Path.GetDirectoryName(folder);
                if (hash == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        public static void InitializeHashFolder(string hashString)
        {
            string compactMachineFolder = GetHashedPath();
            string newPath = Path.Combine(compactMachineFolder, hashString);
            Directory.CreateDirectory(newPath);
            Debug.Log($"Created Compact Machine Hash folder at '{newPath}'");
        }

        public static CompactMachineTeleportKey GetTeleportKey(ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            CompactMachineTeleportKey key = compactMachineClosedChunkSystem.getCompactMachineKey();
            foreach (Vector2Int vector in key.Path) {
                path.Add(vector);
            }
            return new CompactMachineTeleportKey(path);
        }
    }
}
