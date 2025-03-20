using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Partitions;
using Chunks.Systems;

namespace Chunks {

    public interface ISoftLoadedChunk {
        public LoadedClosedChunkSystem getSystem();
    }
    /// <summary>
    /// A lightweight version of a conduit tile chunk
    /// </summary>
    public class SoftLoadedConduitTileChunk : IChunk, ISoftLoadedChunk
    {
        private IChunkPartition[,] partitions;
        private Vector2Int position;
        private int dim;
        public Vector2Int Position { get => position; set => position = value; }
        public IChunkPartition[,] Partitions { get => partitions; set => partitions = value; }
        public LoadedClosedChunkSystem System { get => system; set => system = value; }
        private LoadedClosedChunkSystem system;

        public SoftLoadedConduitTileChunk(List<IChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, int dim) {
            this.position = chunkPosition;
            this.dim = dim;
            GeneratePartitions(chunkPartitionDataList);
        }
        protected void GeneratePartitions(List<IChunkPartitionData> chunkPartitionDataList) {
            partitions = new IChunkPartition[Global.PARTITIONS_PER_CHUNK,Global.PARTITIONS_PER_CHUNK];
            for (int x = 0; x < Global.PARTITIONS_PER_CHUNK; x ++) {
                for (int y = 0; y < Global.PARTITIONS_PER_CHUNK; y ++) {
                    partitions[x,y] = GeneratePartition(chunkPartitionDataList[x*Global.PARTITIONS_PER_CHUNK + y], new Vector2Int(x,y));
                }
            }
        }

        
        /// <summary>
        /// Generates a partition
        /// </summary>
        protected virtual IChunkPartition GeneratePartition(IChunkPartitionData data, Vector2Int position) {
            if (data is not SeralizedWorldData worldData) return null;
            if (worldData is WorldTileConduitData worldTileConduitData) {
                return new ConduitChunkPartition<WorldTileConduitData>(worldTileConduitData,position,this);
            }
            return new TileChunkPartition<SeralizedWorldData>(worldData,position,this);
        }

        public Vector2Int GetPosition()
        {
            return position;
        }

        public IChunkPartition[,] GetChunkPartitions()
        {
            return partitions;
        }

        public IChunkPartition GetPartition(Vector2Int position)
        {
            return partitions[position.x,position.y];
        }

        public int GetDim()
        {
            return dim;
        }

        public List<IChunkPartitionData> GetChunkPartitionData()
        {
            List<IChunkPartitionData> dataList = new List<IChunkPartitionData>();
            foreach (IChunkPartition chunkPartition in partitions) {
                dataList.Add(chunkPartition.GetData());
            }
            return dataList;
        }

        public IChunkSystem GetChunkSystem()
        {
            return system;
        }

        public LoadedClosedChunkSystem getSystem()
        {
            return system;   
        }
    }
}

