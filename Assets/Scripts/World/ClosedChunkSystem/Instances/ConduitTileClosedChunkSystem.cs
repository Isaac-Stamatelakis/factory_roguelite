using System;
using System.Collections;
using System.Collections.Generic;
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
using Player;
using Player.Controls;
using Object = UnityEngine.Object;

namespace Chunks.Systems {
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
        
        public void Initialize(DimController dimController, IntervalVector coveredArea, int dim, SoftLoadedClosedChunkSystem inactiveClosedChunkSystem, PlayerScript playerScript) {
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
            
            StartCoroutine(CreateViewer(playerScript));

            GameObject conduitViewListener = new GameObject();
            ConduitViewController viewListener = conduitViewListener.AddComponent<ConduitViewController>();
            viewListener.Initialize(this,playerScript);
            conduitViewListener.transform.SetParent(transform,false);
        }
        
        private IEnumerator CreateViewer(PlayerScript playerScript) {
            var handle = Addressables.LoadAssetAsync<Object>("Assets/Prefabs/ConduitPortViewer/ConduitPortViewerController.prefab");
            yield return handle;
            GameObject portViewerControllerPrefab = AddressableUtils.validateHandle<GameObject>(handle);
            GameObject clone = GameObject.Instantiate(portViewerControllerPrefab, transform, false);
            viewerController = clone.GetComponent<PortViewerController>();
            viewerController.Initialize(this,playerScript);
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

        public override void saveOnDestroy()
        {
            // Do nothing
        }
    }
}
