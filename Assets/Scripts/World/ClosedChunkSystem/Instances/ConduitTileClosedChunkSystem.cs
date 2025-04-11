using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using TileMaps.Type;
using Conduits.Systems;
using Chunks.Partitions;
using Conduits;
using TileMaps.Layer;
using Chunks.IO;
using Conduit.View;
using Conduits.Ports;
using Newtonsoft.Json;
using Conduits.PortViewer;
using PlayerModule;
using TileMaps;
using TileMaps.Conduit;
using TileEntity;
using Items;
using UnityEngine.AddressableAssets;
using Dimensions;
using JetBrains.Annotations;
using Player;
using Player.Controls;
using TileEntity.AssetManagement;
using Object = UnityEngine.Object;

namespace Chunks.Systems {
    public class InvalidSystemException : Exception {
        public InvalidSystemException()
        {
        }

        protected InvalidSystemException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidSystemException(string message) : base(message)
        {
        }

        public InvalidSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private string savePath;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict;
        public Dictionary<TileMapType, IConduitSystemManager> ConduitSystemManagersDict => conduitSystemManagersDict;
        private PortViewerController viewerController;
        public PortViewerController PortViewerController => viewerController;
        private List<ITickableTileEntity> tickableTileEntities;
        private List<ITickableConduitSystemManager> tickableConduitSystemManagers;
        
        
        public void TileEntityPlaceUpdate(ITileEntityInstance tileEntity) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.AddTileEntity(tileEntity);
            }

            if (tileEntity is ITickableTileEntity tickableTileEntity)
            {
                tickableTileEntities.Add(tickableTileEntity);
            }
        }

        public void TileEntityDeleteUpdate(Vector2Int position)
        {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.DeleteTileEntity(position);
            }

            for (var index = tickableTileEntities.Count-1; index >= 0; index--)
            {
                if (tickableTileEntities[index].GetCellPosition() == position)
                {
                    tickableTileEntities.RemoveAt(index);
                }
            }
        }
        
        
        public void Initialize(DimController dimController, IntervalVector coveredArea, int dim, ClosedChunkSystemAssembler inactiveClosedChunkSystemAssembler, PlayerScript playerScript) {
            if (coveredArea == null)
            {
                throw new InvalidSystemException($"Tried to initialize closed chunk system with '{inactiveClosedChunkSystemAssembler.Chunks.Count}' chunks");
            }
            TileMapBundleFactory.LoadTileSystemMaps(transform,tileGridMaps);
            TileMapBundleFactory.LoadTileEntityMaps(transform,tileEntityMaps, DimensionManager.Instance.MiscDimAssets.UnlitMaterial);
            TileMapBundleFactory.LoadConduitSystemMaps(transform,tileGridMaps);
            InitializeObject(dimController,coveredArea,dim);
            InitalLoadChunks(inactiveClosedChunkSystemAssembler.Chunks);
            
            
            conduitSystemManagersDict = inactiveClosedChunkSystemAssembler.ConduitSystemManagersDict;

            this.tickableTileEntities = inactiveClosedChunkSystemAssembler.GetTickableTileEntities();
            foreach (var (type, conduitSystemManager) in conduitSystemManagersDict)
            {
                conduitSystemManager.SetSystem(this);
            }

            tickableConduitSystemManagers = new List<ITickableConduitSystemManager>();
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values)
            {
                if (conduitSystemManager is ITickableConduitSystemManager tickableSystemManager)
                {
                    tickableConduitSystemManagers.Add(tickableSystemManager);
                }
            }
            
            SyncConduitTileMap(TileMapType.ItemConduit);
            SyncConduitTileMap(TileMapType.FluidConduit);
            SyncConduitTileMap(TileMapType.EnergyConduit);
            SyncConduitTileMap(TileMapType.SignalConduit);
            SyncConduitTileMap(TileMapType.MatrixConduit);
            
            viewerController = playerScript.TileViewers.ConduitPortViewer;
            viewerController.Initialize(this,playerScript);
            GameObject conduitViewListener = new GameObject();
            ConduitViewController viewListener = conduitViewListener.AddComponent<ConduitViewController>();
            viewListener.Initialize(this,playerScript);
            conduitViewListener.transform.SetParent(transform,false);
            this.savePath = inactiveClosedChunkSystemAssembler.SavePath;
        }
        

        private void SyncConduitTileMap(TileMapType tileMapType) {
            IWorldTileMap iWorldTileMap = tileGridMaps[tileMapType];
            if (iWorldTileMap is not ConduitTileMap) {
                Debug.LogError("Attempted to assign conduit manager to a non conduit tile map");
            }
            ConduitTileMap conduitTileMap = (ConduitTileMap) iWorldTileMap;
            conduitTileMap.ConduitSystemManager = conduitSystemManagersDict[tileMapType];
            conduitSystemManagersDict[tileMapType].SetTileMap(conduitTileMap);
        }
        
        public override void PlayerChunkUpdate()
        {
            // Doesn't do anything except refresh viewer
            viewerController?.Refresh();
        }
        
        protected void InitalLoadChunks(List<SoftLoadedConduitTileChunk> unloadedChunks)
        {
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in unloadedChunks) {
                AddChunk(ChunkIO.GetChunkFromUnloadedChunk(unloadedConduitTileChunk,this));
            }
        }

        public void OnDestroy()
        {
            if (conduitSystemManagersDict == null) return;
            foreach (var conduitSystemManager in conduitSystemManagersDict.Values)
            {
                conduitSystemManager.SetTileMap(null);
                conduitSystemManager.SetSystem(null);
            }
        }

        public IConduitSystemManager GetManager(ConduitType conduitType) {
            TileMapType tileMapType = conduitType.ToTileMapType();
            if (!conduitSystemManagersDict.ContainsKey(tileMapType)) {
                Debug.LogError("ConduitTileClosedChunkSystem did not have " + conduitType.ToString() + " inside managed conduit systems");
                return null;
            }
            return conduitSystemManagersDict[tileMapType];
        }
        
        public SoftLoadedClosedChunkSystem ToSoftLoadedSystem()
        {
            List<ISoftLoadableTileEntity> softLoadableTileEntities = new List<ISoftLoadableTileEntity>();
            foreach (var (position, chunk) in cachedChunks)
            {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions())
                {
                    var partitionSoftLoadableTileEntities = partition.GetTileEntitiesOfType<ISoftLoadableTileEntity>();
                    foreach (ISoftLoadableTileEntity softLoadableTileEntity in partitionSoftLoadableTileEntities)
                    {
                        if (softLoadableTileEntity is not IOverrideSoftLoadTileEntity)
                        {
                            softLoadableTileEntities.Add(softLoadableTileEntity);
                        }
                    }
                }
            }
            List<ITickableConduitSystem> tickableConduitSystems = new List<ITickableConduitSystem>();
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values)
            {
                if (conduitSystemManager is not ITickableConduitSystemManager tickableConduitSystemManager) continue;
                tickableConduitSystems.AddRange(tickableConduitSystemManager.GetTickableConduitSystems());
            }
            return new SoftLoadedClosedChunkSystem(softLoadableTileEntities, tickableConduitSystems,savePath,dim);
            
        }
        
        public override void Save()
        {
            SaveEntities();
            var fluidWorldTileMap = GetFluidTileMap();
            
            foreach (var (position, chunk) in cachedChunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    fluidWorldTileMap?.Simulator.SaveToChunk(chunk);
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

        public override void TickUpdate()
        {
            foreach (ITickableTileEntity tickableTileEntity in tickableTileEntities)
            {
                tickableTileEntity.TickUpdate();
            }
            foreach (ITickableConduitSystemManager tickableConduitSystemManager in tickableConduitSystemManagers)
            {
                tickableConduitSystemManager.TickUpdate();
            }
        }

        public override IEnumerator SaveCoroutine()
        {
            var fluidWorldTileMap = GetFluidTileMap();
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            foreach (var (position, chunk) in cachedChunks) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    fluidWorldTileMap?.Simulator.SaveToChunk(chunk);
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
    }
}
