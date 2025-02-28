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
using Player;
using TileEntity;
using TileEntity.Instances.CompactMachine;
using TileMaps.Layer;

namespace Dimensions {
    public interface ICompactMachineDimension {
        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem,DimController dimController);
    }
    

    public class CompactMachineDimController : DimController, IMultipleSystemController, ICompactMachineDimension
    {
        private ISingleSystemController baseDimController;
        private CompactMachineTree systemTree;
        private List<SoftLoadedClosedChunkSystem> systems = new List<SoftLoadedClosedChunkSystem>();
        private Dictionary<CompactMachineTeleportKey,CompactMachineClosedChunkSystem> activeSystems = new Dictionary<CompactMachineTeleportKey, CompactMachineClosedChunkSystem>();
        public void FixedUpdate() {
            foreach (SoftLoadedClosedChunkSystem system in systems) {
                system.TickUpdate();
            }
        }
        public void OnDestroy()
        {
            foreach (SoftLoadedClosedChunkSystem system in systems) {
                system?.Save();
            }
        }

        public ClosedChunkSystem GetActiveSystem(IDimensionTeleportKey key)
        {
            return key is not CompactMachineTeleportKey compactMachineTeleportKey ? null : activeSystems.GetValueOrDefault(compactMachineTeleportKey);
        }

        public List<SoftLoadedClosedChunkSystem> GetAllInactiveSystems()
        {
            return systems;
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
                                    Vector2Int newPosition = nestedCompactMachine.GetCellPosition();
                                    string nestedPath = Path.Combine(path,$"{newPosition.x},{newPosition.y}");
                                    string contentPath = Path.Combine(nestedPath,CompactMachineUtils.CONTENT_PATH);
                                    SoftLoadedClosedChunkSystem newSystem = InactiveClosedChunkFactory.Import(contentPath);
                                    if (newSystem == null) {
                                        Debug.LogError($"No system at path {nestedPath}");
                                        continue;
                                    }
                                    newSystem.SoftLoad();
                                    systems.Add(newSystem);
                                    CompactMachineTree newTree = new CompactMachineTree(newSystem,nestedCompactMachine.Hash);
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

        public void RemoveCompactMachineSystem(CompactMachineTeleportKey key, string hash)
        {
            CompactMachineTree tree = systemTree.getTree(key.Path);
            SaveTree(tree);
            RemoveNode(key);
            if (hash == null) return;
            string dimPath = CompactMachineUtils.GetPositionFolderPath(key.Path);
            string hashPath = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(),hash);
            GlobalHelper.CopyDirectory(dimPath,hashPath);
            Directory.Delete(dimPath,true);
            Debug.Log($"Removed system at path '{dimPath}' and saved at '{hashPath}'");
        }

        private void SaveTree(CompactMachineTree compactMachineTree)
        {
            if (compactMachineTree?.System == null) return;
            compactMachineTree.System.Save();
            foreach (var (position, child) in compactMachineTree.Children)
            {
                SaveTree(child);
            }
        }

        private void RemoveNode(CompactMachineTeleportKey key)
        {
            List<Vector2Int> path = key.Path;
            CompactMachineTree tree = systemTree;
            if (tree == null) return;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int position = path[i];
                tree = tree.Children[position];
            }
            Vector2Int lastPosition = path[^1];
            if (!tree.Children.Remove(lastPosition, out var removed)) return;
            RemoveFromList(removed);
        }

        private void RemoveFromList(CompactMachineTree tree)
        {
            // O(n*m)
            systems.Remove(tree.System);
            foreach (var (position, child) in tree.Children)
            {
                RemoveFromList(child);
            }
        }
        public bool HasSystem(CompactMachineTeleportKey key) {
            return systemTree.getSystem(key.Path) != null;
        }

        public bool IsLocked(List<Vector2Int> path)
        {
            CompactMachineTree tree = systemTree;
            foreach (Vector2Int position in path)
            {
                if (!tree.Children.TryGetValue(position, out var newTree)) return false;
                tree = newTree;
                string hash = tree.hash;
                if (hash == null) continue;
                CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
                if (metaData.Locked) return true;
                
            }
            return false;
        }
        public int GetSubSystems(CompactMachineTeleportKey key)
        {
            int count = 0;
            CompactMachineTree tree = systemTree.getTree(key.Path);
            return GetSubSystems(tree);
        }

        private int GetSubSystems(CompactMachineTree tree)
        {
            int count = 0;
            foreach (var (position, child) in tree.Children)
            {
                count += 1+GetSubSystems(child);
            }

            return count;
        }

        /// <summary>
        /// Creates a new system for a compact machine. Path should be ordered with depth 0 at 0, depth n at n.
        /// Null path means that 
        /// </summary>
        
        public void AddNewSystem(CompactMachineTeleportKey key, CompactMachineInstance compactMachine, string hash) {
            List<Vector2Int> systemPath = key.Path;
            List<Vector2Int> parentPath = new List<Vector2Int>();
            for (int i = 0; i < systemPath.Count-1; i++) {
                parentPath.Add(systemPath[i]);
            }
            Vector2Int placePosition = systemPath.Last();
            CompactMachineTree parentTree = systemTree.getTree(parentPath);
            if (CompactMachineUtils.HashExists(hash))
            {
                CompactMachineUtils.ActivateHashSystem(hash, systemPath);
            }
            else
            {
                CompactMachineUtils.InitalizeCompactMachineSystem(compactMachine, systemPath);
            }
            
            SoftLoadedClosedChunkSystem newSystem = CompactMachineUtils.LoadSystemFromPath(systemPath);
            
            CompactMachineTree newTree = new CompactMachineTree(newSystem, compactMachine.Hash);
            parentTree.Children[placePosition] = newTree;
            systems.Add(newSystem);
            string path = CompactMachineUtils.GetPositionFolderPath(systemPath);

            LoadCompactMachineSystem(compactMachine, newTree, path);
           
        }

        public ClosedChunkSystem ActivateSystem(IDimensionTeleportKey key, PlayerScript playerScript)
        {
            if (key is not CompactMachineTeleportKey compactMachineTeleportKey) {
                return null;
            }
            List<Vector2Int> path = compactMachineTeleportKey.Path;
            if (path.Count == 0) {
                return baseDimController.ActivateSystem(playerScript);
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
            area.Initialize(this, system.CoveredArea, 1, system,playerScript);
            return area;
        }

        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem, DimController baseDimController)
        {
            this.baseDimController =(ISingleSystemController) baseDimController;
            systemTree = new CompactMachineTree(baseSystem,null);
            LoadCompactMachineSystem(null,systemTree,WorldLoadUtils.GetDimPath(1));
            Debug.Log($"Loaded {systems.Count} Compact Machine Systems");
        }

        private class CompactMachineTree {
            public SoftLoadedClosedChunkSystem System;
            public string hash;
            public Dictionary<Vector2Int,CompactMachineTree> Children;

            public CompactMachineTree(SoftLoadedClosedChunkSystem system, string hash)
            {
                System = system;
                Children = new Dictionary<Vector2Int, CompactMachineTree>();
                this.hash = hash;
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

