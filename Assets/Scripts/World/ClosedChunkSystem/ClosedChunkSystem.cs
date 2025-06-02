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
using Entities;
using Entities.Mob;
using Player;
using TileEntity;
using TileEntity.AssetManagement;
using Tiles.TileMap;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World.Cave.Registry;
using Random = System.Random;

namespace Chunks.Systems {
    
    /// <summary>
    /// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
    /// A Dimension can have a collection of ClosedChunkSystems 
    /// </summary>
    public abstract class ClosedChunkSystem : MonoBehaviour, ILoadedChunkSystem
    {
        protected Dictionary<TileMapType, IWorldTileMap> tileGridMaps = new();
        private List<ShaderTilemapManager> tileGridMapShaderMaps = new();
        protected Dictionary<TileEntityTileMapType, Tilemap> tileEntityMaps = new();
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
        public IntervalVector CoveredArea => coveredArea;
        private FluidTileMap fluidTileMap;
        List<IChunkPartition> partitionsToLoad = new List<IChunkPartition>(128);
        List<IChunkPartition> partitionsToUnload = new List<IChunkPartition>(128);
        List<IChunkPartition> partitionsToFarLoad = new List<IChunkPartition>(128);
        private Transform entityContainer;
        private Random random = new Random();
        private int lastShaderMapIndex;
        public Transform EntityContainer => entityContainer;
        public virtual void Awake () {
            mainCamera = Camera.main;
            
        }

        public void AddChunk(ILoadedChunk chunk) {
            if (chunk == null) {
                return;
            }
            Vector2Int chunkPosition = chunk.GetPosition();
            cachedChunks[chunkPosition] = chunk;
            fluidTileMap?.AddChunk(chunk);
        }
        public void RemoveChunk(ILoadedChunk chunk) {
            Vector2Int chunkPosition = chunk.GetPosition();
            if (ChunkIsCached(chunkPosition)) {
                cachedChunks.Remove(chunkPosition);
            }
            fluidTileMap?.Simulator.SaveToChunk(chunk);
            fluidTileMap?.RemoveChunk(chunkPosition);
        }

        public ILoadedChunk GetChunk(Vector2Int position)
        {
            return cachedChunks.GetValueOrDefault(position);
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

        public void InitializeMiscObjects(DimensionObjects dimensionObjects)
        {
            this.loadedPartitionBoundary = Instantiate(dimensionObjects.loadedPartitionBoundary, transform, false);
            loadedPartitionBoundary.Initialize();
            
            this.breakIndicator = Instantiate(dimensionObjects.tileBreakIndicator, transform, false);
        }
        
        /// <summary>
        /// Initializes system objects. This should be called after all tile grid maps are initialized
        /// </summary>
        /// <param name="dimController"></param>
        /// <param name="coveredArea"></param>
        /// <param name="dim"></param>
        
        public void InitializeObject(DimController dimController, IntervalVector coveredArea, int dim) {
            transform.position = Vector3.zero;

            transform.SetParent(dimController.transform,false);

            GameObject chunkContainer = new GameObject();
            chunkContainer.name = "Chunks";
            chunkContainer.transform.SetParent(transform,false);
            chunkContainerTransform = chunkContainer.transform;
            
            GameObject entityContainerObject = new GameObject();
            entityContainerObject.name = "Entities";
            entityContainerObject.transform.SetParent(transform);
            MobEntityParticleController mobEntityParticleController = entityContainerObject.AddComponent<MobEntityParticleController>();
            mobEntityParticleController.Initialize(DimensionManager.Instance.MiscDimAssets.EntityDeathParticlePrefab);
            entityContainer = entityContainerObject.transform;
            entityContainer.transform.localPosition = new Vector3(0, 0, -3f);
            
            player = PlayerManager.Instance.GetPlayer();
            cachedChunks = new Dictionary<Vector2Int, ILoadedChunk>();
            this.dim = dim;
            this.coveredArea = coveredArea;
            InitLoaders();
            
            foreach (var worldTileMap in tileGridMaps.Values)
            {
                if (worldTileMap is IWorldShaderTilemap worldShaderTilemap)
                {
                    tileGridMapShaderMaps.Add(worldShaderTilemap.GetManager());
                }

                if (worldTileMap is IMultiShaderTilemap multiShaderTilemap)
                {
                    multiShaderTilemap.FillShaderList(tileGridMapShaderMaps);
                }
            }
            
            Debug.Log("Closed Chunk System '" + name + "' In Dimension " + dim + $" Initialized with {tileGridMaps.Count} WorldTileMaps & {tileGridMapShaderMaps.Count} TileMapShaders");

            CameraBounds cameraBounds = CameraView.Instance.GetComponent<CameraBounds>();
            cameraBounds.SetSystem(this,dimController.BoundCamera);
            
            fluidTileMap = tileGridMaps[TileMapType.Fluid] as FluidTileMap;

            
        }

        public virtual void InitLoaders() {
            partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
            partitionLoader.Initalize(this,LoadUtils.getPartitionLoaderVariables());

            partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
            partitionUnloader.Initalize(this,LoadUtils.getPartitionUnloaderVariables());
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
        
        public void PlayerPartitionUpdate() {
            Vector2Int playerChunkPosition = GetPlayerChunk();
            
            Vector2Int last = currentPlayerPartition;
            currentPlayerPartition = GetCurrentPartitionPosition();
            playerPartitionChangeDifference = currentPlayerPartition - last;
            
            partitionsToLoad.Clear();
            partitionsToUnload.Clear();
            partitionsToFarLoad.Clear();
            
            const int CHUNK_SEARCH_RANGE = Global.CHUNK_LOAD_RANGE + 1;
            for (int x = - CHUNK_SEARCH_RANGE; x <=  CHUNK_SEARCH_RANGE; x++) {
                for (int y = - CHUNK_SEARCH_RANGE; y <=  CHUNK_SEARCH_RANGE; y++) {
                    Vector2Int chunkPosition = playerChunkPosition + new Vector2Int(x,y); 
                    if (!cachedChunks.TryGetValue(chunkPosition, out var chunk)) {
                        continue;
                    }
                    chunk.GetLoadedPartitionsFar(currentPlayerPartition,CameraView.ChunkPartitionLoadRange+Vector2Int.one,partitionsToUnload);
                    chunk.GetUnloadedPartitionsCloseTo(currentPlayerPartition,CameraView.ChunkPartitionLoadRange,partitionsToLoad);
                }
            }
            partitionLoader.addToQueue(partitionsToLoad);
            partitionUnloader.addToQueue(partitionsToUnload);
            CullShaderMaps();

        }

        private void CullShaderMaps()
        {
            double r = random.NextDouble();
            if (r >= 0.1f) return;
            tileGridMapShaderMaps[lastShaderMapIndex].PushUnusedMaps();
            lastShaderMapIndex++;
            lastShaderMapIndex %= tileGridMapShaderMaps.Count;

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

        public IEnumerator LoadTileEntityAssets()
        {
            HashSet<TileEntityObject> tileEntityObjects = new HashSet<TileEntityObject>();
            foreach (ILoadedChunk loadedChunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in loadedChunk.GetChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) continue;
                    conduitTileChunkPartition.Activate(loadedChunk);
                    partition.GetTileEntityObjects(tileEntityObjects);
                }
            }
            yield return TileEntityAssetRegistry.Instance.LoadAssets(tileEntityObjects);
        }

        public List<ILoadedChunk> GetLoadedChunksNearPlayer() {
            Vector2Int playerPosition = GetPlayerChunk();
            List<ILoadedChunk> chunks = new List<ILoadedChunk>();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                if (chunk.InRange(playerPosition,Global.CHUNK_LOAD_RANGE,Global.CHUNK_LOAD_RANGE)) {
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
            return ChunkInBounds(Global.GetChunkFromWorld(worldPosition));
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
                if (chunk.ScheduleForUnloading || chunk.IsChunkLoaded() ||
                    chunk.InRange(playerPosition, Global.CHUNK_LOAD_RANGE, Global.CHUNK_LOAD_RANGE) ||
                    !chunk.PartionsAreAllUnloaded()) continue;
                
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

        public virtual IEnumerator LoadChunkPartition(IChunkPartition chunkPartition) {
            if (chunkPartition.IsLoading()) yield break;
            loadedPartitionBoundary.PartitionLoaded(chunkPartition.GetRealPosition());
            fluidTileMap?.Simulator.SetPartitionDisplayStatus(chunkPartition.GetRealPosition(),true);
            yield return chunkPartition.Load(tileGridMaps);
            chunkPartition.SetIsLoading(false);
        }

        public void CacheChunk(Vector2Int chunkPosition) {
            ILoadedChunk chunk = ChunkIO.GetChunkFromJson(chunkPosition, this);
            AddChunk(chunk);
        }

        public void InstantCacheChunksNearPlayer() {
            List<Vector2Int> unloadedChunksNearPlayer = GetUnCachedChunkPositionsNearPlayer();
            foreach (Vector2Int pos in unloadedChunksNearPlayer) {
                CacheChunk(pos);
            }
        }
        
        public virtual IEnumerator UnloadChunkPartition(IChunkPartition chunkPartition) {
            
            chunkPartition.UnloadEntities();
            fluidTileMap?.Simulator.SetPartitionDisplayStatus(chunkPartition.GetRealPosition(),false);
            loadedPartitionBoundary.PartitionUnloaded(chunkPartition.GetRealPosition());
            
            yield return StartCoroutine(chunkPartition.UnloadTiles(tileGridMaps));
          
            breakIndicator.UnloadPartition(chunkPartition.GetRealPosition());
        }
        
        public void OnDisable()
        {
            if (isQuitting) {
                return;
            }
            partitionUnloader.clearQueue();
        }

        public FluidTileMap GetFluidTileMap()
        {
            return tileGridMaps[TileMapType.Fluid] as FluidTileMap;
        }
        
        public virtual void Save()
        {
            SaveEntities();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                fluidTileMap?.Simulator.SaveToChunk(chunk);
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition.GetLoaded()) {
                        partition.Save();
                    }
                }
                ChunkIO.WriteChunk(chunk);
            }
        }

        public abstract void TickUpdate();


        public abstract IEnumerator SaveCoroutine();

        public void SyncCaveRegistryTileEntities(CaveRegistry caveRegistry)
        {
            foreach (var chunk in cachedChunks.Values)
            {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions())
                {
                    List<IOnCaveRegistryLoadActionTileEntity> caveRegistryLoadActionTileEntitys =
                        partition.GetTileEntitiesOfType<IOnCaveRegistryLoadActionTileEntity>();
                    foreach (var caveRegistryLoadActionTileEntity in caveRegistryLoadActionTileEntitys)
                    {
                        caveRegistryLoadActionTileEntity.OnCaveRegistryLoaded(caveRegistry);
                    }
                }
            }
        }

        public void SaveEntities()
        {
            for (int i = 0; i < entityContainer.childCount; i++) {
                Transform entityTransform = entityContainer.GetChild(i);
                Entity entity = entityTransform.GetComponent<Entity>();
                if (entity is not ISerializableEntity serializableEntity) {
                    continue;
                }
                SeralizedEntityData seralizedEntityData = serializableEntity.serialize();
                if (seralizedEntityData == null) {
                    continue;
                }
                Vector2 position = new Vector2(seralizedEntityData.x,seralizedEntityData.y);
                Vector2Int chunkPosition = Global.GetChunkFromWorld(position);
                IChunk chunk = GetChunk(chunkPosition);
                if (chunk == null) {
                    continue;
                }
                Vector2Int partitionPosition = Global.GetPartitionFromWorld(position) - chunkPosition*Global.PARTITIONS_PER_CHUNK;

                IChunkPartition partition = chunk.GetPartition(partitionPosition);
                IChunkPartitionData partitionData = partition.GetData();
                if (partitionData is not SeralizedWorldData serializedTileData) {
                    continue;
                }
                serializedTileData.entityData.Add(seralizedEntityData);
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
        public Material UnlitMaterial;
        public ParticleSystem SplashParticlePrefab;
        public ParticleSystem FluidParticlePrefab;
        public ParticleSystem EntityDeathParticlePrefab;
        public TileBase SlopeExtendColliderTile;
        public Material HueShifterWorldMaterial;
        public TileBase EmptyTile;
    }
}


