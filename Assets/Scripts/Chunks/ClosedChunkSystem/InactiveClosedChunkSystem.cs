using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ConduitModule.ConduitSystemModule;
using TileMapModule.Type;
using ConduitModule;
using TileEntityModule;
using ConduitModule.Ports;
using ChunkModule.PartitionModule;

namespace ChunkModule.ClosedChunkSystemModule {
    public class SoftLoadedClosedChunkSystem
    {
        private IntervalVector coveredArea;
        private Dictionary<TileMapType, ConduitSystemManager> conduitSystemManagersDict; 
        private List<SoftLoadedConduitTileChunk> softLoadedChunk;
        public SoftLoadedClosedChunkSystem(List<SoftLoadedConduitTileChunk> unloadedChunks) {
            this.softLoadedChunk = unloadedChunks;
            if (unloadedChunks.Count == 0) {
                Debug.LogError("Empty list provided to soft loaded chunk system constructor");
                return;
            }
            
            SoftLoadedConduitTileChunk initalChunk = unloadedChunks[0];
            int x = initalChunk.getPosition().x;
            int y = initalChunk.getPosition().y;
            this.coveredArea = new IntervalVector(new Interval<int>(x,x), new Interval<int>(y,y));
            for (int i = 1; i < unloadedChunks.Count; i++) {
                updateCoveredArea(unloadedChunks[i]);
            }
            
        }

        public List<SoftLoadedConduitTileChunk> UnloadedChunks { get => softLoadedChunk; set => softLoadedChunk = value; }
        public Dictionary<TileMapType, ConduitSystemManager> ConduitSystemManagersDict { get => conduitSystemManagersDict; set => conduitSystemManagersDict = value; }
        public IntervalVector CoveredArea { get => coveredArea; set => coveredArea = value; }

        public bool chunkIsNeighbor(SoftLoadedConduitTileChunk unloadedChunk) {
            foreach (SoftLoadedConduitTileChunk containedChunk in softLoadedChunk) {
                Vector2Int dif = containedChunk.getPosition() - unloadedChunk.getPosition();
                if (Mathf.Abs(dif.x) <= 1 && Mathf.Abs(dif.y) <= 1) {
                    return true;
                }
            }
            return false;
        }
        public bool systemIsNeighbor(SoftLoadedClosedChunkSystem inactiveClosedChunkSystem) {
            foreach (SoftLoadedConduitTileChunk neighborChunk in inactiveClosedChunkSystem.UnloadedChunks) {
                if (chunkIsNeighbor(neighborChunk)) {
                    return true;
                }
            }
            return false;
        }
        public void merge(SoftLoadedClosedChunkSystem inactiveClosedChunkSystem) {
            this.softLoadedChunk.AddRange(inactiveClosedChunkSystem.UnloadedChunks);
            IntervalVector toMergeArea = inactiveClosedChunkSystem.coveredArea;
            if (toMergeArea.X.UpperBound > coveredArea.X.UpperBound) {
                coveredArea.X.UpperBound = toMergeArea.X.UpperBound;
            } else if (toMergeArea.X.LowerBound < coveredArea.X.LowerBound) {
                coveredArea.X.LowerBound = toMergeArea.X.LowerBound;
            }

            if (toMergeArea.Y.UpperBound > coveredArea.Y.UpperBound) {
                coveredArea.Y.UpperBound = toMergeArea.Y.UpperBound;
            } else if (toMergeArea.Y.LowerBound < coveredArea.Y.LowerBound) {
                coveredArea.Y.LowerBound = toMergeArea.Y.LowerBound;
            }
        }

        public void addChunk(SoftLoadedConduitTileChunk chunk) {
            this.UnloadedChunks.Add(chunk);
            updateCoveredArea(chunk);
        }

        private void updateCoveredArea(SoftLoadedConduitTileChunk chunk) {
            Vector2Int newChunkPosition = chunk.getPosition();
            if (newChunkPosition.x > coveredArea.X.UpperBound) {
                coveredArea.X.UpperBound = newChunkPosition.x;
            } else if (newChunkPosition.x < coveredArea.X.LowerBound) {
                coveredArea.X.LowerBound = newChunkPosition.x;
            }

            if (newChunkPosition.y > coveredArea.Y.UpperBound) {
                coveredArea.Y.UpperBound = newChunkPosition.y;
            } else if (newChunkPosition.y < coveredArea.Y.LowerBound) {
                coveredArea.Y.LowerBound = newChunkPosition.y;
            }
        }

        public void softLoad() {
            //Debug.Log(coveredArea.X.LowerBound + "," + coveredArea.X.UpperBound + "|" + coveredArea.Y.LowerBound + "," + coveredArea.Y.UpperBound); 
            loadTickableTileEntities();
            initConduitSystemManagers();
        }
        private void initConduitSystemManagers() {
            conduitSystemManagersDict = new Dictionary<TileMapType, ConduitSystemManager>();
            initConduitSystemManager(TileMapType.ItemConduit);
            initConduitSystemManager(TileMapType.FluidConduit);
            initConduitSystemManager(TileMapType.EnergyConduit);
            initConduitSystemManager(TileMapType.SignalConduit);
        }

        private void initConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            ConduitSystemManager manager = new ConduitSystemManager(
                conduitType: conduitType,
                conduits: getConduits(conduitType),
                size: getSize(),
                chunkConduitPorts: getTileEntityPorts(conduitType),
                referencePosition: getBottomLeftCorner()
            );
            conduitSystemManagersDict[conduitMapType] = manager;
        }
        /// <summary>
        /// Returns a list of spots conduits can connect to tile entities of each chunk
        /// </summary>
        private Dictionary<TileEntity, List<TileEntityPort>> getTileEntityPorts(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = getBottomLeftCorner();
            Dictionary<TileEntity, List<TileEntityPort>> tileEntityPortData = new Dictionary<TileEntity, List<TileEntityPort>>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunk) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    Dictionary<TileEntity, List<TileEntityPort>> partitionPorts = ((IConduitTileChunkPartition) partition).getEntityPorts(conduitType,chunkFrameOfReference);
                    foreach (KeyValuePair<TileEntity, List<TileEntityPort>> kvp in partitionPorts) {
                        tileEntityPortData[kvp.Key] = kvp.Value;
                    }
                }
            }
            return tileEntityPortData;
            
        }
        private IConduit[,] getConduits(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = getBottomLeftCorner();
            IConduit[,] conduits = new IConduit[size.x,size.y];
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunk) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    ((IConduitTileChunkPartition) partition).getConduits(conduitType,conduits,chunkFrameOfReference);
                }
            }
            return conduits;
        }
        public Vector2Int getBottomLeftCorner() {
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
        }
        protected Vector2Int getSize() {
            int xSizeChunks = Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound)+1;
            int ySizeChunks = Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound)+1;
            return new Vector2Int(xSizeChunks*Global.ChunkSize,ySizeChunks*Global.ChunkSize);
        }

        public Vector2Int getCenter() {
            return new Vector2Int((coveredArea.X.UpperBound+coveredArea.X.LowerBound)/2,(coveredArea.Y.UpperBound+coveredArea.Y.LowerBound)/2);
        }
        private void loadTickableTileEntities() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunk) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                    }
                    ((IConduitTileChunkPartition) partition).softLoadTileEntities();
                }
            }
        }
    }
}

