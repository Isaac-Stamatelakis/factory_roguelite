using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.IO;
using Chunks.Systems;
using Chunks.Partitions;
using TileEntity.Instances.CompactMachines;
using WorldModule;
using System.IO;
using System;
using System.Linq;
using TileEntity;

namespace Dimensions {
    public interface ICompactMachineDimension {
        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem,DimController dimController);
    }
    

    public class CompactMachineDimController : DimController, IMultipleSystemController, ICompactMachineDimension
    {
        public override void Awake() {
            base.Awake();
        }
        private ISingleSystemController baseDimController;
        private CompactMachineTree systemTree;
        private List<SoftLoadedClosedChunkSystem> systems = new List<SoftLoadedClosedChunkSystem>();
        private Dictionary<CompactMachineTeleportKey,CompactMachineClosedChunkSystem> activeSystems = new Dictionary<CompactMachineTeleportKey, CompactMachineClosedChunkSystem>();
        public void FixedUpdate() {
            foreach (SoftLoadedClosedChunkSystem system in systems) {
                system.TickUpdate();
            }
        }
        public void OnDestroy() {
            foreach (SoftLoadedClosedChunkSystem system in systems) {
                system.Save();
            }
        }

        public ClosedChunkSystem getActiveSystem(IDimensionTeleportKey key)
        {
            if (key is not CompactMachineTeleportKey compactMachineTeleportKey) {
                return null;
            }
            if (activeSystems.ContainsKey(compactMachineTeleportKey)) {
                return activeSystems[compactMachineTeleportKey];
            }
            return null;
        }
        private void loadCompactMachineSystem(CompactMachineInstance compactMachine, CompactMachineTree tree, string path) {
            Vector2Int positionInSystem;
            if (compactMachine == null) {
                positionInSystem = Vector2Int.zero;
            } else {
                positionInSystem = compactMachine.getCellPosition();
            }
            SoftLoadedClosedChunkSystem system = tree.System;
            foreach (IChunk chunk in system.Chunks) {
                foreach (IChunkPartition partition in chunk.getChunkPartitions()) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            if (tileEntity is ICompactMachineInteractable compactMachineInteractable) {
                                if (compactMachine != null) {
                                    compactMachineInteractable.syncToCompactMachine(compactMachine);
                                }
                                
                            } else if (tileEntity is CompactMachineInstance nestedCompactMachine) {
                                Vector2Int newPosition = nestedCompactMachine.getCellPosition();
                                string nestedPath = Path.Combine(path,$"{newPosition.x},{newPosition.y}");
                                string contentPath = Path.Combine(nestedPath,CompactMachineHelper.CONTENT_PATH);
                                SoftLoadedClosedChunkSystem newSystem = InactiveClosedChunkFactory.Import(contentPath);
                                if (newSystem == null) {
                                    Debug.LogError($"No system at path {nestedPath}");
                                    continue;
                                }
                                newSystem.softLoad();
                                systems.Add(newSystem);
                                CompactMachineTree newTree = new CompactMachineTree(newSystem);
                                tree.Children[(Vector2Int)newPosition] = newTree;
                                loadCompactMachineSystem(nestedCompactMachine,newTree,nestedPath);
                            }
                        }
                    }
                }
            }
        }
        public bool hasSystem(CompactMachineTeleportKey key) {
            return systemTree.getSystem(key.Path) != null;
        }

        /// <summary>
        /// Creates a new system for a compact machine. Path should be ordered with depth 0 at 0, depth n at n.
        /// Null path means that 
        /// </summary>
        public void addNewSystem(CompactMachineTeleportKey key, CompactMachineInstance compactMachine) {
            StartCoroutine(addNewSystemCoroutine(key,compactMachine));
        }

        private IEnumerator addNewSystemCoroutine(CompactMachineTeleportKey key, CompactMachineInstance compactMachine) {
            List<Vector2Int> systemPath = key.Path;
            List<Vector2Int> parentPath = new List<Vector2Int>();
            for (int i = 0; i < systemPath.Count-1; i++) {
                parentPath.Add(systemPath[i]);
            }
            Vector2Int placePosition = systemPath.Last();
            CompactMachineTree parentTree = systemTree.getTree(parentPath);
            yield return CompactMachineHelper.initalizeCompactMachineSystem(compactMachine,systemPath);
            SoftLoadedClosedChunkSystem newSystem = CompactMachineHelper.loadSystemFromPath(systemPath);
            CompactMachineTree newTree = new CompactMachineTree(
                newSystem
            );
            parentTree.Children[placePosition] = newTree;
            systems.Add(newSystem);
            foreach (IChunk chunk in newSystem.Chunks) {
                foreach (IChunkPartition partition in chunk.getChunkPartitions()) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            if (tileEntity is ICompactMachineInteractable compactMachineInteractable) {
                                compactMachineInteractable.syncToCompactMachine(compactMachine);
                            }
                        }
                    }
                }
            }
        }

        public ClosedChunkSystem activateSystem(IDimensionTeleportKey key, Vector2Int dimOffsetPosition)
        {
            if (key is not CompactMachineTeleportKey compactMachineTeleportKey) {
                return null;
            }
            List<Vector2Int> path = compactMachineTeleportKey.Path;
            if (path.Count == 0) {
                return baseDimController.activateSystem(dimOffsetPosition);
            }
            if (activeSystems.ContainsKey(compactMachineTeleportKey)) {
                return activeSystems[compactMachineTeleportKey];
            }
            SoftLoadedClosedChunkSystem system = systemTree.getSystem(path);
            if (system == null) {
                Debug.LogError($"Tried to get compact machine at path {path}");
            }
            GameObject closedChunkSystemObject = new GameObject();
            Vector2Int center = system.GetCenter();
            closedChunkSystemObject.name="Compact Machine [" + center.x + "," + center.y +"]";
            CompactMachineClosedChunkSystem area = closedChunkSystemObject.AddComponent<CompactMachineClosedChunkSystem>();
            area.setCompactMachineKey(compactMachineTeleportKey);
            area.transform.SetParent(transform,false);
            area.initalize(
                this,
                system.CoveredArea,
                1,
                system,
                dimOffsetPosition
            );
            return area;
        }

        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem, DimController baseDimController)
        {
            this.baseDimController =(ISingleSystemController) baseDimController;
            systemTree = new CompactMachineTree(baseSystem);
            loadCompactMachineSystem(null,systemTree,WorldLoadUtils.GetDimPath(1));
            Debug.Log($"Loaded {systems.Count} Compact Machine Systems");
        }

        private class CompactMachineTree {
            public SoftLoadedClosedChunkSystem System;
            public Dictionary<Vector2Int,CompactMachineTree> Children;

            public CompactMachineTree(SoftLoadedClosedChunkSystem system)
            {
                System = system;
                Children = new Dictionary<Vector2Int, CompactMachineTree>();
            }
            public SoftLoadedClosedChunkSystem getSystem(List<Vector2Int> path, int depth=0) {
                if (depth == path.Count) {
                    return System;
                }
                Vector2Int key = path[depth];
                if (Children.ContainsKey(key)) {
                    return Children[key].getSystem(path,depth+1);
                }
                return null;
            }
            public CompactMachineTree getTree(List<Vector2Int> path, int depth=0) {
                if (depth == path.Count) {
                    return this;
                }
                Vector2Int key = path[depth];
                if (Children.ContainsKey(key)) {
                    return Children[key].getTree(path,depth+1);
                }
                return null;
            }
            public override string ToString() {
                return ToString(0);
            }

            private string ToString(int depth) {
                var indent = new string(' ', depth * 2);
                var result = $"{indent}System\n";

                foreach (var child in Children)
                {
                    result += $"{indent}Child Position: {child.Key}\n";
                    result += child.Value.ToString(depth + 1);
                }

                return result;
            }
        }
    }
}

