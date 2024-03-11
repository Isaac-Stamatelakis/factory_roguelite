using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.IO;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule.PartitionModule;
using TileEntityModule.Instances.CompactMachines;
using WorldModule;

namespace DimensionModule {
    public class CompactMachineDimController : DimController, IMultipleSystemController
    {
        private List<SoftLoadedClosedChunkSystem>[] systemsInRingDepth;
        
        /*
        public void initalizeSystems() {
            if (!WorldCreation.dimExists(Global.WorldName,1)) {
                WorldCreation.createDimFolder(Global.WorldName,1);
            }
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(1);
            Debug.Log(name +  " Loaded " +  unloadedChunks.Count + " Chunks");
            List<SoftLoadedClosedChunkSystem> unsortedSystems = formSystems(unloadedChunks);
            Debug.Log(name + " Loaded " + unsortedSystems.Count + " Closed Chunk Systems");

            systemsInRingDepth = new List<SoftLoadedClosedChunkSystem>[CompactMachineHelper.MaxDepth+1];
            for (int i = 0; i < systemsInRingDepth.Length; i++) {
                systemsInRingDepth[i] = new List<SoftLoadedClosedChunkSystem>();
            }

            foreach (SoftLoadedClosedChunkSystem unsortedSystem in unsortedSystems) {
                Vector2Int center = unsortedSystem.getCenter();
                int depth = CompactMachineHelper.getDepth(center);
                if (depth < 0 || depth > CompactMachineHelper.MaxDepth) {
                    Debug.LogError("SoftLoadedClosedChunkSystem with center " + center + " and depth " + depth + " is out of bounds");
                    continue;
                }
                systemsInRingDepth[depth].Add(unsortedSystem);
            }
            string debugText = "";
            for (int i = 0; i < systemsInRingDepth.Length; i++) {
                debugText += "Ring" + i + " Loaded with " + systemsInRingDepth[i].Count + " Systems\n";
            }
            Debug.Log(name + " Rings from depth in range [0," + CompactMachineHelper.MaxDepth + "] Loaded\n" + debugText); 
        }
        private List<SoftLoadedClosedChunkSystem> formSystems(List<SoftLoadedConduitTileChunk> unloadedChunks) {
            List<SoftLoadedClosedChunkSystem> unsortedSystems =  new List<SoftLoadedClosedChunkSystem>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in unloadedChunks) {
                bool found = false;
                for (int i = unsortedSystems.Count-1; i >= 0; i--) {
                    SoftLoadedClosedChunkSystem inactiveClosedChunkSystem = unsortedSystems[i];
                    found = inactiveClosedChunkSystem.chunkIsNeighbor(unloadedChunk);
                    if (found) {
                        inactiveClosedChunkSystem.addChunk(unloadedChunk);
                        for (int j = unsortedSystems.Count-1; j >= 0; j--) {
                            if (i == j) {
                                continue;
                            }
                            if (!inactiveClosedChunkSystem.systemIsNeighbor(unsortedSystems[j])) {
                                continue;
                            }
                            inactiveClosedChunkSystem.merge(unsortedSystems[j]);
                            unsortedSystems.RemoveAt(j);
                            break;
                        }
                        break;
                    }
                }
                if (!found) {
                    unsortedSystems.Add(new SoftLoadedClosedChunkSystem(new List<SoftLoadedConduitTileChunk>{unloadedChunk}));
                }
            }
            return unsortedSystems;
        }
        */

        public void FixedUpdate() {
            if (systemsInRingDepth == null) {
                return;
            }
            foreach (List<SoftLoadedClosedChunkSystem> systems in systemsInRingDepth) {
                foreach (SoftLoadedClosedChunkSystem system in systems) {
                    system.tickUpdate();
                }
            }
        }

        public ClosedChunkSystem getSystemFromWorldPosition(Vector2 worldPosition)
        {
            Vector2Int cellPosition = Global.getCellPositionFromWorld(worldPosition);
            return getSystemFromCellPositon(cellPosition);
        }

        public bool hasSystemOfCompactMachine(CompactMachine compactMachine) {
            if (systemsInRingDepth == null) {
                return false;
            }
            return getSystem(compactMachine) != null;
        }

        public SoftLoadedClosedChunkSystem getSystem(CompactMachine compactMachine) {
            Vector2Int systemPosition = compactMachine.getCellPosition();
            int depth = CompactMachineHelper.getDepth(systemPosition);
            if (depth < 0 || depth > CompactMachineHelper.MaxDepth) {
                Debug.LogWarning("Attempted to check existence of compact machine + '" + compactMachine.name + "' with out of bounds depth:" + depth);
                return null;
            }
            List<SoftLoadedClosedChunkSystem> systems = systemsInRingDepth[depth];
            foreach (SoftLoadedClosedChunkSystem system in systems) {
                if (system.CoveredArea.contains(systemPosition)) {
                    return system;
                }
            }
            return null;
        }

        public ClosedChunkSystem getSystemFromCellPositon(Vector2Int cellPosition)
        {
            int depth = CompactMachineHelper.getDepth(cellPosition);
            if (depth < 0 || depth > CompactMachineHelper.MaxDepth) {
                Debug.LogError("Attempted to activate compact machine closed chunk system with depth " + depth + " which is out of bounds");
                return null;
            }
            List<SoftLoadedClosedChunkSystem> depthSystems = systemsInRingDepth[depth];
            foreach (SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem in depthSystems) {
                IntervalVector coveredArea = softLoadedClosedChunkSystem.CoveredArea;
                if (!coveredArea.contains(cellPosition)) {
                    continue;
                }
                GameObject closedChunkSystemObject = new GameObject();
                Vector2Int center = softLoadedClosedChunkSystem.getCenter();
                closedChunkSystemObject.name="Compact Machine [" + center.x + "," + center.y +"]";
                ConduitTileClosedChunkSystem area = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
                area.initalize(
                    transform,
                    coveredArea: coveredArea,
                    dim: 1,
                    softLoadedClosedChunkSystem
                );
                return area;
            }
            Debug.LogError("Could not find closed chunk system at cell position " + cellPosition);
            return null;
        }


        public void activateSystem(CompactMachine compactMachine) {
            SoftLoadedClosedChunkSystem system = InactiveClosedChunkFactory.importFromFolder(compactMachine.getCellPosition(),1);
            if (system == null) {
                CompactMachineHelper.initalizeCompactMachineSystem(compactMachine);
                system = InactiveClosedChunkFactory.importFromFolder(compactMachine.getCellPosition(),1);
            }
            if (system == null) {
                Debug.LogError("Could not loaded system for compact machine " + compactMachine.getName());
                return;
            }
            int depth = CompactMachineHelper.getDepth(compactMachine.getCellPosition());
            if (systemsInRingDepth == null) {
                systemsInRingDepth = new List<SoftLoadedClosedChunkSystem>[CompactMachineHelper.MaxDepth+1];
                for (int i = 0; i < systemsInRingDepth.Length; i++) {
                    systemsInRingDepth[i] = new List<SoftLoadedClosedChunkSystem>();
                }
            }
            systemsInRingDepth[depth].Add(system);
            system.softLoad();
            system.syncToCompactMachine(compactMachine);
            return;
        }

        public void OnDestroy() {
            if (systemsInRingDepth == null) {
                return;
            }
            foreach (List<SoftLoadedClosedChunkSystem> systems in systemsInRingDepth) {
                foreach (SoftLoadedClosedChunkSystem system in systems) {
                    system.save();
                }
            }
        }
    }
}

