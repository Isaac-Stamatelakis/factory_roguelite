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
using Object = UnityEngine.Object;

namespace Chunks.Systems {
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private List<SoftLoadedConduitTileChunk> unloadedChunks;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict;
        private PortViewerController viewerController;
        public PortViewerController PortViewerController => viewerController;
        
        public override void Awake()
        {
            base.Awake();
            TileMapBundleFactory.LoadConduitSystemMaps(transform,tileGridMaps);
           
        }


        public void tileEntityPlaceUpdate(ITileEntityInstance tileEntity) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.AddTileEntity(tileEntity);
            }
        }

        public void tileEntityDeleteUpdate(Vector2Int position) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.DeleteTileEntity(position);
            }
        }
        
        public void initalize(DimController dimController, IntervalVector coveredArea, int dim, SoftLoadedClosedChunkSystem inactiveClosedChunkSystem, Vector2Int dimPositionOffset) {
            initalizeObject(dimController,coveredArea,dim,dimPositionOffset);
            InitalLoadChunks(inactiveClosedChunkSystem.Chunks);
            conduitSystemManagersDict = inactiveClosedChunkSystem.ConduitSystemManagersDict;
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in inactiveClosedChunkSystem.Chunks) {
                ILoadedChunk loadedChunk = cachedChunks[unloadedConduitTileChunk.Position];
                foreach (IConduitTileChunkPartition partition in loadedChunk.GetChunkPartitions()) {
                    partition.activate(loadedChunk);
                }
            }
            StartCoroutine(createViewer());

            syncConduitTileMap(TileMapType.ItemConduit);
            syncConduitTileMap(TileMapType.FluidConduit);
            syncConduitTileMap(TileMapType.EnergyConduit);
            syncConduitTileMap(TileMapType.SignalConduit);
            syncConduitTileMap(TileMapType.MatrixConduit);
        }

        private IEnumerator createViewer() {
            var handle = Addressables.LoadAssetAsync<Object>("Assets/Prefabs/ConduitPortViewer/ConduitPortViewerController.prefab");
            yield return handle;
            GameObject portViewerControllerPrefab = AddressableUtils.validateHandle<GameObject>(handle);
            GameObject clone = GameObject.Instantiate(portViewerControllerPrefab, transform, false);
            viewerController = clone.GetComponent<PortViewerController>();
            viewerController.initalize(this);
        }

        private void syncConduitTileMap(TileMapType tileMapType) {
            IWorldTileMap iWorldTileMap = tileGridMaps[tileMapType];
            if (iWorldTileMap is not ConduitTileMap) {
                Debug.LogError("Attempted to assign conduit manager to a non conduit tile map");
            }
            ConduitTileMap conduitTileMap = (ConduitTileMap) iWorldTileMap;
            conduitTileMap.ConduitSystemManager = conduitSystemManagersDict[tileMapType];
            conduitSystemManagersDict[tileMapType].SetTileMap(conduitTileMap);
        }
        public override IEnumerator unloadChunkPartition(IChunkPartition chunkPartition)
        {
            yield return base.unloadChunkPartition(chunkPartition);
            
        }

        public override void playerChunkUpdate()
        {
            
        }
        protected void InitalLoadChunks(List<SoftLoadedConduitTileChunk> unloadedChunks)
        {
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in unloadedChunks) {
                addChunk(ChunkIO.getChunkFromUnloadedChunk(unloadedConduitTileChunk,this));
            }
            //Debug.Log("Conduit Closed Chunk System '" + name + "' Loaded " + cachedChunks.Count + " Chunks");
        }

        public void OnDestroy()
        {
            foreach (var conduitSystemManager in conduitSystemManagersDict.Values)
            {
                conduitSystemManager.SetTileMap(null);
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
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
        }

        public override void saveOnDestroy()
        {
            // Do nothing
        }
    }
}
