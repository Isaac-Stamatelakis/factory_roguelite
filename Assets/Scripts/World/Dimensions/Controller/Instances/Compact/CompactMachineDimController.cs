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
using TileMaps.Layer;

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

        public ClosedChunkSystem GetActiveSystem(IDimensionTeleportKey key)
        {
            return key is not CompactMachineTeleportKey compactMachineTeleportKey ? null : activeSystems.GetValueOrDefault(compactMachineTeleportKey);
        }
        private void LoadCompactMachineSystem(CompactMachineInstance compactMachine, CompactMachineTree tree, string path) {
            SoftLoadedClosedChunkSystem system = tree.System;
            foreach (IChunk chunk in system.Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            
                            switch (tileEntity)
                            {
                                case ICompactMachineInteractable compactMachineInteractable:
                                {
                                    if (compactMachine != null) {
                                        compactMachineInteractable.SyncToCompactMachine(compactMachine);
                                    }
                                    break;
                                }
                                case CompactMachineInstance nestedCompactMachine:
                                {
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
                                    LoadCompactMachineSystem(nestedCompactMachine,newTree,nestedPath);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadCompactMachineInteractableTileEntity()
        {
            
        }
        public bool HasSystem(CompactMachineTeleportKey key) {
            return systemTree.getSystem(key.Path) != null;
        }

        /// <summary>
        /// Creates a new system for a compact machine. Path should be ordered with depth 0 at 0, depth n at n.
        /// Null path means that 
        /// </summary>
        
        public void AddNewSystem(CompactMachineTeleportKey key, CompactMachineInstance compactMachine) {
            List<Vector2Int> systemPath = key.Path;
            List<Vector2Int> parentPath = new List<Vector2Int>();
            for (int i = 0; i < systemPath.Count-1; i++) {
                parentPath.Add(systemPath[i]);
            }
            Vector2Int placePosition = systemPath.Last();
            CompactMachineTree parentTree = systemTree.getTree(parentPath);
            CompactMachineHelper.InitalizeCompactMachineSystem(compactMachine, systemPath);
            SoftLoadedClosedChunkSystem newSystem = CompactMachineHelper.loadSystemFromPath(systemPath);
            
            CompactMachineTree newTree = new CompactMachineTree(
                newSystem
            );
            parentTree.Children[placePosition] = newTree;
            systems.Add(newSystem);
            foreach (IChunk chunk in newSystem.Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            if (tileEntity is ICompactMachineInteractable compactMachineInteractable) {
                                compactMachineInteractable.SyncToCompactMachine(compactMachine);
                            }
                        }
                    }
                }
            }
        }

        public ClosedChunkSystem ActivateSystem(IDimensionTeleportKey key)
        {
            if (key is not CompactMachineTeleportKey compactMachineTeleportKey) {
                return null;
            }
            List<Vector2Int> path = compactMachineTeleportKey.Path;
            if (path.Count == 0) {
                return baseDimController.ActivateSystem();
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
            area.Initialize(this, system.CoveredArea, 1, system);
            return area;
        }

        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem, DimController baseDimController)
        {
            this.baseDimController =(ISingleSystemController) baseDimController;
            systemTree = new CompactMachineTree(baseSystem);
            LoadCompactMachineSystem(null,systemTree,WorldLoadUtils.GetDimPath(1));
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

