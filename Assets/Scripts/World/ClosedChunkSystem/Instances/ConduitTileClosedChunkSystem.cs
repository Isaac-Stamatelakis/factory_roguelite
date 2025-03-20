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
        private List<SoftLoadedConduitTileChunk> unloadedChunks;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict;
        public Dictionary<TileMapType, IConduitSystemManager> ConduitSystemManagersDict => conduitSystemManagersDict;
        private PortViewerController viewerController;
        public PortViewerController PortViewerController => viewerController;
        
        
        public void TileEntityPlaceUpdate(ITileEntityInstance tileEntity) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.AddTileEntity(tileEntity);
            }
        }

        public void TileEntityDeleteUpdate(Vector2Int position) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.DeleteTileEntity(position);
            }
        }
        
        public void Initialize(DimController dimController, IntervalVector coveredArea, int dim, LoadedClosedChunkSystem inactiveClosedChunkSystem, PlayerScript playerScript) {
            if (coveredArea == null)
            {
                throw new InvalidSystemException($"Tried to initialize closed chunk system with '{inactiveClosedChunkSystem.Chunks.Count}' chunks");
            }
            TileMapBundleFactory.LoadTileSystemMaps(transform,tileGridMaps);
            TileMapBundleFactory.LoadTileEntityMaps(transform,tileEntityMaps, DimensionManager.Instance.MiscDimAssets.LitMaterial);
            TileMapBundleFactory.LoadConduitSystemMaps(transform,tileGridMaps);
            InitalizeObject(dimController,coveredArea,dim);
            InitalLoadChunks(inactiveClosedChunkSystem.Chunks);
            conduitSystemManagersDict = inactiveClosedChunkSystem.ConduitSystemManagersDict;
            foreach (var (type, conduitSystemManager) in conduitSystemManagersDict)
            {
                conduitSystemManager.SetSystem(this);
            }
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in inactiveClosedChunkSystem.Chunks) {
                ILoadedChunk loadedChunk = cachedChunks[unloadedConduitTileChunk.Position];
                foreach (IChunkPartition partition in loadedChunk.GetChunkPartitions()) {
                    if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) continue;
                    conduitTileChunkPartition.Activate(loadedChunk);
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
        public override IEnumerator UnloadChunkPartition(IChunkPartition chunkPartition)
        {
            yield return base.UnloadChunkPartition(chunkPartition);
            
        }

        public override void PlayerChunkUpdate()
        {
            // Doesn't do anything except refresh viewer
            viewerController?.Refresh();
        }
        
        protected void InitalLoadChunks(List<SoftLoadedConduitTileChunk> unloadedChunks)
        {
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in unloadedChunks) {
                addChunk(ChunkIO.GetChunkFromUnloadedChunk(unloadedConduitTileChunk,this));
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

        public Vector2Int GetBottomLeftCorner() {
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.CHUNK_SIZE;
        }

        public override void Save()
        {
            // Do nothing
        }
    }
}
