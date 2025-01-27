using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Partitions;
using Chunks.Systems;

namespace Chunks {

    public interface ISoftLoadedChunk {
        public SoftLoadedClosedChunkSystem getSystem();
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
        public SoftLoadedClosedChunkSystem System { get => system; set => system = value; }
        private SoftLoadedClosedChunkSystem system;

        public SoftLoadedConduitTileChunk(List<IChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, int dim) {
            this.position = chunkPosition;
            this.dim = dim;
            generatePartitions(chunkPartitionDataList);
        }
        protected void generatePartitions(List<IChunkPartitionData> chunkPartitionDataList) {
            partitions = new IChunkPartition[Global.PartitionsPerChunk,Global.PartitionsPerChunk];
            for (int x = 0; x < Global.PartitionsPerChunk; x ++) {
                for (int y = 0; y < Global.PartitionsPerChunk; y ++) {
                    partitions[x,y] = generatePartition(chunkPartitionDataList[x*Global.PartitionsPerChunk + y], new Vector2Int(x,y));
                }
            }
        }

        
        /// <summary>
        /// Generates a partition
        /// </summary>
        protected virtual IChunkPartition generatePartition(IChunkPartitionData data, Vector2Int position) {
            if (data is SeralizedWorldData) {
                if (data is WorldTileConduitData) {
                    return new ConduitChunkPartition<WorldTileConduitData>((WorldTileConduitData) data,position,this);
                }
                return new TileChunkPartition<SeralizedWorldData>((SeralizedWorldData) data,position,this);
            } else 
            return null;
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

        public SoftLoadedClosedChunkSystem getSystem()
        {
            return system;   
        }
    }
}

