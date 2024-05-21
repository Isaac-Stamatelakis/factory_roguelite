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
using TileEntityModule;
using Items;

namespace Chunks.ClosedChunkSystemModule {
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private List<SoftLoadedConduitTileChunk> unloadedChunks;
        private Dictionary<TileMapType, IConduitSystemManager> conduitSystemManagersDict;
        private PortViewerController viewerController;
        
        public override void Awake()
        {
            base.Awake();
            TileMapBundleFactory.loadConduitSystemMaps(transform,tileGridMaps);
        }


        public void tileEntityPlaceUpdate(TileEntity tileEntity) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.addTileEntity(tileEntity);
            }
        }

        public void tileEntityDeleteUpdate(Vector2Int position) {
            foreach (IConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.deleteTileEntity(position);
            }
        }
        
        public void initalize(Transform dimTransform, IntervalVector coveredArea, int dim, SoftLoadedClosedChunkSystem inactiveClosedChunkSystem, Vector2Int dimPositionOffset) {
            this.dimPositionOffset = dimPositionOffset;
            transform.position = new Vector3(-dimPositionOffset.x/2f,-dimPositionOffset.y/2f,0);
            initalizeObject(dimTransform,coveredArea,dim);
            initalLoadChunks(inactiveClosedChunkSystem.UnloadedChunks);
            conduitSystemManagersDict = inactiveClosedChunkSystem.ConduitSystemManagersDict;
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in inactiveClosedChunkSystem.UnloadedChunks) {
                ILoadedChunk loadedChunk = cachedChunks[unloadedConduitTileChunk.Position];
                foreach (IConduitTileChunkPartition partition in loadedChunk.getChunkPartitions()) {
                    partition.activate(loadedChunk);
                }
            }
            GameObject portViewerController = new GameObject();
            portViewerController.name = "Conduit Port View Controller";
            portViewerController.transform.SetParent(transform,false);
            viewerController = portViewerController.AddComponent<PortViewerController>();

            syncConduitTileMap(TileMapType.ItemConduit);
            syncConduitTileMap(TileMapType.FluidConduit);
            syncConduitTileMap(TileMapType.EnergyConduit);
            syncConduitTileMap(TileMapType.SignalConduit);
            syncConduitTileMap(TileMapType.MatrixConduit);
        }

        private void syncConduitTileMap(TileMapType tileMapType) {
            ITileMap tileMap = tileGridMaps[tileMapType];
            if (tileMap is not ConduitTileMap) {
                Debug.LogError("Attempted to assign conduit manager to a non conduit tile map");
            }
            ConduitTileMap conduitTileMap = (ConduitTileMap) tileMap;
            conduitTileMap.ConduitSystemManager = conduitSystemManagersDict[tileMapType];
        }


        public override IEnumerator loadChunkPartition(IChunkPartition chunkPartition, double angle)
        {
            yield return base.loadChunkPartition(chunkPartition, angle);
        }

        public override IEnumerator unloadChunkPartition(IChunkPartition chunkPartition)
        {
            yield return base.unloadChunkPartition(chunkPartition);
            
        }

        public override void playerChunkUpdate()
        {
            
        }
        protected void initalLoadChunks(List<SoftLoadedConduitTileChunk> unloadedChunks)
        {
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in unloadedChunks) {
                addChunk(ChunkIO.getChunkFromUnloadedChunk(unloadedConduitTileChunk,this));
            }
            //Debug.Log("Conduit Closed Chunk System '" + name + "' Loaded " + cachedChunks.Count + " Chunks");
        }

        public IConduitSystemManager getManager(ConduitType conduitType) {
            TileMapType tileMapType = conduitType.toTileMapType();
            if (!conduitSystemManagersDict.ContainsKey(tileMapType)) {
                Debug.LogError("ConduitTileClosedChunkSystem did not have " + conduitType.ToString() + " inside managed conduit systems");
                return null;
            }
            return conduitSystemManagersDict[tileMapType];
        }

        public Vector2Int getBottomLeftCorner() {
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
        }

        public override void saveOnDestroy()
        {
            // Do nothing
        }
    }
}
