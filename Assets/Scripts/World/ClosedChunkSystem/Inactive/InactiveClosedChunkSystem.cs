using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Systems;
using TileMaps.Type;
using Conduits;
using TileEntity;
using Conduits.Ports;
using Chunks.Partitions;
using TileEntity.Instances.CompactMachines;
using Chunks.IO;
using Dimensions;
using TileMaps.Layer;
using Tiles;
using World.Cave.Registry;

namespace Chunks.Systems {
    public class CompactMachineChunkSystemAssembler : ClosedChunkSystemAssembler, ICompactMachineClosedChunkSystem
    {
        private CompactMachineInstance compactMachineInstance;
        public CompactMachineChunkSystemAssembler(List<SoftLoadedConduitTileChunk> unloadedChunks, string savePath, int dim) : base(unloadedChunks, savePath, dim)
        {
        }

        public CompactMachineTeleportKey GetCompactMachineKey()
        {
            return compactMachineInstance.GetTeleportKey();
        }

        public void SetCompactMachine(CompactMachineInstance compactMachineInstance, CompactMachineTeleportKey key)
        {
            this.compactMachineInstance = compactMachineInstance;
        }

        public CompactMachineInstance GetCompactMachine()
        {
            return compactMachineInstance;
        }
    }
    
    public class SoftLoadedClosedChunkSystem : IChunkSystem
    {
        public SoftLoadedClosedChunkSystem(List<ISoftLoadableTileEntity> softLoadableTileEntities, List<ITickableConduitSystem> tickableConduitSystems, string savePath, int dim)
        {
            this.tickableTileEntities = new List<ITickableTileEntity>();
            foreach (ISoftLoadableTileEntity softLoadableTileEntity in softLoadableTileEntities)
            {
                if (softLoadableTileEntity is ITickableTileEntity tickableTileEntity)
                {
                    tickableTileEntities.Add(tickableTileEntity);
                }
            }
            this.softLoadableTileEntities = softLoadableTileEntities;
            this.TickableConduitSystems = tickableConduitSystems;
            this.dim = dim;
            this.savePath = savePath;
        }

        private List<ISoftLoadableTileEntity> softLoadableTileEntities;
        private List<ITickableTileEntity> tickableTileEntities;
        private List<ITickableConduitSystem> TickableConduitSystems;
        private int dim;
        private string savePath;
        public string SavePath => savePath;
        public void TickUpdate()
        {
            foreach (ITickableTileEntity tickableTileEntity in tickableTileEntities)
            {
                tickableTileEntity.TickUpdate();
            }
            foreach (ITickableConduitSystem tickableConduitSystem in TickableConduitSystems)
            {
                tickableConduitSystem.TickUpdate();
            }
        }

        /// <summary>
        /// Similar to Save, this function 
        /// </summary>
        /// <returns></returns>
        public IEnumerator SaveCoroutine()
        {
            var tileEntities = GetSerializableTileEntities();
            if (tileEntities.Count == 0) yield break;
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(dim, savePath);
            foreach (SoftLoadedConduitTileChunk chunk in chunks)
            {
                bool updated = SaveChunk(tileEntities, chunk);
                if (!updated) continue;
                yield return wait;
            }
        }

        public Dictionary<Vector2Int, ISoftLoadableTileEntity> GetSoftLoadableTileEntities()
        {
            Dictionary<Vector2Int,ISoftLoadableTileEntity> tileEntities = new Dictionary<Vector2Int, ISoftLoadableTileEntity>();
            foreach (ISoftLoadableTileEntity softLoadableTileEntity in softLoadableTileEntities)
            {
                tileEntities[softLoadableTileEntity.GetCellPosition()] = softLoadableTileEntity;
            }

            return tileEntities;
        }

        public void SyncCaveRegistryTileEntities(CaveRegistry caveRegistry)
        {
            foreach (ITickableTileEntity tickableTileEntity in tickableTileEntities)
            {
                if (tickableTileEntity is IOnCaveRegistryLoadActionTileEntity caveRegistryLoadActionTileEntity)
                {
                    caveRegistryLoadActionTileEntity.OnCaveRegistryLoaded(caveRegistry);
                }
            }
        }
        
        private Dictionary<Vector2Int, ISerializableTileEntity> GetSerializableTileEntities()
        {
            Dictionary<Vector2Int, ISerializableTileEntity> tileEntities = new Dictionary<Vector2Int, ISerializableTileEntity>();
            foreach (ITickableTileEntity tickableTileEntity in tickableTileEntities)
            {
                if (tickableTileEntity is ISerializableTileEntity serializableTileEntity)
                {
                    tileEntities[tickableTileEntity.GetCellPosition()] = serializableTileEntity;
                }
            }
            return tileEntities;
        }

        /// <summary>
        /// Overwrites tile entity data in a chunk
        /// </summary>
        /// <param name="serializableTileEntities"></param>
        /// <param name="softLoadedConduitTileChunk"></param>
        /// <returns>True if anything is changed, false if no change</returns>
        private bool SaveChunk(Dictionary<Vector2Int, ISerializableTileEntity> serializableTileEntities, SoftLoadedConduitTileChunk chunk)
        {
            bool updated = false;
            foreach (IChunkPartition partition in chunk.Partitions)
            {
                var worldData = partition.GetData();
                for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
                {
                    for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                    {
                        Vector2Int cellPosition = new Vector2Int(x, y) + partition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE;
                        if (!serializableTileEntities.TryGetValue(cellPosition,out ISerializableTileEntity serializableTileEntity)) continue;
                        worldData.baseData.sTileEntityOptions[x, y] = serializableTileEntity.Serialize();
                        updated = true;
                    }
                }
            }
            ChunkIO.WriteChunk(chunk,path:savePath,directory:true);
            return updated;
        }

        /// <summary>
        /// Sets tickable tile entity chunks to null and clears conduit system data
        /// </summary>
        public void ClearActiveComponents()
        {
            foreach (ISoftLoadableTileEntity softLoadableTileEntity in softLoadableTileEntities)
            {
                softLoadableTileEntity.SetChunk(null);
            }

            for (var i = TickableConduitSystems.Count-1; i >=0; i--)
            {
                var tickableConduitSystem = TickableConduitSystems[i];
                tickableConduitSystem.ClearNonSoftLoadableTileEntities();
                if (tickableConduitSystem.IsEmpty())
                {
                    TickableConduitSystems.RemoveAt(i);
                    continue;
                }
                tickableConduitSystem.ClearConduits();
            }
        }

        public void Save()
        {
            var tileEntities = GetSerializableTileEntities();
            if (tileEntities.Count == 0) return;
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(dim, savePath);
            foreach (SoftLoadedConduitTileChunk chunk in chunks)
            {
                SaveChunk(tileEntities, chunk);
            }
        }

        public override string ToString()
        {
            return $"SoftLoadedClosedChunkSystem at path {savePath} has {tickableTileEntities.Count} TickableTileEntities & {TickableConduitSystems.Count} TickableConduitSystems";
        }
    }
    public class ClosedChunkSystemAssembler : ILoadedChunkSystem
    {
        private IntervalVector coveredArea;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict; 
        private List<SoftLoadedConduitTileChunk> softLoadedChunks;
        private string savePath;
        public string SavePath => savePath;
        private int dim;
        public ClosedChunkSystemAssembler(List<SoftLoadedConduitTileChunk> unloadedChunks, string savePath, int dim) {
            this.softLoadedChunks = unloadedChunks;
            this.dim = dim;
            this.savePath = savePath;
            if (unloadedChunks.Count == 0) {
                return;
            }
            for (int i = 0; i < unloadedChunks.Count; i++) {
                updateCoveredArea(unloadedChunks[i]);
                Chunks[i].SystemAssembler = this;
            } 
        }

        public List<SoftLoadedConduitTileChunk> Chunks { get => softLoadedChunks; set => softLoadedChunks = value; }
        public Dictionary<TileMapType, IConduitSystemManager> ConduitSystemManagersDict { get => conduitSystemManagersDict; set => conduitSystemManagersDict = value; }
        public IntervalVector CoveredArea { get => coveredArea; set => coveredArea = value; }
        
        private void updateCoveredArea(SoftLoadedConduitTileChunk chunk) {
            if (coveredArea == null) {
                int x = chunk.GetPosition().x;
                int y = chunk.GetPosition().y;
                this.coveredArea = new IntervalVector(new Interval<int>(x,x), new Interval<int>(y,y));
                return;
            }
            Vector2Int newChunkPosition = chunk.GetPosition();
            if (newChunkPosition.x > coveredArea.X.UpperBound) {
                coveredArea.X.UpperBound = newChunkPosition.x;
            } else if (newChunkPosition.x < coveredArea.X.LowerBound) {
                coveredArea.X.LowerBound = newChunkPosition.x;
            }

            if (newChunkPosition.y > coveredArea.Y.UpperBound) {
                coveredArea.Y.UpperBound = newChunkPosition.y;
            } else if (newChunkPosition.y < coveredArea.Y.LowerBound) {
                coveredArea.Y.LowerBound = newChunkPosition.y;
            }
        }

        public void LoadSystem(Dictionary<Vector2Int, ISoftLoadableTileEntity> loadedTileEntities = null, bool softLoaded = true) {
            SoftLoadTileEntities(loadedTileEntities,softLoaded);
            AssembleMultiBlocks();
            InitConduitSystemManagers();
        }

        public SoftLoadedClosedChunkSystem ToSoftLoaded()
        {
            List<ISoftLoadableTileEntity> softLoadableTileEntities = new List<ISoftLoadableTileEntity>();
            foreach (IChunk chunk in Chunks)
            {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions())
                {
                    softLoadableTileEntities.AddRange(partition.GetTileEntitiesOfType<ISoftLoadableTileEntity>());
                }
            }

            List<ITickableConduitSystem> tickableConduitSystems = new List<ITickableConduitSystem>();
            foreach (IConduitSystemManager conduitSystemManager in ConduitSystemManagersDict.Values)
            {
                if (conduitSystemManager is ITickableConduitSystemManager tickableConduitSystemManager)
                {
                    var systems = tickableConduitSystemManager.GetTickableConduitSystems();
                    tickableConduitSystems.AddRange(systems);
                }
            }
            return new SoftLoadedClosedChunkSystem(softLoadableTileEntities,tickableConduitSystems,savePath,dim);
        }

        public List<ITickableTileEntity> GetTickableTileEntities()
        {
            List<ITickableTileEntity> tickableTileEntities = new List<ITickableTileEntity>();
            foreach (IChunk chunk in Chunks)
            {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions())
                {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
                    {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            ITileEntityInstance tileEntityInstance = partition.GetTileEntity(new Vector2Int(x, y));
                            if (tileEntityInstance is ITickableTileEntity tickableTileEntity) tickableTileEntities.Add(tickableTileEntity);
                        }
                    }
                }
            }
            return tickableTileEntities;
        }
        
        private void InitConduitSystemManagers() {
            conduitSystemManagersDict = new Dictionary<TileMapType, IConduitSystemManager>();
            InitConduitSystemManager(TileMapType.ItemConduit);
            InitConduitSystemManager(TileMapType.FluidConduit);
            InitConduitSystemManager(TileMapType.EnergyConduit);
            InitConduitSystemManager(TileMapType.SignalConduit);
            InitConduitSystemManager(TileMapType.MatrixConduit);
        }

        private void InitConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts = ConduitPortFactory.GetTileEntityPorts(conduitType,softLoadedChunks);
            Dictionary<Vector2Int, IConduit> conduits = GetConduits(conduitType, tileEntityPorts);
            IConduitSystemManager manager = ConduitSystemManagerFactory.CreateManager(
                conduitType: conduitType,
                conduits: conduits,
                chunkConduitPorts: tileEntityPorts
            );
            conduitSystemManagersDict[conduitMapType] = manager;
        }
        

        public TileItem getTileItem(Vector2Int currentCellPosition, Dictionary<Vector2Int, IChunk> chunkCache, Dictionary<Vector2Int, IChunkPartition> partitionCache, TileMapLayer layer) {
            Vector2Int chunkPosition = Global.getChunkFromCell(currentCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(currentCellPosition);
            if (!chunkCache.ContainsKey(chunkPosition)) {
                chunkCache[chunkPosition] = getChunk(chunkPosition);
            }
            IChunk chunk = chunkCache[chunkPosition];
            if (chunk == null) {
                return null;
            }
            if (!partitionCache.ContainsKey(partitionPosition)) {
                Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PARTITIONS_PER_CHUNK;
                partitionCache[partitionPosition] = chunk.GetPartition(adjustedPartitionPosition);
            }
            IChunkPartition partition = partitionCache[partitionPosition];
            Vector2Int cellPositionInPartition = Global.getPositionInPartition(currentCellPosition);
            TileItem tileItem = partition.GetTileItem(cellPositionInPartition,layer);
            return tileItem;
        }

        public ITileEntityInstance GetTileEntity(Vector2Int currentCellPosition) {
            Vector2Int chunkPosition = Global.getChunkFromCell(currentCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(currentCellPosition);
            IChunk chunk = getChunk(chunkPosition);
            if (chunk == null) {
                return null;
            }
            Vector2Int adjustedPartitionPosition = partitionPosition-chunkPosition*Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = chunk.GetPartition(adjustedPartitionPosition);
            Vector2Int cellPositionInPartition = Global.getPositionInPartition(currentCellPosition);
            return partition.GetTileEntity(cellPositionInPartition);
        }

        public SoftLoadedConduitTileChunk getChunk(Vector2Int cellPosition) {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                if (chunk.GetPosition().Equals(cellPosition)) {
                    return chunk;
                }
            }
            return null;
        }
        private Dictionary<Vector2Int, IConduit> GetConduits(ConduitType conduitType,Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts) {
            Dictionary<Vector2Int, IConduit> conduits = new Dictionary<Vector2Int, IConduit>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunks) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition chunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    chunkPartition.GetConduits(conduitType,conduits,tileEntityPorts);
                }
            }
            return conduits;
        }
       
        protected Vector2Int GetSize() {
            int xSizeChunks = Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound)+1;
            int ySizeChunks = Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound)+1;
            return new Vector2Int(xSizeChunks*Global.CHUNK_SIZE,ySizeChunks*Global.CHUNK_SIZE);
        }
        
        private void SoftLoadTileEntities(Dictionary<Vector2Int, ISoftLoadableTileEntity> preloadedTileEntities, bool softLoaded) {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                        continue;
                    }
                    if (!softLoaded)
                    {
                        conduitTileChunkPartition.LoadNonSoftLoadableConduitTileEntities();
                    }
                    
                    if (preloadedTileEntities != null)
                    {
                        conduitTileChunkPartition.SyncPreLoadedTileEntities(preloadedTileEntities);
                    }
                    else
                    {
                        conduitTileChunkPartition.SoftLoadTileEntities();
                    }

                    
                }
            }
        }

        private void AssembleMultiBlocks() {
            foreach (SoftLoadedConduitTileChunk chunk in softLoadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                        continue;
                    }
                    conduitTileChunkPartition.AssembleMultiBlocks();
                }
            }
        }

        public void Save() {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogWarning("Non conduit partition in soft loaded tile chunk");
                        continue;
                    }
                    Dictionary<ConduitType, IConduit[,]> partitionConduits = new Dictionary<ConduitType, IConduit[,]>();
                    foreach (KeyValuePair<TileMapType,IConduitSystemManager> kvp in conduitSystemManagersDict) {
                        IConduitSystemManager manager = kvp.Value;
                        partitionConduits[kvp.Key.toConduitType()] = manager.GetConduitPartitionData(partition.GetRealPosition());
                    }
                    conduitTileChunkPartition.SetConduits(partitionConduits);
                    partition.Save();
                }
                ChunkIO.WriteChunk(chunk,path:savePath,directory:true);
            }
        }

        public IEnumerator SaveCoroutine()
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                        Debug.LogWarning("Non conduit partition in soft loaded tile chunk");
                        continue;
                    }
                    Dictionary<ConduitType, IConduit[,]> partitionConduits = new Dictionary<ConduitType, IConduit[,]>();
                    foreach (KeyValuePair<TileMapType,IConduitSystemManager> kvp in conduitSystemManagersDict) {
                        IConduitSystemManager manager = kvp.Value;
                        partitionConduits[kvp.Key.toConduitType()] = manager.GetConduitPartitionData(partition.GetRealPosition());
                    }
                    conduitTileChunkPartition.SetConduits(partitionConduits);
                    partition.Save();
                }
                ChunkIO.WriteChunk(chunk,path:savePath,directory:true);
                yield return wait;
            }
        }

        public void TickUpdate() {
            foreach (IConduitSystemManager manager in conduitSystemManagersDict.Values) {
                if (manager is ITickableConduitSystemManager tickableConduitSystem) {
                    tickableConduitSystem.TickUpdate();
                }
            }
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    partition.Tick();
                }
            }
        }

        public IChunk GetChunkAtPosition(Vector2Int chunkPosition)
        {
            foreach (var chunk in softLoadedChunks)
            {
                if (chunk.GetPosition() == chunkPosition) return chunk;
            }

            return null;
        }

        public void SyncCaveRegistryTileEntities(CaveRegistry caveRegistry)
        {
            foreach (SoftLoadedConduitTileChunk chunk in Chunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
                    {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                        {
                            ITileEntityInstance tileEntityInstance = partition.GetTileEntity(new Vector2Int(x, y));
                            if (tileEntityInstance is not IOnCaveRegistryLoadActionTileEntity caveRegistryLoadActionTileEntity) continue;
                            caveRegistryLoadActionTileEntity.OnCaveRegistryLoaded(caveRegistry);
                        }
                    }
                }
            }
        }
    }
}

