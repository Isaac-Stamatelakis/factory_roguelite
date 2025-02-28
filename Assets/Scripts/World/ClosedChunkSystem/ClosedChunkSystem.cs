using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using Dimensions;
using Player;
using TileEntity;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Chunks.Systems {
    
    /// <summary>
    /// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
    /// A Dimension can have a collection of ClosedChunkSystems 
    /// </summary>

    

    public abstract class ClosedChunkSystem : MonoBehaviour, IChunkSystem
    {
        protected Dictionary<TileMapType, IWorldTileMap> tileGridMaps = new Dictionary<TileMapType, IWorldTileMap>();
        protected Dictionary<TileEntityTileMapType, Tilemap> tileEntityMaps = new Dictionary<TileEntityTileMapType, Tilemap>();
        protected PlayerScript player;
        protected Dictionary<Vector2Int, ILoadedChunk> cachedChunks;
        public Dictionary<Vector2Int,ILoadedChunk> CachedChunk => cachedChunks;
        protected TileBreakIndicator breakIndicator;
        protected IntervalVector coveredArea;
        protected PartitionLoader partitionLoader;
        protected PartitionUnloader partitionUnloader;
        protected PartitionFarLoader partitionFarLoader;
        protected Transform chunkContainerTransform;
        protected Vector2Int currentPlayerPartition;
        protected Vector2Int playerPartitionChangeDifference;
        public Transform ChunkContainerTransform => chunkContainerTransform;
        protected int dim;
        public TileBreakIndicator BreakIndicator => breakIndicator;
        public int Dim {get{return dim;}}
        private bool isQuitting;
        private LoadedPartitionBoundary loadedPartitionBoundary;
        private Camera mainCamera;
        protected bool interactable = true;
        public bool Interactable => interactable;
        public virtual void Awake () {
            mainCamera = Camera.main;
            
        }

        public void Start()
        {
            
        }

        public void addChunk(ILoadedChunk chunk) {
            if (chunk == null) {
                return;
            }
            Vector2Int chunkPosition = chunk.GetPosition();
            cachedChunks[chunkPosition] = chunk;
            FluidWorldTileMap fluidWorldTileMap = tileGridMaps[TileMapType.Fluid] as FluidWorldTileMap;
            fluidWorldTileMap?.AddChunk(chunk);
        }
        public void removeChunk(ILoadedChunk chunk) {
            Vector2Int chunkPosition = chunk.GetPosition();
            if (ChunkIsCached(chunkPosition)) {
                cachedChunks.Remove(chunkPosition);
            }
            FluidWorldTileMap fluidWorldTileMap = tileGridMaps[TileMapType.Fluid] as FluidWorldTileMap;
            fluidWorldTileMap?.RemoveChunk(chunkPosition);
        }

        public ILoadedChunk getChunk(Vector2Int position) {
            if (cachedChunks.ContainsKey(position)) {
                return cachedChunks[position];
            }
            return null;
        }

        public bool LocalWorldPositionInSystem(Vector2 worldPosition) {
            Vector2Int playerChunkPosition = GetChunkPositionFromWorld(worldPosition);
            return cachedChunks.ContainsKey(playerChunkPosition);
        }

        public void DeactivateAllPartitions() {
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    partition.SetTileLoaded(false);
                    partition.UnloadTileEntities();
                }
            }
        }

        public bool ContainsChunk(Vector2Int position) {
            return cachedChunks.ContainsKey(position);
        }
        
        public bool ContainsTileMap(TileMapType tileMapType) {
            return tileGridMaps.ContainsKey(tileMapType);
        }

        public IWorldTileMap GetTileMap(TileMapType tileMapType)
        {
            return tileGridMaps.GetValueOrDefault(tileMapType);
        }

        public void InitalizeMiscObjects(DimensionObjects dimensionObjects) {
            this.loadedPartitionBoundary = dimensionObjects.loadedPartitionBoundary;
            dimensionObjects.loadedPartitionBoundary.transform.SetParent(transform,false);
            loadedPartitionBoundary.reset();

            this.breakIndicator = dimensionObjects.tileBreakIndicator;
            dimensionObjects.tileBreakIndicator.transform.SetParent(transform,false);
        }
        
        public void InitalizeObject(DimController dimController, IntervalVector coveredArea, int dim) {
            transform.position = Vector3.zero;

            transform.SetParent(dimController.transform,false);

            GameObject chunkContainer = new GameObject();
            chunkContainer.name = "Chunks";
            chunkContainer.transform.SetParent(transform,false);
            chunkContainerTransform = chunkContainer.transform;
            player = PlayerManager.Instance.GetPlayer();
            cachedChunks = new Dictionary<Vector2Int, ILoadedChunk>();
            this.dim = dim;
            this.coveredArea = coveredArea;
            InitLoaders();

            Debug.Log("Closed Chunk System '" + name + "' In Dimension " + dim + " Loaded");

            CameraBounds cameraBounds = CameraView.Instance.GetComponent<CameraBounds>();
            cameraBounds.SetSystem(this,dimController.BoundCamera);
        }

        public virtual void InitLoaders() {
            partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
            partitionLoader.Initalize(this,LoadUtils.getPartitionLoaderVariables());

            partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
            partitionUnloader.Initalize(this,LoadUtils.getPartitionUnloaderVariables());
            
            //partitionFarLoader = chunkContainerTransform.gameObject.AddComponent<PartitionFarLoader>();
            //partitionFarLoader.Initalize(this,LoadUtils.getPartitionFarLoaderVariables());
            
        }

        public IntervalVector GetBounds() {
            return coveredArea * (Global.CHUNK_SIZE/2);
        }
        
        public abstract void PlayerChunkUpdate();


        private Vector2Int GetCurrentPartitionPosition()
        {
            return new Vector2Int(
                Mathf.FloorToInt(mainCamera.transform.position.x / (Global.CHUNK_PARTITION_SIZE >> 1)),
                Mathf.FloorToInt(mainCamera.transform.position.y / (Global.CHUNK_PARTITION_SIZE>>1))
            );
        }
        
        public virtual void PlayerPartitionUpdate() {
            Vector2Int playerChunkPosition = GetPlayerChunk();
            
            Vector2Int last = currentPlayerPartition;
            currentPlayerPartition = GetCurrentPartitionPosition();
            playerPartitionChangeDifference = currentPlayerPartition - last;
            
            List<IChunkPartition> partitionsToLoad = new List<IChunkPartition>();
            List<IChunkPartition> partitionsToUnload = new List<IChunkPartition>();
            List<IChunkPartition> partitionsToFarLoad = new List<IChunkPartition>();
            for (int x = - Global.CHUNK_LOAD_RANGE; x <=  Global.CHUNK_LOAD_RANGE; x++) {
                for (int y = - Global.CHUNK_LOAD_RANGE; y <=  Global.CHUNK_LOAD_RANGE; y++) {
                    Vector2Int chunkPosition = playerChunkPosition + new Vector2Int(x,y); 
                    if (!cachedChunks.TryGetValue(chunkPosition, out var chunk)) {
                        continue;
                    }

                    partitionsToUnload.AddRange(chunk.getLoadedPartitionsFar(currentPlayerPartition,CameraView.ChunkPartitionLoadRange));
                    partitionsToLoad.AddRange(chunk.getUnloadedPartitionsCloseTo(currentPlayerPartition,CameraView.ChunkPartitionLoadRange,0));
                    partitionsToFarLoad.AddRange(chunk.getUnFarLoadedParititionsCloseTo(
                        currentPlayerPartition,
                        CameraView.ChunkPartitionLoadRange+new Vector2Int(Global.EXTRA_TILE_ENTITY_LOAD_RANGE,Global.EXTRA_TILE_ENTITY_LOAD_RANGE)
                    ));
                }
            }
            partitionLoader.addToQueue(partitionsToLoad);
            partitionUnloader.addToQueue(partitionsToUnload);

            foreach (IChunkPartition partition in partitionsToFarLoad)
            {
                partition.LoadFarLoadTileEntities();
            }
            //partitionFarLoader.addToQueue(partitionsToFarLoad);
        }

        public List<Vector2Int> GetUnCachedChunkPositionsNearPlayer() {
            List<Vector2Int> positions = new List<Vector2Int>();
            Vector2Int playerChunkPosition = GetPlayerChunk();
            for (int x = playerChunkPosition.x-Global.CHUNK_LOAD_RANGE; x  <= playerChunkPosition.x+Global.CHUNK_LOAD_RANGE; x ++) {
                for (int y = playerChunkPosition.y-Global.CHUNK_LOAD_RANGE; y <= playerChunkPosition.y+Global.CHUNK_LOAD_RANGE; y ++) {
                    Vector2Int vect = new Vector2Int(x,y);
                    if (ChunkInBounds(vect) && !ChunkIsCached(vect)) {
                        positions.Add(vect);
                    }
                }
            }
            return positions;
        }

        public List<ILoadedChunk> GetLoadedChunksNearPlayer() {
            Vector2Int playerPosition = GetPlayerChunk();
            List<ILoadedChunk> chunks = new List<ILoadedChunk>();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                if (chunk.inRange(playerPosition,Global.CHUNK_LOAD_RANGE,Global.CHUNK_LOAD_RANGE)) {
                    chunks.Add(chunk);
                }
            }
            return chunks;
        }

        public bool ChunkInBounds(Vector2Int position) {
            return 
                position.x >= coveredArea.X.LowerBound && 
                position.x <= coveredArea.X.UpperBound && 
                position.y >= coveredArea.Y.LowerBound && 
                position.y <= coveredArea.Y.UpperBound;
        }
        public bool WorldPositionInBounds(Vector2 worldPosition) {
            return ChunkInBounds(Global.getChunkFromWorld(worldPosition));
        }   
        public bool ChunkIsCached(Vector2Int position) {
            return this.cachedChunks.ContainsKey(position);
        }

        public Tilemap GetTileEntityTileMap(TileEntityTileMapType tileEntityTileMapType)
        {
            return tileEntityMaps[tileEntityTileMapType];
        }
        public List<Chunk> GetLoadedChunksOutsideRange() {
            Vector2Int playerPosition = GetPlayerChunk();
            List<Chunk> chunksToUnload = new List<Chunk>();
            foreach (Chunk chunk in cachedChunks.Values)
            {
                if (chunk.ScheduleForUnloading || chunk.isChunkLoaded() ||
                    chunk.inRange(playerPosition, Global.CHUNK_LOAD_RANGE, Global.CHUNK_LOAD_RANGE) ||
                    !chunk.partionsAreAllUnloaded()) continue;
                
                chunk.ScheduleForUnloading = true;
                chunksToUnload.Add(chunk);
            }
            return chunksToUnload;
        }
        public Vector2Int GetPlayerChunk() {
            return GetChunkPositionFromWorld(player.transform.position);
        }

        public static Vector2Int GetChunkPositionFromWorld(Vector2 world) {
            return new Vector2Int(Mathf.FloorToInt(world.x / ((Global.PARTITIONS_PER_CHUNK >> 1)*Global.CHUNK_PARTITION_SIZE)),Mathf.FloorToInt(world.y / ((Global.PARTITIONS_PER_CHUNK >> 1)*Global.CHUNK_PARTITION_SIZE)));
        }

        public Vector2Int GetPlayerChunkPartition() {
            return currentPlayerPartition;
        }

        public Vector2Int GetPlayerPartitionChangeDifference() {
            return playerPartitionChangeDifference;
        }

        public virtual IEnumerator LoadChunkPartition(IChunkPartition chunkPartition, Direction direction) {
            loadedPartitionBoundary.partitionLoaded(chunkPartition.GetRealPosition());
            yield return chunkPartition.Load(tileGridMaps,direction);
            chunkPartition.SetTileLoaded(true);
        }

        public void CacheChunk(Vector2Int chunkPosition) {
            ILoadedChunk chunk = ChunkIO.GetChunkFromJson(chunkPosition, this);
            addChunk(chunk);
        }

        public void InstantCacheChunksNearPlayer() {
            List<Vector2Int> unloadedChunksNearPlayer = GetUnCachedChunkPositionsNearPlayer();
            foreach (Vector2Int pos in unloadedChunksNearPlayer) {
                CacheChunk(pos);
            }
        }
        
        public virtual IEnumerator UnloadChunkPartition(IChunkPartition chunkPartition) {
            chunkPartition.UnloadEntities();
            loadedPartitionBoundary.partitionUnloaded(chunkPartition.GetRealPosition());
            yield return StartCoroutine(chunkPartition.UnloadTiles(tileGridMaps));
            breakIndicator.unloadPartition(chunkPartition.GetRealPosition());
            chunkPartition.SetTileLoaded(false);
            chunkPartition.SetFarLoaded(false);
        }

        public void OnApplicationQuit() {
            isQuitting = true;
            Save();
        }
        public void OnDisable()
        {
            if (isQuitting) {
                return;
            }
            partitionUnloader.clearQueue();
        }

        public virtual void Save() {
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition.GetLoaded()) {
                        partition.Save();
                    }
                }
                ChunkIO.WriteChunk(chunk);
            }
        }
        

        public IEnumerator SaveCoroutine()
        {
            var delay = new WaitForFixedUpdate();
            List<Vector2Int> currentlyCachedChunks = cachedChunks.Keys.ToList(); // Required to prevent modifying collection during enumeration
            foreach (Vector2Int chunkPosition in currentlyCachedChunks) {
                if (!cachedChunks.TryGetValue(chunkPosition, out var chunk)) continue;
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition.GetLoaded()) {
                        partition.Save();
                    }
                }
                ChunkIO.WriteChunk(chunk);
                yield return delay;
            }
        }

        protected Vector2Int GetSize() {
            int xSizeChunks = Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound)+1;
            int ySizeChunks = Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound)+1;
            return new Vector2Int(xSizeChunks*Global.CHUNK_SIZE,ySizeChunks*Global.CHUNK_SIZE);
        }


        public IChunk GetChunkAtPosition(Vector2Int chunkPosition)
        {
            cachedChunks.TryGetValue(chunkPosition, out var chunk);
            return chunk;
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

    [System.Serializable]
    public class MiscDimAssets
    {
        [FormerlySerializedAs("UnlitMaterial")] public Material LitMaterial;
    }
}


