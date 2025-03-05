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
        private CompactMachineClosedChunkSystem activeSystem;
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
            return activeSystem;
        }

        public List<SoftLoadedClosedChunkSystem> GetAllInactiveSystems()
        {
            return systems;
        }

        private void LoadCompactMachineSystem(CompactMachineTree tree, string path, bool blueprinted) {
            SoftLoadedClosedChunkSystem system = tree.System;
            foreach (IChunk chunk in system.Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            
                            switch (tileEntity)
                            {
                                case CompactMachineInstance nestedCompactMachine when nestedCompactMachine.IsActive:
                                {
                                    Vector2Int newPosition = nestedCompactMachine.GetCellPosition();
                                    string nestedPath = Path.Combine(path,$"{newPosition.x},{newPosition.y}");
                                    string contentPath = Path.Combine(nestedPath,CompactMachineUtils.CONTENT_PATH);
                                    SoftLoadedCompactMachineChunkSystem newSystem = CompactMachineUtils.LoadSystemFromPath(nestedCompactMachine,contentPath);
                                    if (newSystem == null) {
                                        Debug.LogError($"No system at path {nestedPath}");
                                        continue;
                                    }
                                    newSystem.SoftLoad();
                                    systems.Add(newSystem);
                                    CompactMachineTree newTree = new CompactMachineTree(newSystem,nestedCompactMachine);
                                    tree.Children[newPosition] = newTree;
                                    LoadCompactMachineSystem(newTree,nestedPath, blueprinted);
                                    break;
                                }
                                case IBluePrintModifiedTileEntity blueprintModifyTileEntity when blueprinted:
                                    if (blueprintModifyTileEntity is IBluePrintPlaceInitializedTileEntity placeInitializedTileEntity)
                                    {
                                        placeInitializedTileEntity.PlaceInitialize();
                                    }

                                    if (blueprintModifyTileEntity is IOnBluePrintActionTileEntity onBluePrintActionTileEntity)
                                    {
                                        onBluePrintActionTileEntity.OnBluePrint();
                                    } 
                                    break;
                                    
                            }
                        }
                    }
                }
            }
        }

        public void RemoveCompactMachineSystem(CompactMachineTeleportKey key, string hash)
        {
            CompactMachineTree tree = systemTree.GetTree(key.Path);
            SaveTree(tree);
            RemoveNode(key);
            if (hash == null) return;
            CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
            string dimPath = CompactMachineUtils.GetPositionFolderPath(key.Path);
            if (metaData.Instances <= 1)
            {
                string hashPath = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(),hash);
                GlobalHelper.CopyDirectory(dimPath,hashPath);
                Debug.Log($"Removed system at path '{dimPath}' and saved at '{hashPath}'");
            }
            else
            {
                Debug.Log($"Removed system at path '{dimPath}'");
            }
            
            Directory.Delete(dimPath,true);
            
        }



        public void SaveTree(List<Vector2Int> path)
        {
            SaveTree(systemTree.GetTree(path));
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
            return systemTree.GetSystem(key.Path) != null;
        }

        public bool IsLocked(List<Vector2Int> path)
        {
            CompactMachineTree tree = systemTree;
            foreach (Vector2Int position in path)
            {
                if (!tree.Children.TryGetValue(position, out var newTree)) return false;
                tree = newTree;
                string hash = tree.CompactMachineInstance?.Hash;
                if (hash == null) continue;
                CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
                if (metaData.Locked) return true;
            }
            return false;
        }
        public int GetSubSystems(CompactMachineTeleportKey key)
        {
            CompactMachineTree tree = systemTree.GetTree(key.Path);
            return GetSubSystems(tree);
        }

        private int GetSubSystems(CompactMachineTree tree)
        {
            if (tree == null) return 0;
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
        
        public void AddNewSystem(CompactMachineTeleportKey key, CompactMachineInstance compactMachine, string hash, bool blueprint) {
            List<Vector2Int> systemPath = key.Path;
            List<Vector2Int> parentPath = new List<Vector2Int>();
            for (int i = 0; i < systemPath.Count-1; i++) {
                parentPath.Add(systemPath[i]);
            }
            Vector2Int placePosition = systemPath.Last();
            CompactMachineTree parentTree = systemTree.GetTree(parentPath);
            
            if (CompactMachineUtils.HashExists(hash))
            {
                CompactMachineUtils.ActivateHashSystem(hash, systemPath);
            }
            else
            {
                CompactMachineUtils.InitalizeCompactMachineSystem(compactMachine, systemPath);
            }
            
            SoftLoadedCompactMachineChunkSystem newSystem = CompactMachineUtils.LoadSystemFromPath(compactMachine,systemPath);
            newSystem.SoftLoad();
            
            CompactMachineTree newTree = new CompactMachineTree(newSystem, compactMachine);
            parentTree.Children[placePosition] = newTree;
            systems.Add(newSystem);
            string path = CompactMachineUtils.GetPositionFolderPath(systemPath);

            LoadCompactMachineSystem(newTree, path, blueprint);
           
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
            if (activeSystem)
            {
                CompactMachineTeleportKey activeKey = activeSystem.GetCompactMachineKey();
                if (activeKey.IsEqualTo(compactMachineTeleportKey)) return activeSystem;
            }

            CompactMachineTree currentNode = systemTree.GetTree(path);
            SoftLoadedClosedChunkSystem system = currentNode.System;
            CompactMachineInstance compactMachineInstance = currentNode.CompactMachineInstance;

            bool error = false;
            
            if (system == null)
            {
                error = true;
                Debug.LogError($"Compact Machine System at path '{path}' is null");
            }

            if (compactMachineInstance == null)
            {
                error = true;
                Debug.LogError($"Compact Machine Tree at path '{path}' has no compact machine instance");
            }

            if (error) return null;
            
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name = "Compact Machine System";
            CompactMachineClosedChunkSystem area = closedChunkSystemObject.AddComponent<CompactMachineClosedChunkSystem>();
            area.SetCompactMachine(compactMachineInstance, compactMachineTeleportKey);
            area.transform.SetParent(transform,false);
            area.Initialize(this, system.CoveredArea, 1, system,playerScript);
            return area;
        }

        public void softLoadSystem(SoftLoadedClosedChunkSystem baseSystem, DimController baseDimController)
        {
            this.baseDimController =(ISingleSystemController) baseDimController;
            systemTree = new CompactMachineTree(baseSystem,null);
            LoadCompactMachineSystem(systemTree,WorldLoadUtils.GetDimPath(1),false);
            Debug.Log($"Loaded {systems.Count} Compact Machine Systems");
        }

        public void ReSyncConduitPorts(List<Vector2Int> path, CompactMachinePortType portType, ConduitType conduitType, Vector2Int breakCellPosition)
        {
            CompactMachineTree node = systemTree.GetTree(path);
            if (node.System is not ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem) return;
       
            var compactMachine = compactMachineClosedChunkSystem.GetCompactMachine();
            foreach (var chunk in node.System.Chunks)
            {
                foreach (var chunkPartition in chunk.Partitions)
                {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
                    {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            Vector2Int positionInPartition = new Vector2Int(x, y);
                            var tileEntity = chunkPartition.GetTileEntity(positionInPartition);
                            
                            if (tileEntity is not ICompactMachineConduitPort compactMachineConduitPort) continue;
                            Vector2Int cellPosition = chunkPartition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE + positionInPartition;
                            if (cellPosition == breakCellPosition) continue;
                            
                            if (compactMachineConduitPort.GetConduitType() != conduitType || compactMachineConduitPort.GetPortType() != portType) continue;
                            compactMachineConduitPort.SyncToCompactMachine(compactMachine);
                        }
                    }
                }
            }
        }

        public IChunkSystem GetSystem(List<Vector2Int> path)
        {
            CompactMachineTree node = systemTree.GetTree(path);
            return node.System;
        }
        
        

        private class CompactMachineTree {
            public SoftLoadedClosedChunkSystem System;
            public CompactMachineInstance CompactMachineInstance;
            public Dictionary<Vector2Int,CompactMachineTree> Children;

            public CompactMachineTree(SoftLoadedClosedChunkSystem system, CompactMachineInstance compactMachineInstance)
            {
                System = system;
                Children = new Dictionary<Vector2Int, CompactMachineTree>();
                this.CompactMachineInstance = compactMachineInstance;
            }
            public SoftLoadedCompactMachineChunkSystem GetSystem(List<Vector2Int> path, int depth=0)
            {
                if (depth == path.Count) {
                    return System as SoftLoadedCompactMachineChunkSystem;
                }
                Vector2Int key = path[depth];
                if (Children.ContainsKey(key)) {
                    return Children[key].GetSystem(path,depth+1);
                }
                return null;
            }
            
            public CompactMachineTree GetTree(List<Vector2Int> path, int depth=0) {
                if (depth == path.Count) {
                    return this;
                }
                Vector2Int key = path[depth];
                if (Children.ContainsKey(key)) {
                    return Children[key].GetTree(path,depth+1);
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

