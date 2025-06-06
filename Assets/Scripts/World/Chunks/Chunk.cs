using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Chunks.IO;
using Chunks.Systems;
using Chunks.Partitions;
using TileMaps;
using TileMaps.Type;
using TileEntity;
using Dimensions;
using Entities;
using Entities.Mobs;
using Newtonsoft.Json;
using World.Cave.Registry;

namespace Chunks {
    public interface ILoadedChunk : IChunk {
        public void GetUnloadedPartitionsCloseTo(Vector2Int target, Vector2Int range, List<IChunkPartition> fill);
        public void GetLoadedPartitionsFar(Vector2Int target, Vector2Int range, List<IChunkPartition> fill);
        public void GetUnFarLoadedParititionsCloseTo(Vector2Int target, Vector2Int range, List<IChunkPartition> fill);
        public bool PartionsAreAllUnloaded();
        /// <summary>
        /// Deletes all chunk partitions
        /// </summary>
        public void Unload();
        public float DistanceFrom(Vector2Int target);
        public bool InRange(Vector2Int target, int xRange, int yRange);
        public bool IsChunkLoaded();
        public Transform GetEntityContainer();
        public Transform GetTileEntityContainer();
        public IWorldTileMap GetTileMap(TileMapType type);
        public ClosedChunkSystem GetSystem();
        
    }

    public interface IChunk {
        public Vector2Int GetPosition();
        public IChunkPartition[,] GetChunkPartitions();
        public IChunkPartition GetPartition(Vector2Int partitionPosition);
        public int GetDim();
        public List<IChunkPartitionData> GetChunkPartitionData();
        public ILoadedChunkSystem GetChunkSystem();

    }

    public interface IChunkSystem
    {
        public void Save();
        public void TileEntityTickUpdate();
        public IEnumerator SaveCoroutine();
        public void SyncCaveRegistryTileEntities(CaveRegistry caveRegistry);
    }

    public interface ILoadedChunkSystem : IChunkSystem
    {
        public IChunk GetChunkAtPosition(Vector2Int chunkPosition);
        public IChunk GetChunkAtCellPosition(Vector2Int cellPosition)
        {
            Vector2Int chunkPosition = Global.GetChunkFromCell(cellPosition);
            return GetChunkAtPosition(chunkPosition);
        }
        
        public IChunkPartition GetChunkPartitionAtCellPosition(Vector2Int cellPosition)
        {
            IChunk chunk = GetChunkAtCellPosition(cellPosition);
            Vector2Int position = Global.GetPartitionFromCell(cellPosition);
            return chunk.GetPartition(position);
        }
        
        public (IChunkPartition, Vector2Int) GetPartitionAndPositionAtCellPosition(Vector2Int cellPosition)
        {
            Vector2Int chunkPosition = Global.GetChunkFromCell(cellPosition);
            IChunk chunk = GetChunkAtPosition(chunkPosition);
            Vector2Int positionInPartition = Global.GetPositionInPartition(cellPosition);
            Vector2Int partitionPosition = Global.GetPartitionFromCell(cellPosition)-chunkPosition*Global.PARTITIONS_PER_CHUNK; 
            return (chunk?.GetPartition(partitionPosition), positionInPartition);
        }
    }
    
    public class Chunk : MonoBehaviour, ILoadedChunk
    {
        private IChunkPartition[,] partitions;
        [SerializeField] protected bool chunkLoaded = false;
        public bool ScheduleForUnloading;
        private Transform entityContainer;
        private ClosedChunkSystem ClosedChunkSystem;
        private Vector2Int position;
        private int dim;
        private Transform tileEntityContainer;
        public float DistanceFrom(Vector2Int target)
        {
            return Mathf.Pow(target.x-position.x,2) + Mathf.Pow(target.y-position.y,2);
        }
        public virtual void Initalize(int dim, List<IChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            this.dim = dim;
            this.position = chunkPosition;
            this.partitions = new IChunkPartition[Global.PARTITIONS_PER_CHUNK,Global.PARTITIONS_PER_CHUNK];
            this.ClosedChunkSystem = closedChunkSystem;
            GeneratePartitions(chunkPartitionDataList);
            transform.localPosition = new Vector3(chunkPosition.x*Global.CHUNK_SIZE/2,chunkPosition.y*Global.CHUNK_SIZE/2,0);
            InitalizeContainers();
            
        }

        public virtual void InitalizeFromUnloaded(int dim, IChunkPartition[,] partitions, Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            this.dim = dim;
            this.position = chunkPosition;
            this.partitions = partitions;
            this.ClosedChunkSystem = closedChunkSystem;
            transform.localPosition = new Vector3(chunkPosition.x*Global.CHUNK_SIZE/2,chunkPosition.y*Global.CHUNK_SIZE/2,0);
            InitalizeContainers();
        }

        protected void InitalizeContainers() {
            DimController dimController = ClosedChunkSystem.transform.parent.GetComponent<DimController>();
            entityContainer = dimController.GetActiveSystem()?.EntityContainer;
            transform.SetParent(ClosedChunkSystem.ChunkContainerTransform,false);
            GameObject tileEntityContainerObject = new GameObject();
            tileEntityContainerObject.name = "TileEntities";
            tileEntityContainer = tileEntityContainerObject.transform;
            tileEntityContainer.transform.SetParent(transform,false);
        }

        protected void GeneratePartitions(List<IChunkPartitionData> chunkPartitionDataList) {
            for (int x = 0; x < Global.PARTITIONS_PER_CHUNK; x ++) {
                List<IChunkPartition> chunkPartitions = new List<IChunkPartition>();
                for (int y = 0; y < Global.PARTITIONS_PER_CHUNK; y ++) {
                    partitions[x,y] = GeneratePartition(chunkPartitionDataList[x*Global.PARTITIONS_PER_CHUNK + y], new Vector2Int(x,y));
                }
            }
        }

        
        /// <summary>
        /// Generates a partition
        /// </summary>
        protected virtual IChunkPartition GeneratePartition(IChunkPartitionData data, Vector2Int partitionPosition) {
            if (data is WorldTileConduitData worldTileConduitData) {
                return new ConduitChunkPartition<WorldTileConduitData>(worldTileConduitData,partitionPosition,this);
            }
            if (data is SeralizedWorldData seralizedWorldData)
            {
                return new TileChunkPartition<SeralizedWorldData>(seralizedWorldData,partitionPosition,this);
            }
            
            return null;
        }
        public List<IChunkPartitionData> GetChunkPartitionData()
        {
            List<IChunkPartitionData> dataList = new List<IChunkPartitionData>();
            foreach (IChunkPartition chunkPartition in partitions) {
                dataList.Add(chunkPartition.GetData());
            }
            return dataList;
        }

        public ILoadedChunkSystem GetChunkSystem()
        {
            return ClosedChunkSystem;
        }

        public virtual void Unload()
        {
            ChunkIO.WriteChunk(this);
            GameObject.Destroy(gameObject);

        }

        public IChunkPartition[,] GetChunkPartitions()
        {
            return this.partitions;
        }

        public void GetUnloadedPartitionsCloseTo(Vector2Int target, Vector2Int range, List<IChunkPartition> toFill)
        {
            foreach (IChunkPartition partition in partitions) {
                if (partition.GetLoaded()) {
                    continue;
                }
                Vector2Int position = partition.GetRealPosition();
                Vector2Int dif = position-target;
                if (dif.x < -1) {
                    dif.x *= -1;
                }
                if (dif.y < -1) {
                    dif.y *= -1;
                }
                if (dif.x <= range.x && dif.y <= range.y) {
                    toFill.Add(partition);
                } 
            }
        }
        
        public bool InRange(Vector2Int target, int xRange, int yRange)
        {
            return Mathf.Abs(target.x-position.x) <= xRange && Mathf.Abs(target.y-position.y) <= yRange;
        }

        public bool IsChunkLoaded()
        {
            return this.chunkLoaded;
        }

        public void GetLoadedPartitionsFar(Vector2Int target, Vector2Int range, List<IChunkPartition> fill)
        {
            foreach (IChunkPartition partition in partitions) {
                if (partition.GetLoaded() && !partition.GetScheduledForUnloading() && !partition.InRange(target,range.x,range.y)) {
                    fill.Add(partition);
                }
            }
        }

        public void GetUnFarLoadedParititionsCloseTo(Vector2Int target, Vector2Int range, List<IChunkPartition> fill)
        {
            throw new System.NotImplementedException();
        }

        public Vector2Int GetPosition()
        {
            return this.position;
        }

        public int GetDim()
        {
            return this.dim;
        }

        public Transform GetEntityContainer()
        {
            return ClosedChunkSystem.EntityContainer;
        }

        public Transform GetTileEntityContainer()
        {
            return tileEntityContainer;
        }

        public bool PartionsAreAllUnloaded()
        {
            foreach (IChunkPartition partition in partitions) {
                if (partition.GetLoaded() || partition.GetScheduledForUnloading()) {
                    return false;
                }
            }
            return true;
        }

        public IChunkPartition GetPartition(Vector2Int partitionPosition)
        {
            return this.partitions[partitionPosition.x,partitionPosition.y];
        }

        public IWorldTileMap GetTileMap(TileMapType type)
        {
            return ClosedChunkSystem.GetTileMap(type);
        }

        public ClosedChunkSystem GetSystem()
        {
            return ClosedChunkSystem;
        }
    }
}
