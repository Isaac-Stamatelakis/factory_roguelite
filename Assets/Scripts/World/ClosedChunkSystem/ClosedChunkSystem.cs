using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.IO;
using TileMapModule.Layer;
using TileMapModule;
using TileMapModule.Type;
using ChunkModule.LoadController;
using TileMapModule.Conduit;
using ChunkModule.PartitionModule;
using Tiles;

namespace ChunkModule.ClosedChunkSystemModule {
    /// <summary>
    /// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
    /// A Dimension can have a collection of ClosedChunkSystems 
    /// </summary>

    public abstract class ClosedChunkSystem : MonoBehaviour
    {
        protected Dictionary<TileMapType, ITileMap> tileGridMaps = new Dictionary<TileMapType, ITileMap>();
        protected Transform playerTransform;
        //public ChunkList chunkList;
        protected Dictionary<Vector2Int, ILoadedChunk> cachedChunks;
        protected TileBreakIndicator breakIndicator;
        protected IntervalVector coveredArea;
        protected PartitionLoader partitionLoader;
        protected PartitionUnloader partitionUnloader;    
        protected Transform chunkContainerTransform;
        public Transform ChunkContainerTransform {get{return chunkContainerTransform;}}
        protected int dim;
        public TileBreakIndicator BreakIndicator {get => breakIndicator;}
        public int Dim {get{return dim;}}

        
        public virtual void Awake () {
            
        }

        public void addChunk(ILoadedChunk chunk) {
            Vector2Int chunkPosition = chunk.getPosition();
            cachedChunks[chunkPosition] = chunk;
        }
        public void removeChunk(ILoadedChunk chunk) {
            Vector2Int chunkPosition = chunk.getPosition();
            if (chunkIsCached(chunkPosition)) {
                cachedChunks.Remove(chunkPosition);
            }
        }

        public ILoadedChunk getChunk(Vector2Int position) {
            if (cachedChunks.ContainsKey(position)) {
                return cachedChunks[position];
            }
            return null;
        }

        public void deactivateAllPartitions() {
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in chunk.getChunkPartitions()) {
                    partition.setTileLoaded(false);
                }
            }
        }

        public bool containsChunk(Vector2Int position) {
            return cachedChunks.ContainsKey(position);
        }

        public bool containsTileMap(TileMapType tileMapType) {
            return tileGridMaps.ContainsKey(tileMapType);
        }

        public ITileMap getTileMap(TileMapType tileMapType) {
            if (tileGridMaps.ContainsKey(tileMapType)) {
                return tileGridMaps[tileMapType];
            }
            return null;
        }
        
        public void initalizeObject(Transform dimTransform, IntervalVector coveredArea, int dim) {
            transform.SetParent(dimTransform,false);
            GameObject chunkContainer = new GameObject();
            chunkContainer.name = "Chunks";
            chunkContainer.transform.SetParent(transform);
            chunkContainerTransform = chunkContainer.transform;
            playerTransform = GameObject.Find("Player").GetComponent<Transform>();
            cachedChunks = new Dictionary<Vector2Int, ILoadedChunk>();
            this.dim = dim;
            this.coveredArea = coveredArea;
            initLoaders();

            GameObject breakIndicatorObject = new GameObject();
            breakIndicator = breakIndicatorObject.AddComponent<TileBreakIndicator>();
            breakIndicator.init(this);
            breakIndicatorObject.transform.SetParent(transform,false);
            breakIndicator.name = "TileBreakIndicator";
            breakIndicator.transform.position = new Vector3(0,0,-1);

            CameraBounds cameraBounds = GameObject.Find("Main Camera").GetComponent<CameraBounds>();
            cameraBounds.ClosedChunkSystem = this;
            Debug.Log("Closed Chunk System '" + name + "' In Dimension " + dim + " Loaded");
            GameObject.Find("Player").GetComponent<PlayerRobot>().enabled = true;
        }

        public virtual void initLoaders() {
            partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
            partitionLoader.init(this);
            partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
            partitionUnloader.init(this,partitionLoader);
        }

        protected void initTileMapContainer(TileMapType tileType) {
            GameObject container = new GameObject();
            container.transform.SetParent(gameObject.transform);
            container.name = tileType.ToString();
            container.layer = LayerMask.NameToLayer(tileType.ToString());
            container.transform.localPosition = new Vector3(0,0,tileType.getZValue());
            Grid grid = container.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.5f,0.5f,1f);
            if (tileType.isTile()) {
                TileGridMap tileGridMap = container.AddComponent<TileGridMap>();
                tileGridMap.type = tileType;
                tileGridMaps[tileType] = tileGridMap;
            } else if (tileType.isConduit()) {
                ConduitTileMap tileGridMap = container.AddComponent<ConduitTileMap>();
                tileGridMap.type = tileType;
                tileGridMaps[tileType] = tileGridMap;
            }
        }
        
        public abstract void playerChunkUpdate(); 

        public virtual void playerPartitionUpdate() {
            List<ILoadedChunk> chunksNearPlayer = getLoadedChunksNearPlayer();
            List<IChunkPartition> partitionsToLoad = new List<IChunkPartition>();
            Vector2Int playerPartition = getPlayerChunkPartition();
            foreach (ILoadedChunk chunk in chunksNearPlayer) {
                partitionsToLoad.AddRange(chunk.getUnloadedPartitionsCloseTo(playerPartition));
            }
            partitionLoader.addToQueue(partitionsToLoad);

            List<IChunkPartition> partitionsToUnload = new List<IChunkPartition>();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                partitionsToUnload.AddRange(chunk.getLoadedPartitionsFar(playerPartition));
            }
            partitionUnloader.addToQueue(partitionsToUnload);
        }

        public List<Vector2Int> getUnCachedChunkPositionsNearPlayer() {
            List<Vector2Int> positions = new List<Vector2Int>();
            Vector2Int playerChunkPosition = getPlayerChunk();
            for (int x = playerChunkPosition.x-Global.ChunkLoadRangeX; x  <= playerChunkPosition.x+Global.ChunkLoadRangeX; x ++) {
                for (int y = playerChunkPosition.y-Global.ChunkLoadRangeY; y <= playerChunkPosition.y+Global.ChunkLoadRangeY; y ++) {
                    Vector2Int vect = new Vector2Int(x,y);
                    if (chunkInBounds(vect) && !chunkIsCached(vect)) {
                        positions.Add(vect);
                    }
                }
            }
            return positions;
        }

        public List<ILoadedChunk> getLoadedChunksNearPlayer() {
            Vector2Int playerPosition = getPlayerChunk();
            List<ILoadedChunk> chunks = new List<ILoadedChunk>();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                if (chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY)) {
                    chunks.Add(chunk);
                }
            }
            return chunks;
        }

        public bool chunkInBounds(Vector2Int position) {
            return 
                position.x >= coveredArea.X.LowerBound && 
                position.x <= coveredArea.X.UpperBound && 
                position.y >= coveredArea.Y.LowerBound && 
                position.y <= coveredArea.Y.UpperBound;
        }
        public bool worldPositionInBounds(Vector2 worldPosition) {
            return chunkInBounds(Global.getChunkFromWorld(worldPosition));
        }   
        public bool chunkIsCached(Vector2Int position) {
            return this.cachedChunks.ContainsKey(position);
        }

        
        public List<Chunk> getLoadedChunksOutsideRange() {
            Vector2Int playerPosition = getPlayerChunk();
            List<Chunk> chunksToUnload = new List<Chunk>();
            foreach (Chunk chunk in cachedChunks.Values) {
                if (!chunk.isChunkLoaded() && !chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY) && chunk.partionsAreAllUnloaded()) {
                    chunksToUnload.Add(chunk);
                }
            }
            return chunksToUnload;
        }
        public Vector2Int getPlayerChunk() {
            return new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)),Mathf.FloorToInt(playerTransform.position.y / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)));
        }

        public Vector2Int getPlayerChunkPartition() {
            return new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / (Global.ChunkPartitionSize >> 1)),Mathf.FloorToInt(playerTransform.position.y / (Global.ChunkPartitionSize>>1)));
        }

        public virtual IEnumerator loadChunkPartition(IChunkPartition chunkPartition,double angle) {
            yield return chunkPartition.load(tileGridMaps,angle);
            chunkPartition.setTileLoaded(true);
        }
        public virtual IEnumerator unloadChunkPartition(IChunkPartition chunkPartition) {
            yield return StartCoroutine(chunkPartition.unloadTiles(tileGridMaps));
            breakIndicator.unloadPartition(chunkPartition.getRealPosition());
            chunkPartition.setTileLoaded(false);
            chunkPartition.setScheduleForUnloading(false);
        }

        /// <summary> 
        /// This is called when game ends. Saves all partitions
        /// </summary>
        public void OnDisable()
        {
            saveOnDestroy();
        }

        public virtual void saveOnDestroy() {
            partitionUnloader.clearAll();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in  chunk.getChunkPartitions()) {
                    if (partition.getLoaded()) {
                        partition.save();
                    }
                }
                ChunkIO.writeChunk(chunk);
            }
        }

        protected Vector2Int getSize() {
            int xSizeChunks = Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound)+1;
            int ySizeChunks = Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound)+1;
            return new Vector2Int(xSizeChunks*Global.ChunkSize,ySizeChunks*Global.ChunkSize);
        }

        
    }
}


