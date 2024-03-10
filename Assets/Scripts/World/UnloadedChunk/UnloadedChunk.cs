using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.PartitionModule;

namespace ChunkModule {
    /// <summary>
    /// A lightweight version of a conduit tile chunk
    /// </summary>
    public class SoftLoadedConduitTileChunk : IChunk
    {
        private IChunkPartition[,] partitions;
        private Vector2Int position;
        private int dim;
        public Vector2Int Position { get => position; set => position = value; }
        public IChunkPartition[,] Partitions { get => partitions; set => partitions = value; }

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
            if (data is SerializedTileData) {
                if (data is SerializedTileConduitData) {
                    return new ConduitChunkPartition<SerializedTileConduitData>((SerializedTileConduitData) data,position,this);
                }
                return new TileChunkPartition<SerializedTileData>((SerializedTileData) data,position,this);
            } else 
            return null;
        }

        public Vector2Int getPosition()
        {
            return position;
        }

        public IChunkPartition[,] getChunkPartitions()
        {
            return partitions;
        }

        public IChunkPartition getPartition(Vector2Int position)
        {
            return partitions[position.x,position.y];
        }

        public int getDim()
        {
            return dim;
        }

        public List<IChunkPartitionData> getChunkPartitionData()
        {
            List<IChunkPartitionData> dataList = new List<IChunkPartitionData>();
            foreach (IChunkPartition chunkPartition in partitions) {
                dataList.Add(chunkPartition.getData());
            }
            return dataList;
        }
    }
}

