using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Systems;
using TileMaps.Type;
using Conduits;
using TileEntityModule;
using Conduits.Ports;
using Chunks.Partitions;
using TileEntityModule.Instances.CompactMachines;
using Chunks.IO;
using TileMaps.Layer;

namespace Chunks.Systems {
    public class SoftLoadedClosedChunkSystem
    {
        private IntervalVector coveredArea;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict; 
        private List<SoftLoadedConduitTileChunk> softLoadedChunks;
        private string savePath;
        public SoftLoadedClosedChunkSystem(List<SoftLoadedConduitTileChunk> unloadedChunks, string savePath) {
            this.softLoadedChunks = unloadedChunks;
            this.savePath = savePath;
            if (unloadedChunks.Count == 0) {
                return;
            }
            for (int i = 0; i < unloadedChunks.Count; i++) {
                updateCoveredArea(unloadedChunks[i]);
                Chunks[i].System = this;
            } 
        }

        public List<SoftLoadedConduitTileChunk> Chunks { get => softLoadedChunks; set => softLoadedChunks = value; }
        public Dictionary<TileMapType, IConduitSystemManager> ConduitSystemManagersDict { get => conduitSystemManagersDict; set => conduitSystemManagersDict = value; }
        public IntervalVector CoveredArea { get => coveredArea; set => coveredArea = value; }

        public bool chunkIsNeighbor(SoftLoadedConduitTileChunk unloadedChunk) {
            foreach (SoftLoadedConduitTileChunk containedChunk in softLoadedChunks) {
                Vector2Int dif = containedChunk.getPosition() - unloadedChunk.getPosition();
                if (Mathf.Abs(dif.x) <= 1 && Mathf.Abs(dif.y) <= 1) {
                    return true;
                }
            }
            return false;
        }
        public bool systemIsNeighbor(SoftLoadedClosedChunkSystem inactiveClosedChunkSystem) {
            foreach (SoftLoadedConduitTileChunk neighborChunk in inactiveClosedChunkSystem.Chunks) {
                if (chunkIsNeighbor(neighborChunk)) {
                    return true;
                }
            }
            return false;
        }
        public void merge(SoftLoadedClosedChunkSystem inactiveClosedChunkSystem) {
            this.softLoadedChunks.AddRange(inactiveClosedChunkSystem.Chunks);
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
            this.Chunks.Add(chunk);
            chunk.System = this;
            updateCoveredArea(chunk);
        }

        private void updateCoveredArea(SoftLoadedConduitTileChunk chunk) {
            if (coveredArea == null) {
                int x = chunk.getPosition().x;
                int y = chunk.getPosition().y;
                this.coveredArea = new IntervalVector(new Interval<int>(x,x), new Interval<int>(y,y));
                return;
            }
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
            softLoadTileEntities();
            assembleMultiBlocks();
            initConduitSystemManagers();
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine) {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                    }
                    ((IConduitTileChunkPartition) partition).syncToCompactMachine(compactMachine);
                }
            }
        }
        private void initConduitSystemManagers() {
            conduitSystemManagersDict = new Dictionary<TileMapType, IConduitSystemManager>();
            initConduitSystemManager(TileMapType.ItemConduit);
            initConduitSystemManager(TileMapType.FluidConduit);
            initConduitSystemManager(TileMapType.EnergyConduit);
            initConduitSystemManager(TileMapType.SignalConduit);
            initConduitSystemManager(TileMapType.MatrixConduit);
        }

        private void initConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPorts = getTileEntityPorts(conduitType);
            IConduitSystemManager manager = ConduitSystemManagerFactory.createManager(
                conduitType: conduitType,
                conduits: getConduits(conduitType,tileEntityPorts),
                size: getSize(),
                chunkConduitPorts: tileEntityPorts,
                referencePosition: getBottomLeftCorner()
            );
            conduitSystemManagersDict[conduitMapType] = manager;
        }
        /// <summary>
        /// Returns a list of spots conduits can connect to tile entities of each chunk
        /// </summary>
        private Dictionary<ITileEntityInstance, List<TileEntityPort>> getTileEntityPorts(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = getBottomLeftCorner();
            Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPortData = new Dictionary<ITileEntityInstance, List<TileEntityPort>>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunks) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    Dictionary<ITileEntityInstance, List<TileEntityPort>> partitionPorts = ((IConduitTileChunkPartition) partition).getEntityPorts(conduitType,chunkFrameOfReference);
                    foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in partitionPorts) {
                        tileEntityPortData[kvp.Key] = kvp.Value;
                    }
                }
            }
            return tileEntityPortData;
            
        }

        /// <summary>
        /// Returns the tileEntity at a given cellPosition
        /// </summary>
        public ITileEntityInstance getSoftLoadedTileEntity(Vector2Int cellPosition) {
            Vector2Int? tilePosition = FindTileAtLocation.find(cellPosition,this);
            if (tilePosition == null) {
                return null;
            }
            return GetTileEntity((Vector2Int)tilePosition);
            
        }


        public TileItem getTileItem(Vector2Int currentCellPosition, Dictionary<Vector2Int, IChunk> chunkCache, Dictionary<Vector2Int, IChunkPartition> partitionCache, TileMapLayer layer) {
            Vector2Int chunkPosition = Global.getChunkFromCell(currentCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(currentCellPosition);
            if (!chunkCache.ContainsKey(chunkPosition)) {
                chunkCache[chunkPosition] = getChunk(chunkPosition);
            }
            IChunk chunk = chunkCache[chunkPosition];
            if (chunk == null) {
                return null;
            }
            if (!partitionCache.ContainsKey(partitionPosition)) {
                Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PartitionsPerChunk;
                partitionCache[partitionPosition] = chunk.getPartition(adjustedPartitionPosition);
            }
            IChunkPartition partition = partitionCache[partitionPosition];
            Vector2Int cellPositionInPartition = Global.getPositionInPartition(currentCellPosition);
            TileItem tileItem = partition.GetTileItem(cellPositionInPartition,layer);
            return tileItem;
        }

        public ITileEntityInstance GetTileEntity(Vector2Int currentCellPosition) {
            Vector2Int chunkPosition = Global.getChunkFromCell(currentCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(currentCellPosition);
            IChunk chunk = getChunk(chunkPosition);
            if (chunk == null) {
                return null;
            }
            Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PartitionsPerChunk;
            IChunkPartition partition = chunk.getPartition(adjustedPartitionPosition);
            Vector2Int cellPositionInPartition = Global.getPositionInPartition(currentCellPosition);
            return partition.GetTileEntity(cellPositionInPartition);
        }

        public SoftLoadedConduitTileChunk getChunk(Vector2Int cellPosition) {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                if (chunk.getPosition().Equals(cellPosition)) {
                    return chunk;
                }
            }
            return null;
        }
        private IConduit[,] getConduits(ConduitType conduitType,Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPorts) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = getBottomLeftCorner();
            IConduit[,] conduits = new IConduit[size.x,size.y];
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunks) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    ((IConduitTileChunkPartition) partition).getConduits(conduitType,conduits,chunkFrameOfReference,tileEntityPorts);
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
        private void softLoadTileEntities() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                    }
                    ((IConduitTileChunkPartition) partition).softLoadTileEntities();
                }
            }
        }

        private void assembleMultiBlocks() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                    }
                    ((IConduitTileChunkPartition) partition).assembleMultiBlocks();
                }
            }
        }

        public void save() {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.getChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogWarning("Non conduit partition in soft loaded tile chunk");
                        continue;
                    }
                    Dictionary<ConduitType, IConduit[,]> partitionConduits = new Dictionary<ConduitType, IConduit[,]>();
                    foreach (KeyValuePair<TileMapType,IConduitSystemManager> kvp in conduitSystemManagersDict) {
                        IConduitSystemManager manager = kvp.Value;
                        if (manager is PortConduitSystemManager portConduitSystemManager) {
                            partitionConduits[kvp.Key.toConduitType()] = portConduitSystemManager.getConduitPartitionData(partition.getRealPosition());
                        }
                        
                    }
                    conduitTileChunkPartition.setConduits(partitionConduits);
                    partition.save();
                }
                ChunkIO.writeChunk(chunk,path:savePath,directory:true);
            }
        }

        public void tickUpdate() {
            foreach (IConduitSystemManager manager in conduitSystemManagersDict.Values) {
                if (manager is ITickableConduitSystem tickableConduitSystem) {
                    tickableConduitSystem.tickUpdate();
                }
            }
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    partition.tick();
                }
            }
        }
    }
}

