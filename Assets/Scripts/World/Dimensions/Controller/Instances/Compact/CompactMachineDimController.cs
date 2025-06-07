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
        public void SoftLoadSystem(ClosedChunkSystemAssembler baseSystemAssembler,DimController dimController);
    }
    

    public class CompactMachineDimController : DimController, IMultipleSystemController, ICompactMachineDimension
    {
        private ISingleSystemController baseDimController;
        private CompactMachineTree<SoftLoadedClosedChunkSystem> systemTree;
        
        private readonly List<SoftLoadedClosedChunkSystem> softLoadedClosedChunkSystems = new();
        private readonly CompactMachineUpdateMap compactMachineUpdateMap = new();
        private CompactMachineClosedChunkSystem activeSystem;
        private List<Vector2Int> currentSystemPath;
        public List<Vector2Int> CurrentSystemPath => currentSystemPath;
        private uint tickCounter;
        private const uint ACTIVE_SYSTEM_TICK_OFFSET = 1;
        
        public override ClosedChunkSystem GetActiveSystem()
        {
            return activeSystem;
        }

        public List<IChunkSystem> GetAllSystems()
        {
            List<IChunkSystem> systems = new List<IChunkSystem>();
            foreach (SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem in softLoadedClosedChunkSystems)
            {
                systems.Add(softLoadedClosedChunkSystem);
            }

            if (activeSystem)
            {
                systems.Add(activeSystem);
            }
            return systems;
        }

        private void LoadCompactMachineSystem(CompactMachineTree<ClosedChunkSystemAssembler> tree, string path, bool blueprinted, int depth) {
            ClosedChunkSystemAssembler systemAssembler = tree.Data;
            foreach (IChunk chunk in systemAssembler.Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            ITileEntityInstance tileEntity = partition.GetTileEntity(new Vector2Int(x,y));
                            
                            switch (tileEntity)
                            {
                                case CompactMachineInstance nestedCompactMachine when nestedCompactMachine.IsActive:
                                {
                                    nestedCompactMachine.Depth = depth + 1;
                                    Vector2Int newPosition = nestedCompactMachine.GetCellPosition();
                                    string nestedPath = Path.Combine(path,$"{newPosition.x},{newPosition.y}");
                                    string contentPath = Path.Combine(nestedPath,CompactMachineUtils.CONTENT_PATH);
                                    CompactMachineChunkSystemAssembler newSystemAssembler = CompactMachineUtils.LoadSystemFromPath(nestedCompactMachine,contentPath);
                                    if (newSystemAssembler == null) {
                                        Debug.LogError($"No system at path {nestedPath}");
                                        continue;
                                    }
                                    newSystemAssembler.LoadSystem();
                                    CompactMachineTree<ClosedChunkSystemAssembler> newTree = new CompactMachineTree<ClosedChunkSystemAssembler>(newSystemAssembler,nestedCompactMachine);
                                    tree.Children[newPosition] = newTree;
                                    LoadCompactMachineSystem(newTree,nestedPath, blueprinted,depth+1);
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
            CompactMachineTree<SoftLoadedClosedChunkSystem> tree = systemTree.GetTree(key.Path);
            SaveTree(tree);
            RemoveNode(key);
            if (hash == null) return;
            CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
            string dimPath = CompactMachineUtils.GetPositionFolderPath(key.Path);
            if (!Directory.Exists(dimPath))
            {
                Debug.LogError($"Tried to remove system at path '{dimPath}' which doesn't exist.");
                return;
            }
            if (metaData.Instances <= 1)
            {
                string hashPath = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(),hash);
                if (!Directory.Exists(hashPath))
                {
                    Debug.LogError($"Tried to remove system at hash path '{hashPath}' which doesn't exist.");
                    return;
                }

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
        private void SaveTree(CompactMachineTree<SoftLoadedClosedChunkSystem> compactMachineTree)
        {
            if (compactMachineTree?.Data == null) return;
            compactMachineTree.Data.Save();
            foreach (var (position, child) in compactMachineTree.Children)
            {
                SaveTree(child);
            }
        }

        private void RemoveNode(CompactMachineTeleportKey key)
        {
            List<Vector2Int> path = key.Path;
            CompactMachineTree<SoftLoadedClosedChunkSystem> tree = systemTree;
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

        private void RemoveFromList(CompactMachineTree<SoftLoadedClosedChunkSystem> tree)
        {
            // O(n*m)
            softLoadedClosedChunkSystems.Remove(tree.Data);
            compactMachineUpdateMap.RemoveSoftLoadedSystem(tree.Data);
            foreach (var (position, child) in tree.Children)
            {
                RemoveFromList(child);
            }
        }
        public bool HasSystem(CompactMachineTeleportKey key) {
            return systemTree.GetData(key.Path) != null;
        }

        public bool IsLocked(List<Vector2Int> path)
        {
            CompactMachineTree<SoftLoadedClosedChunkSystem> tree = systemTree;
            foreach (Vector2Int position in path)
            {
                if (!tree.Children.TryGetValue(position, out var newTree)) return false;
                tree = newTree;
                string hash = tree.CompactMachineInstance?.Hash;
                if (hash == null) continue;
                CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
                if (metaData == null) return true;
                if (metaData.Locked) return true;
            }
            return false;
        }
        public int GetSubSystems(CompactMachineTeleportKey key)
        {
            CompactMachineTree<SoftLoadedClosedChunkSystem> tree = systemTree.GetTree(key.Path);
            return GetSubSystems(tree);
        }

        private int GetSubSystems(CompactMachineTree<SoftLoadedClosedChunkSystem> tree)
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
            CompactMachineTree<SoftLoadedClosedChunkSystem> parentTree = systemTree.GetTree(parentPath);
            
            if (CompactMachineUtils.HashExists(hash))
            {
                CompactMachineUtils.ActivateHashSystem(hash, systemPath);
            }
            else
            {
                CompactMachineUtils.InitalizeCompactMachineSystem(compactMachine, systemPath);
            }
            
            CompactMachineChunkSystemAssembler newSystemAssembler = CompactMachineUtils.LoadSystemFromPath(compactMachine,systemPath);
            newSystemAssembler.LoadSystem();
            
            CompactMachineTree<ClosedChunkSystemAssembler> newAssemblerTree = new CompactMachineTree<ClosedChunkSystemAssembler>(newSystemAssembler, compactMachine);
            
            
            string path = CompactMachineUtils.GetPositionFolderPath(systemPath);
    
            LoadCompactMachineSystem(newAssemblerTree, path, blueprint,systemPath.Count);

            var newTree = SoftLoadTree(newAssemblerTree);
            parentTree.Children[placePosition] = newTree;
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

            CompactMachineTree<SoftLoadedClosedChunkSystem> currentNode = systemTree.GetTree(path);
            SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = currentNode.Data;
            CompactMachineInstance compactMachineInstance = currentNode.CompactMachineInstance;
            string currentPath = softLoadedClosedChunkSystem.SavePath;
            
            bool error = false;
            
            if (compactMachineInstance == null)
            {
                error = true;
                Debug.LogError($"Compact Machine Tree at path '{path}' has no compact machine instance");
            }

            if (error) return null;
            
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,currentPath);
            ClosedChunkSystemAssembler systemAssembler = new ClosedChunkSystemAssembler(unloadedChunks,currentPath,0);
            systemAssembler.LoadSystem(softLoadedClosedChunkSystem.GetSoftLoadableTileEntities(),false);
            
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name = "Compact Machine System";
            CompactMachineClosedChunkSystem area = closedChunkSystemObject.AddComponent<CompactMachineClosedChunkSystem>();
            area.SetCompactMachine(compactMachineInstance, compactMachineTeleportKey);
            area.transform.SetParent(transform,false);
            area.Initialize(this, systemAssembler.CoveredArea, 1, systemAssembler,playerScript);
            currentSystemPath = path;
            activeSystem = area;
            softLoadedClosedChunkSystems.Remove(softLoadedClosedChunkSystem);
            compactMachineUpdateMap.RemoveSoftLoadedSystem(softLoadedClosedChunkSystem);
            currentNode.Data = null;
            return area;
        }

        public void SoftLoadSystem(ClosedChunkSystemAssembler baseSystemAssembler, DimController baseDimController)
        {
            this.baseDimController =(ISingleSystemController) baseDimController;
            var assemblerTree = new CompactMachineTree<ClosedChunkSystemAssembler>(baseSystemAssembler,null);
            LoadCompactMachineSystem(assemblerTree,WorldLoadUtils.GetDimPath(1),false,0);
            systemTree = SoftLoadTree(assemblerTree);
            
            // Remove dim0 data from tree
            softLoadedClosedChunkSystems.Remove(systemTree.Data);
            compactMachineUpdateMap.RemoveSoftLoadedSystem(systemTree.Data);
            systemTree.Data = null;
            Debug.Log($"Loaded {softLoadedClosedChunkSystems.Count} Compact Machine Systems");
        }

        public void ReSyncConduitPorts(List<Vector2Int> path, CompactMachinePortType portType, ConduitType conduitType, Vector2Int breakCellPosition)
        {
            activeSystem?.ReSyncCompactMachinePorts(breakCellPosition,conduitType,portType);
        }

        private CompactMachineTree<SoftLoadedClosedChunkSystem> SoftLoadTree(CompactMachineTree<ClosedChunkSystemAssembler> assemberTree)
        {
            SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = assemberTree.Data.ToSoftLoaded();
            CompactMachineTree<SoftLoadedClosedChunkSystem> tree = new CompactMachineTree<SoftLoadedClosedChunkSystem>(softLoadedClosedChunkSystem, assemberTree.CompactMachineInstance);
            softLoadedClosedChunkSystems.Add(softLoadedClosedChunkSystem);
            compactMachineUpdateMap.AddSoftLoadedSystem(softLoadedClosedChunkSystem);
            foreach (var (position, child) in assemberTree.Children)
            {
                tree.Children[position] = SoftLoadTree(child);
            }
            return tree;
        }
        

        private class CompactMachineTree<T>
        {
            public T Data;
            public Dictionary<Vector2Int, CompactMachineTree<T>> Children;
            public CompactMachineInstance CompactMachineInstance;

            public CompactMachineTree(T data, CompactMachineInstance compactMachineInstance)
            {
                Data = data;
                Children = new Dictionary<Vector2Int, CompactMachineTree<T>>();
                this.CompactMachineInstance = compactMachineInstance;
            }

            public T GetData(List<Vector2Int> path, int depth = 0)
            {
                if (depth == path.Count)
                {
                    return Data;
                }

                Vector2Int key = path[depth];
                if (Children.ContainsKey(key))
                {
                    return Children[key].GetData(path, depth + 1);
                }

                return default;
            }

            public CompactMachineTree<T> GetTree(List<Vector2Int> path, int depth = 0)
            {
                if (depth == path.Count)
                {
                    return this;
                }

                Vector2Int key = path[depth];
                if (Children.ContainsKey(key))
                {
                    return Children[key].GetTree(path, depth + 1);
                }

                return null;
            }

            public override string ToString()
            {
                return ToString(0);
            }

            private string ToString(int depth)
            {
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

        public override void TickUpdate()
        {
            compactMachineUpdateMap.TickUpdate(tickCounter);
            
            uint activeSystemCounter = tickCounter + ACTIVE_SYSTEM_TICK_OFFSET;
            if (activeSystemCounter % Global.CONDUIT_TICK_RATE == 0)
            {
                activeSystem?.TileEntityTickUpdate();
            }

            if (activeSystemCounter % Global.CONDUIT_TICK_RATE == 0)
            {
                activeSystem?.ConduitTickUpdate();
            }
            tickCounter++;
        }

        public override void DeActivateSystem()
        {
            if (activeSystem is not ConduitClosedChunkSystem closedChunkSystem) return;
            var softLoadedSystem = closedChunkSystem.ToSoftLoadedSystem();
            softLoadedSystem.ClearActiveComponents();
            this.softLoadedClosedChunkSystems.Add(softLoadedSystem);
            compactMachineUpdateMap.AddSoftLoadedSystem(softLoadedSystem);
            
            var currentTree = systemTree.GetTree(currentSystemPath);
            currentTree.Data = softLoadedSystem;
            GameObject.Destroy(closedChunkSystem.gameObject);
            activeSystem = null;
        }

        private class CompactMachineUpdateMap
        {
            private uint currentTickOffset; // Can start at 0 since dim0 system will always be the first in
            private const int MAX_INDEX = 50; // Have to change this if every change fixed update rate. Cannot hard code it cause unity complains when calling Time.fixedDeltaTime in static constructor
            private readonly Dictionary<uint, List<SoftLoadedClosedChunkSystem>> tileEntityTickSystemListDict = new();
            private readonly Dictionary<uint, List<SoftLoadedClosedChunkSystem>> conduitTickSystemListDict = new();

            public void AddSoftLoadedSystem(SoftLoadedClosedChunkSystem system)
            {
                uint tileEntityTick = currentTickOffset % Global.TILE_ENTITY_TICK_RATE;
                if (!tileEntityTickSystemListDict.ContainsKey(tileEntityTick))
                {
                    tileEntityTickSystemListDict.Add(tileEntityTick, new List<SoftLoadedClosedChunkSystem>());
                }
                tileEntityTickSystemListDict[tileEntityTick].Add(system);
                
                
                uint conduitEntityTick = currentTickOffset % Global.CONDUIT_TICK_RATE;
                if (!conduitTickSystemListDict.ContainsKey(conduitEntityTick))
                {
                    conduitTickSystemListDict.Add(conduitEntityTick, new List<SoftLoadedClosedChunkSystem>());
                }
                conduitTickSystemListDict[conduitEntityTick].Add(system);
                
                system.SetTickOffset(currentTickOffset);
                currentTickOffset++;
                
                // Avoid putting at the same offset as dim0 system which is when current % max is 0
                if (currentTickOffset % MAX_INDEX == 0) currentTickOffset++;
            }

            public void RemoveSoftLoadedSystem(SoftLoadedClosedChunkSystem system)
            {
                uint tileEntityTick = system.TickOffset % Global.TILE_ENTITY_TICK_RATE;
                tileEntityTickSystemListDict[tileEntityTick].Remove(system);
                if (tileEntityTickSystemListDict[tileEntityTick].Count == 0) 
                {
                    tileEntityTickSystemListDict.Remove(tileEntityTick);
                }
                
                uint conduitEntityTick = system.TickOffset % Global.CONDUIT_TICK_RATE;
                conduitTickSystemListDict[conduitEntityTick].Remove(system);
                if (conduitTickSystemListDict[conduitEntityTick].Count == 0) 
                {
                    conduitTickSystemListDict.Remove(conduitEntityTick);
                }
            }

            public void TickUpdate(uint counter)
            {
                uint tileEntityTick = counter % Global.TILE_ENTITY_TICK_RATE;
                if (tileEntityTickSystemListDict.TryGetValue(tileEntityTick, out var tileEntityUpdateSystems))
                {
                    foreach (var system in tileEntityUpdateSystems)
                    {
                        system.TileEntityTickUpdate();
                    }
                }
                
                uint conduitTick = counter % Global.CONDUIT_TICK_RATE;
                if (conduitTickSystemListDict.TryGetValue(conduitTick, out var conduitUpdateSystems))
                {
                    foreach (var system in conduitUpdateSystems)
                    {
                        system.ConduitTickUpdate();
                    }
                }
            }
        }
    }
}

