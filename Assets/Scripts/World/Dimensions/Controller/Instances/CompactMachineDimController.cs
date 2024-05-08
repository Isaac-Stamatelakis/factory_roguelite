using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.IO;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule.PartitionModule;
using TileEntityModule.Instances.CompactMachines;
using WorldModule;

namespace Dimensions {
    public class CompactMachineDimController : DimController, IMultipleSystemController
    {
        private List<SoftLoadedClosedChunkSystem>[] systemsInRingDepth;

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

