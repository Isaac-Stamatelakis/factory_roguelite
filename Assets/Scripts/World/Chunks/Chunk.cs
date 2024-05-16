using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Chunks.IO;
using Chunks.ClosedChunkSystemModule;
using Chunks.Partitions;
using TileMaps;
using TileMaps.Type;
using TileEntityModule;
using Dimensions;
using Entities;
using Entities.Mobs;
using Newtonsoft.Json;

namespace Chunks {
    public interface ILoadedChunk : IChunk {
        public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target);
        public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target);
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
        public ITileMap getTileMap(TileMapType type);
        public ClosedChunkSystem getSystem();
        public HashSet<string> getEntityIds();
        
    }

    public interface IChunk {
        public Vector2Int getPosition();
        public IChunkPartition[,] getChunkPartitions();
        public IChunkPartition getPartition(Vector2Int position);
        public int getDim();
        public List<IChunkPartitionData> getChunkPartitionData();
        
    }

    public interface ISerizable {
        public void serialze();
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
            this.partitions = new IChunkPartition[Global.PartitionsPerChunk,Global.PartitionsPerChunk];
            this.closedChunkSystem = closedChunkSystem;
            generatePartitions(chunkPartitionDataList);
            transform.localPosition = new Vector3(chunkPosition.x*Global.ChunkSize/2,chunkPosition.y*Global.ChunkSize/2,0);
            initalizeContainers();
            
        }

        public virtual void initalizeFromUnloaded(int dim, IChunkPartition[,] partitions, Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            this.dim = dim;
            this.position = chunkPosition;
            this.partitions = partitions;
            this.closedChunkSystem = closedChunkSystem;
            transform.SetParent(closedChunkSystem.ChunkContainerTransform);
            transform.localPosition = new Vector3(chunkPosition.x*Global.ChunkSize/2,chunkPosition.y*Global.ChunkSize/2,0);
            initalizeContainers();
        }

        protected void initalizeContainers() {
            DimController dimController = closedChunkSystem.transform.parent.GetComponent<DimController>();
            entityContainer = dimController.EntityContainer;
            transform.SetParent(closedChunkSystem.ChunkContainerTransform);
            GameObject tileEntityContainerObject = new GameObject();
            tileEntityContainerObject.name = "TileEntities";
            tileEntityContainer = tileEntityContainerObject.transform;
            tileEntityContainer.transform.SetParent(transform,false);
        }

        protected void generatePartitions(List<IChunkPartitionData> chunkPartitionDataList) {
            for (int x = 0; x < Global.PartitionsPerChunk; x ++) {
                List<IChunkPartition> chunkPartitions = new List<IChunkPartition>();
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
        public List<IChunkPartitionData> getChunkPartitionData()
        {
            List<IChunkPartitionData> dataList = new List<IChunkPartitionData>();
            foreach (IChunkPartition chunkPartition in partitions) {
                dataList.Add(chunkPartition.getData());
            }
            return dataList;
        }

        public virtual void unload()
        {
            ChunkIO.writeChunk(this);
            GameObject.Destroy(gameObject);

        }

        public IChunkPartition[,] getChunkPartitions()
        {
            return this.partitions;
        }

        public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target)
        {
            List<IChunkPartition> close = new List<IChunkPartition>();
            foreach (IChunkPartition partition in partitions) {
                if (!partition.getLoaded() && partition.inRange(target,Global.ChunkPartitionLoadRange.x,Global.ChunkPartitionLoadRange.y)) {
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

        public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target)
        {
            List<IChunkPartition> far = new List<IChunkPartition>();
            foreach (IChunkPartition partition in partitions) {
                if (partition.getLoaded() && !partition.inRange(target,Global.ChunkPartitionLoadRange.x,Global.ChunkPartitionLoadRange.y)) {
                    far.Add(partition);
                } 
            }
            return far;
        }

        public Vector2Int getPosition()
        {
            return this.position;
        }

        public int getDim()
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
                    if (partition.getLoaded() || partition.getScheduledForUnloading()) {
                        return false;
                    }
                }
            return true;
        }

        public IChunkPartition getPartition(Vector2Int position)
        {
            return this.partitions[position.x,position.y];
        }

        public ITileMap getTileMap(TileMapType type)
        {
            return closedChunkSystem.getTileMap(type);
        }

        public ClosedChunkSystem getSystem()
        {
            return closedChunkSystem;
        }

        public HashSet<string> getEntityIds()
        {
            HashSet<string> entityIds = new HashSet<string>();
            foreach (IChunkPartition partition in partitions) {
                IChunkPartitionData data = partition.getData();
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
