using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.IO;
using TileMaps.Layer;
using TileMaps;
using TileMaps.Type;
using Chunks.Loaders;
using TileMaps.Conduit;
using Chunks.Partitions;
using Tiles;
using Fluids;
using PlayerModule;

namespace Chunks.Systems {
    
    /// <summary>
    /// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
    /// A Dimension can have a collection of ClosedChunkSystems 
    /// </summary>

    

    public abstract class ClosedChunkSystem : MonoBehaviour
    {
        protected Dictionary<TileMapType, ITileMap> tileGridMaps = new Dictionary<TileMapType, ITileMap>();
        protected Transform playerTransform;
        protected Dictionary<Vector2Int, ILoadedChunk> cachedChunks;
        public Dictionary<Vector2Int,ILoadedChunk> CachedChunk {get => cachedChunks;}
        protected TileBreakIndicator breakIndicator;
        protected IntervalVector coveredArea;
        protected PartitionLoader partitionLoader;
        protected PartitionUnloader partitionUnloader;
        protected PartitionFarLoader partitionFarLoader;
        protected Transform chunkContainerTransform;
        public Transform ChunkContainerTransform {get{return chunkContainerTransform;}}
        protected int dim;
        public TileBreakIndicator BreakIndicator {get => breakIndicator;}
        public int Dim {get{return dim;}}

        public Vector2Int DimPositionOffset { get => dimPositionOffset;}

        private bool isQuitting = false;
        private LoadedPartitionBoundary loadedPartitionBoundary;
        protected Vector2Int dimPositionOffset;
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

        public bool localWorldPositionInSystem(Vector2 worldPosition) {
            Vector2Int playerChunkPosition = getChunkPositionFromWorld(worldPosition);
            return cachedChunks.ContainsKey(playerChunkPosition);
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

        public Vector2 getWorldDimOffset() {
            return new Vector2(dimPositionOffset.x/2f,dimPositionOffset.y/2f);
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

        public void initalizeMiscObjects(DimensionObjects dimensionObjects) {
            this.loadedPartitionBoundary = dimensionObjects.loadedPartitionBoundary;
            dimensionObjects.loadedPartitionBoundary.transform.SetParent(transform,false);
            loadedPartitionBoundary.reset();

            this.breakIndicator = dimensionObjects.tileBreakIndicator;
            dimensionObjects.tileBreakIndicator.transform.SetParent(transform,false);
        }
        
        public void initalizeObject(Transform dimTransform, IntervalVector coveredArea, int dim, Vector2Int offset) {
            this.dimPositionOffset = offset;
            transform.position = new Vector3(-dimPositionOffset.x/2f,-dimPositionOffset.y/2f,0);

            transform.SetParent(dimTransform,false);
            GameObject chunkContainer = new GameObject();
            chunkContainer.name = "Chunks";
            chunkContainer.transform.SetParent(transform,false);
            chunkContainerTransform = chunkContainer.transform;
            playerTransform = GameObject.Find("Player").GetComponent<Transform>();
            cachedChunks = new Dictionary<Vector2Int, ILoadedChunk>();
            this.dim = dim;
            this.coveredArea = coveredArea;
            initLoaders();

            CameraBounds cameraBounds = GameObject.Find("Main Camera").GetComponent<CameraBounds>();
            cameraBounds.ClosedChunkSystem = this;
            Debug.Log("Closed Chunk System '" + name + "' In Dimension " + dim + " Loaded");
            GameObject.Find("Player").GetComponent<PlayerRobot>().enabled = true;
        }

        public virtual void initLoaders() {
            partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
            partitionLoader.initalize(this,LoadUtils.getPartitionLoaderVariables());

            partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
            partitionUnloader.initalize(this,LoadUtils.getPartitionUnloaderVariables());
            partitionUnloader.setLoader(partitionLoader);

            partitionFarLoader = chunkContainerTransform.gameObject.AddComponent<PartitionFarLoader>();
            partitionFarLoader.initalize(this,LoadUtils.getPartitionFarLoaderVariables());
            
        }
        
        public abstract void playerChunkUpdate(); 

        public virtual void playerPartitionUpdate() {
            List<ILoadedChunk> chunksNearPlayer = getLoadedChunksNearPlayer();
            Vector2Int playerPartition = getPlayerChunkPartition();

            List<IChunkPartition> partitionsToLoad = new List<IChunkPartition>();
            foreach (ILoadedChunk chunk in chunksNearPlayer) {
                partitionsToLoad.AddRange(chunk.getUnloadedPartitionsCloseTo(playerPartition,Global.ChunkPartitionLoadRange));
            }

            partitionLoader.addToQueue(partitionsToLoad);
            List<IChunkPartition> partitionsToUnload = new List<IChunkPartition>();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                partitionsToUnload.AddRange(chunk.getLoadedPartitionsFar(playerPartition,Global.ChunkPartitionLoadRange));
            }
            partitionUnloader.addToQueue(partitionsToUnload);

            List<IChunkPartition> partitionsToFarLoad = new List<IChunkPartition>();
            foreach (ILoadedChunk chunk in chunksNearPlayer) {
                partitionsToFarLoad.AddRange(chunk.getUnFarLoadedParititionsCloseTo(
                    playerPartition,
                    Global.ChunkPartitionLoadRange+new Vector2Int(Global.EXTRA_TILE_ENTITY_LOAD_RANGE,Global.EXTRA_TILE_ENTITY_LOAD_RANGE)
                ));
            }
            partitionFarLoader.addToQueue(partitionsToFarLoad);
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
                if (!chunk.ScheduleForUnloading && !chunk.isChunkLoaded() && !chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY) && chunk.partionsAreAllUnloaded()) {
                    chunk.ScheduleForUnloading = true;
                    chunksToUnload.Add(chunk);
                }
            }
            return chunksToUnload;
        }
        public Vector2Int getPlayerChunk() {
            Vector2Int pos = getChunkPositionFromWorld(playerTransform.position);
            return pos + DimPositionOffset/Global.ChunkSize;
        }

        public Vector2Int getChunkPositionFromWorld(Vector2 world) {
            return new Vector2Int(Mathf.FloorToInt(world.x / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)),Mathf.FloorToInt(world.y / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)));
        }

        public Vector2Int getPlayerChunkPartition() {
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / (Global.ChunkPartitionSize >> 1)),Mathf.FloorToInt(playerTransform.position.y / (Global.ChunkPartitionSize>>1)));
            return pos + DimPositionOffset/Global.ChunkPartitionSize;
        }

        public virtual IEnumerator loadChunkPartition(IChunkPartition chunkPartition, Vector2Int direction) {
            loadedPartitionBoundary.partitionLoaded(chunkPartition.getRealPosition());
            yield return chunkPartition.load(tileGridMaps,direction,DimPositionOffset);
            chunkPartition.setTileLoaded(true);
            
        }

        public void cacheChunk(Vector2Int chunkPosition) {
            ILoadedChunk chunk = ChunkIO.getChunkFromJson(chunkPosition, this);
            addChunk(chunk);
        }

        public void instantCacheChunksNearPlayer() {
            List<Vector2Int> unloadedChunksNearPlayer = getUnCachedChunkPositionsNearPlayer();
            foreach (Vector2Int pos in unloadedChunksNearPlayer) {
                cacheChunk(pos);
            }
        }
        public virtual IEnumerator unloadChunkPartition(IChunkPartition chunkPartition) {
            chunkPartition.unloadEntities();
            loadedPartitionBoundary.partitionUnloaded(chunkPartition.getRealPosition());
            yield return StartCoroutine(chunkPartition.unloadTiles(tileGridMaps));
            breakIndicator.unloadPartition(chunkPartition.getRealPosition());
            chunkPartition.setTileLoaded(false);
            chunkPartition.setFarLoaded(false);
            
        }

        public void OnApplicationQuit() {
            isQuitting = true;
            saveOnDestroy();
        }
        public void OnDisable()
        {
            if (isQuitting) {
                return;
            }
            saveOnDestroy();
        }

        public virtual void saveOnDestroy() {
            partitionUnloader.clearQueue();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in chunk.getChunkPartitions()) {
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

    [System.Serializable]
    public struct DimensionObjects {
        public TileBreakIndicator tileBreakIndicator;
        public LoadedPartitionBoundary loadedPartitionBoundary;

        public DimensionObjects(TileBreakIndicator tileBreakIndicator, LoadedPartitionBoundary loadedPartitionBoundary)
        {
            this.tileBreakIndicator = tileBreakIndicator;
            this.loadedPartitionBoundary = loadedPartitionBoundary;
        }
    }
}


