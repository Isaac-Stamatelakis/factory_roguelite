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

namespace Chunks {
    public interface ILoadedChunk : IChunk {
        public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target, Vector2Int range, int yDownModifier);
        public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target, Vector2Int range);
        public List<IChunkPartition> getUnFarLoadedParititionsCloseTo(Vector2Int target, Vector2Int range);
        public bool partionsAreAllUnloaded();
        /// <summary>
        /// Deletes all chunk partitions
        /// </summary>
        public void unload();
        public float distanceFrom(Vector2Int target);
        public bool inRange(Vector2Int target, int xRange, int yRange);
        public bool isChunkLoaded();
        public Transform getEntityContainer();
        public Transform getTileEntityContainer();
        public IWorldTileMap getTileMap(TileMapType type);
        public ClosedChunkSystem getSystem();
        public HashSet<string> getEntityIds();
        
    }

    public interface IChunk {
        public Vector2Int GetPosition();
        public IChunkPartition[,] GetChunkPartitions();
        public IChunkPartition GetPartition(Vector2Int position);
        public int GetDim();
        public List<IChunkPartitionData> GetChunkPartitionData();
        public IChunkSystem GetChunkSystem();

    }

    public interface IChunkSystem
    {
        public IChunk GetChunkAtPosition(Vector2Int chunkPosition);
        public IChunk GetChunkAtCellPosition(Vector2Int cellPosition)
        {
            Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
            return GetChunkAtPosition(chunkPosition);
        }
        
        public IChunkPartition GetChunkPartitionAtCellPosition(Vector2Int cellPosition)
        {
            IChunk chunk = GetChunkAtCellPosition(cellPosition);
            Vector2Int position = Global.getPartitionFromCell(cellPosition);
            return chunk.GetPartition(position);
        }
        
        public (IChunkPartition, Vector2Int) GetPartitionAndPositionAtCellPosition(Vector2Int cellPosition)
        {
            Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
            IChunk chunk = GetChunkAtPosition(chunkPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(cellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(cellPosition)-chunkPosition*Global.PARTITIONS_PER_CHUNK; 
            return (chunk?.GetPartition(partitionPosition), positionInPartition);
        }

    }
    
    public class Chunk : MonoBehaviour, ILoadedChunk
    {
        protected IChunkPartition[,] partitions;

        
        /// <summary>
        /// a chunk is soft loaded if all tile entity machines inside of it are loaded
        /// </summary>
        [SerializeField] protected bool softLoaded = false;
        
        /// <summary>
        /// a chunk is chunk loaded if it remains softloaded whilst the player is far away
        /// </summary>
        [SerializeField] protected bool chunkLoaded = false;
        public bool ScheduleForUnloading;
        protected Transform entityContainer;
        protected ClosedChunkSystem closedChunkSystem;
        protected Vector2Int position; 
        protected int dim;
        protected Transform tileEntityContainer;
        public float distanceFrom(Vector2Int target)
        {
            return Mathf.Pow(target.x-position.x,2) + Mathf.Pow(target.y-position.y,2);
        }
        public virtual void initalize(int dim, List<IChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            this.dim = dim;
            this.position = chunkPosition;
            this.partitions = new IChunkPartition[Global.PARTITIONS_PER_CHUNK,Global.PARTITIONS_PER_CHUNK];
            this.closedChunkSystem = closedChunkSystem;
            generatePartitions(chunkPartitionDataList);
            transform.localPosition = new Vector3(chunkPosition.x*Global.CHUNK_SIZE/2,chunkPosition.y*Global.CHUNK_SIZE/2,0);
            initalizeContainers();
            
        }

        public virtual void initalizeFromUnloaded(int dim, IChunkPartition[,] partitions, Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            this.dim = dim;
            this.position = chunkPosition;
            this.partitions = partitions;
            this.closedChunkSystem = closedChunkSystem;
            transform.localPosition = new Vector3(chunkPosition.x*Global.CHUNK_SIZE/2,chunkPosition.y*Global.CHUNK_SIZE/2,0);
            initalizeContainers();
        }

        protected void initalizeContainers() {
            DimController dimController = closedChunkSystem.transform.parent.GetComponent<DimController>();
            entityContainer = dimController.EntityContainer;
            transform.SetParent(closedChunkSystem.ChunkContainerTransform,false);
            GameObject tileEntityContainerObject = new GameObject();
            tileEntityContainerObject.name = "TileEntities";
            tileEntityContainer = tileEntityContainerObject.transform;
            tileEntityContainer.transform.SetParent(transform,false);
        }

        protected void generatePartitions(List<IChunkPartitionData> chunkPartitionDataList) {
            for (int x = 0; x < Global.PARTITIONS_PER_CHUNK; x ++) {
                List<IChunkPartition> chunkPartitions = new List<IChunkPartition>();
                for (int y = 0; y < Global.PARTITIONS_PER_CHUNK; y ++) {
                    partitions[x,y] = generatePartition(chunkPartitionDataList[x*Global.PARTITIONS_PER_CHUNK + y], new Vector2Int(x,y));
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
            return closedChunkSystem;
        }

        public virtual void unload()
        {
            ChunkIO.WriteChunk(this);
            GameObject.Destroy(gameObject);

        }

        public IChunkPartition[,] GetChunkPartitions()
        {
            return this.partitions;
        }

        public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target, Vector2Int range, int yDownModifier)
        {
            List<IChunkPartition> close = new List<IChunkPartition>();
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
                    close.Add(partition);
                } 
            }
            return close;
        }

        public List<IChunkPartition> getUnFarLoadedParititionsCloseTo(Vector2Int target, Vector2Int range)
        {
            List<IChunkPartition> close = new List<IChunkPartition>();
            foreach (IChunkPartition partition in partitions) {
                if (!partition.GetFarLoaded() && !partition.GetScheduledForFarLoading() && partition.InRange(target,range.x,range.y)) {
                    partition.SetScheduledForFarLoading(true);
                    close.Add(partition);
                } 
            }
            return close;
        }

        public bool inRange(Vector2Int target, int xRange, int yRange)
        {
            return Mathf.Abs(target.x-position.x) <= xRange && Mathf.Abs(target.y-position.y) <= yRange;
        }

        public bool isChunkLoaded()
        {
            return this.chunkLoaded;
        }

        public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target, Vector2Int range)
        {
            List<IChunkPartition> far = new List<IChunkPartition>();
            foreach (IChunkPartition partition in partitions) {
                if (partition.GetLoaded() && !partition.GetScheduledForUnloading() && !partition.InRange(target,range.x,range.y)) {
                    partition.SetScheduleForUnloading(true);
                    far.Add(partition);
                } 
            }
            return far;
        }

        public List<IChunkPartition> getFarLoadedPartitionsFar(Vector2Int target, Vector2Int range)
        {
            List<IChunkPartition> far = new List<IChunkPartition>();
            foreach (IChunkPartition partition in partitions) {
                if (partition.GetFarLoaded() && !partition.InRange(target,range.x,range.y)) {
                    far.Add(partition);
                } 
            }
            return far;
        }

        public Vector2Int GetPosition()
        {
            return this.position;
        }

        public int GetDim()
        {
            return this.dim;
        }

        public Transform getEntityContainer()
        {
            
            return entityContainer;
        }

        public Transform getTileEntityContainer()
        {
            return tileEntityContainer;
        }

        public bool partionsAreAllUnloaded()
        {
            foreach (IChunkPartition partition in partitions) {
                    if (partition.GetLoaded() || partition.GetScheduledForUnloading()) {
                        return false;
                    }
                }
            return true;
        }

        public IChunkPartition GetPartition(Vector2Int position)
        {
            return this.partitions[position.x,position.y];
        }

        public IWorldTileMap getTileMap(TileMapType type)
        {
            return closedChunkSystem.GetTileMap(type);
        }

        public ClosedChunkSystem getSystem()
        {
            return closedChunkSystem;
        }

        public HashSet<string> getEntityIds()
        {
            HashSet<string> entityIds = new HashSet<string>();
            foreach (IChunkPartition partition in partitions) {
                IChunkPartitionData data = partition.GetData();
                if (data is not SeralizedWorldData serializedTileData) {
                    continue;
                }
                foreach (SeralizedEntityData seralizedEntityData in serializedTileData.entityData) {
                    if (seralizedEntityData.type != EntityType.Mob) {
                        continue;
                    }
                    try {
                        SerializedMobData serializedMobData = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedMobData>(seralizedEntityData.data);
                        entityIds.Add(serializedMobData.id);
                    } catch (JsonSerializationException e) {
                        Debug.LogError($"Chunk failed to get an id with data: {seralizedEntityData.data}\nerror: {e}");
                    }
                    
                }
            }
            return entityIds;
        }
    }
}
