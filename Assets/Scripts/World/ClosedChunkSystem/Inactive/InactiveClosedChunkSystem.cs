using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Systems;
using TileMaps.Type;
using Conduits;
using TileEntity;
using Conduits.Ports;
using Chunks.Partitions;
using TileEntity.Instances.CompactMachines;
using Chunks.IO;
using TileMaps.Layer;
using Tiles;

namespace Chunks.Systems {
    public class SoftLoadedClosedChunkSystem : IChunkSystem
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
                Vector2Int dif = containedChunk.GetPosition() - unloadedChunk.GetPosition();
                if (Mathf.Abs(dif.x) <= 1 && Mathf.Abs(dif.y) <= 1) {
                    return true;
                }
            }
            return false;
        }

        private void updateCoveredArea(SoftLoadedConduitTileChunk chunk) {
            if (coveredArea == null) {
                int x = chunk.GetPosition().x;
                int y = chunk.GetPosition().y;
                this.coveredArea = new IntervalVector(new Interval<int>(x,x), new Interval<int>(y,y));
                return;
            }
            Vector2Int newChunkPosition = chunk.GetPosition();
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

        public void SoftLoad() {
            SoftLoadTileEntities();
            AssembleMultiBlocks();
            InitConduitSystemManagers();
        }
        
        private void InitConduitSystemManagers() {
            conduitSystemManagersDict = new Dictionary<TileMapType, IConduitSystemManager>();
            InitConduitSystemManager(TileMapType.ItemConduit);
            InitConduitSystemManager(TileMapType.FluidConduit);
            InitConduitSystemManager(TileMapType.EnergyConduit);
            InitConduitSystemManager(TileMapType.SignalConduit);
            InitConduitSystemManager(TileMapType.MatrixConduit);
        }

        private void InitConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts = ConduitPortFactory.GetTileEntityPorts(conduitType,softLoadedChunks);
            Dictionary<Vector2Int, IConduit> conduits = GetConduits(conduitType, tileEntityPorts);
            IConduitSystemManager manager = ConduitSystemManagerFactory.CreateManager(
                conduitType: conduitType,
                conduits: conduits,
                chunkConduitPorts: tileEntityPorts
            );
            conduitSystemManagersDict[conduitMapType] = manager;
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
                Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PARTITIONS_PER_CHUNK;
                partitionCache[partitionPosition] = chunk.GetPartition(adjustedPartitionPosition);
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
            Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = chunk.GetPartition(adjustedPartitionPosition);
            Vector2Int cellPositionInPartition = Global.getPositionInPartition(currentCellPosition);
            return partition.GetTileEntity(cellPositionInPartition);
        }

        public SoftLoadedConduitTileChunk getChunk(Vector2Int cellPosition) {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                if (chunk.GetPosition().Equals(cellPosition)) {
                    return chunk;
                }
            }
            return null;
        }
        private Dictionary<Vector2Int, IConduit> GetConduits(ConduitType conduitType,Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts) {
            Vector2Int chunkFrameOfReference = GetBottomLeftCorner();
            Dictionary<Vector2Int, IConduit> conduits = new Dictionary<Vector2Int, IConduit>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunks) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition chunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    chunkPartition.GetConduits(conduitType,conduits,chunkFrameOfReference,tileEntityPorts);
                }
            }
            return conduits;
        }
        public Vector2Int GetBottomLeftCorner() {
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.CHUNK_SIZE;
        }
        protected Vector2Int GetSize() {
            int xSizeChunks = Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound)+1;
            int ySizeChunks = Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound)+1;
            return new Vector2Int(xSizeChunks*Global.CHUNK_SIZE,ySizeChunks*Global.CHUNK_SIZE);
        }

        public Vector2Int GetCenter() {
            return new Vector2Int((coveredArea.X.UpperBound+coveredArea.X.LowerBound)/2,(coveredArea.Y.UpperBound+coveredArea.Y.LowerBound)/2);
        }
        private void SoftLoadTileEntities() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                        continue;
                    }
                    conduitTileChunkPartition.SoftLoadTileEntities();
                }
            }
        }

        private void AssembleMultiBlocks() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                        continue;
                    }
                    conduitTileChunkPartition.AssembleMultiBlocks();
                }
            }
        }

        public void Save() {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogWarning("Non conduit partition in soft loaded tile chunk");
                        continue;
                    }
                    Dictionary<ConduitType, IConduit[,]> partitionConduits = new Dictionary<ConduitType, IConduit[,]>();
                    foreach (KeyValuePair<TileMapType,IConduitSystemManager> kvp in conduitSystemManagersDict) {
                        IConduitSystemManager manager = kvp.Value;
                        partitionConduits[kvp.Key.toConduitType()] = manager.GetConduitPartitionData(partition.GetRealPosition());
                    }
                    conduitTileChunkPartition.SetConduits(partitionConduits);
                    partition.Save();
                }
                ChunkIO.writeChunk(chunk,path:savePath,directory:true);
            }
        }

        public void TickUpdate() {
            foreach (IConduitSystemManager manager in conduitSystemManagersDict.Values) {
                if (manager is ITickableConduitSystem tickableConduitSystem) {
                    tickableConduitSystem.tickUpdate();
                }
            }
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    partition.Tick();
                }
            }
        }

        public IChunk GetChunkAtPosition(Vector2Int chunkPosition)
        {
            foreach (var chunk in softLoadedChunks)
            {
                if (chunk.GetPosition() == chunkPosition) return chunk;
            }

            return null;
        }
    }
}

