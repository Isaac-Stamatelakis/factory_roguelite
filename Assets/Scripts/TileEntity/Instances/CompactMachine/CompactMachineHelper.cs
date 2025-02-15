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
using DevTools.Structures;
using Newtonsoft.Json;

namespace TileEntity.Instances.CompactMachines {
    public static class CompactMachineHelper 
    {
        /// 4*24^6 < |2^31-1| < |-2^31|
        private static int maxDepth = 4;
        public static int MaxDepth {get => maxDepth; }
        public static readonly string CONTENT_PATH = "_content";

        public static Vector2Int getPositionInNextRing(Vector2Int position) {
            return position*seperationPerTile();
        }
        public static SoftLoadedClosedChunkSystem loadSystemFromPath(List<Vector2Int> path) {
            string systemPath = Path.Combine(getPositionFolderPath(path),CONTENT_PATH);
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(1,systemPath);
            SoftLoadedClosedChunkSystem system = new SoftLoadedClosedChunkSystem(chunks,systemPath);
            system.SoftLoad();
            return system;
        }

        /// </summary>
        /// 
        /// <summary>
        public static Vector2Int getRingSizeInChunks() {
            IntervalVector dim0Area = WorldCreation.GetDim0Bounds();
            return dim0Area.getSize();
        }

        public static List<Vector2Int> getTreePath(Vector2Int position) {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int size = getRingSizeInChunks();
            int depth = getDepth(position);
            double divider = size.x * Mathf.Pow(seperationPerTile(),depth); 
            Vector2Int temp = position;
            while (depth > 0) {
                double x = position.x/divider;
                double y = position.y/divider;
                depth --;   
                Vector2Int depthPosition = new Vector2Int((int)x,(int)y);
                temp -= depthPosition;
                position -= depthPosition;
                path.Add(depthPosition);
                divider /= seperationPerTile();
            }
            path.Add(temp);
            return path;
        }

        public static int getDepth(Vector2Int position) {
            Vector2Int size = getRingSizeInChunks();
            int depth = 0;
            int val = size.x*seperationPerTile();
            while (position.x >= val) {
                depth++;
                val *= seperationPerTile();
            }
            return depth;   
        }
        public static int seperationPerTile() {
            return Global.CHUNK_SIZE;
        }

        public static Vector2Int getParentPosition(CompactMachineInstance compactMachine) {
            Vector2Int position = compactMachine.getCellPosition();
            return new Vector2Int(Mathf.FloorToInt(position.x/seperationPerTile()),Mathf.FloorToInt(position.y/seperationPerTile()));
        }


        /// </summary>
        /// Maps a port inside a compact machine to its port on the compact machine tile entity
        /// <summary>
        public static Vector2Int getPortPositionInLayout(Vector2Int relativePortPosition, ConduitPortLayout layout, ConduitType type) {
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
            string savePath = Path.Combine(getPositionFolderPath(path),CONTENT_PATH);
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

        public static string getPositionFolderPath(List<Vector2Int> path) {
            string systemPath = WorldLoadUtils.GetDimPath(1);
            foreach (Vector2Int position in path) {
                systemPath = Path.Combine(systemPath,$"{position.x},{position.y}");
            }
            return systemPath;
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
    }
}
