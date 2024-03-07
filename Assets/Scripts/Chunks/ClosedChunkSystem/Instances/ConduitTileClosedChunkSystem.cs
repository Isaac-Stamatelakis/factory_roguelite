using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using ConduitModule.ConduitSystemModule;
using ChunkModule.PartitionModule;
using ConduitModule;
using TileMapModule.Layer;
using ChunkModule.IO;
using ConduitModule.Ports;
using Newtonsoft.Json;
using ConduitModule.PortViewer;
using PlayerModule;
using TileMapModule;
using TileMapModule.Conduit;
using TileEntityModule;

namespace ChunkModule.ClosedChunkSystemModule {
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private List<SoftLoadedConduitTileChunk> unloadedChunks;
        private Dictionary<TileMapType, ConduitSystemManager> conduitSystemManagersDict;
        private PortViewerController viewerController;
        
        public void FixedUpdate() {
            if (conduitSystemManagersDict == null) {
                return;
            }
            foreach (ConduitSystemManager manager in conduitSystemManagersDict.Values) {
                manager.tickUpdate();
            }
        }
        public override void Awake()
        {
            List<TileMapType> standardMaps = TileMapBundleFactory.getStandardTileTypes();
            foreach (TileMapType tileMapType in standardMaps) {
                initTileMapContainer(tileMapType);
            }
            List<TileMapType> conduitMaps = TileMapBundleFactory.getConduitTileTypes();
            foreach (TileMapType tileMapType in conduitMaps) {
                initTileMapContainer(tileMapType);
            }
            
            base.Awake();
        }

        public override void OnDisable()
        {
            partitionUnloader.clearAll();
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (List<IChunkPartition> chunkPartitionList in chunk.getChunkPartitions()) {
                    foreach (IChunkPartition partition in chunkPartitionList) {
                        if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                            continue;
                        }
                        
                        if (partition.getLoaded() && conduitTileChunkPartition.getConduitLoaded()) {
                            Dictionary<ConduitType, IConduit[,]> partitionConduits = new Dictionary<ConduitType, IConduit[,]>();
                            foreach (KeyValuePair<TileMapType,ConduitSystemManager> kvp in conduitSystemManagersDict) {
                                partitionConduits[kvp.Key.toConduitType()] = kvp.Value.getConduitPartitionData(partition.getRealPosition());
                            }
                            conduitTileChunkPartition.setConduits(partitionConduits);
                            partition.save(tileGridMaps);
                        }
                    }
                }
                ChunkIO.writeChunk(chunk);
            }
        }

        public void tileEntityPlaceUpdate(TileEntity tileEntity) {
            foreach (ConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.addTileEntity(tileEntity);
            }
        }

        public void tileEntityDeleteUpdate(Vector2Int position) {
            foreach (ConduitSystemManager conduitSystemManager in conduitSystemManagersDict.Values) {
                conduitSystemManager.deleteTileEntity(position);
            }
        }
        
        public void initalize(Transform dimTransform, IntervalVector coveredArea, int dim, SoftLoadedClosedChunkSystem inactiveClosedChunkSystem) {
            initalizeObject(dimTransform,coveredArea,dim);
            initalLoadChunks(inactiveClosedChunkSystem.UnloadedChunks);
            conduitSystemManagersDict = inactiveClosedChunkSystem.ConduitSystemManagersDict;
            foreach (SoftLoadedConduitTileChunk unloadedConduitTileChunk in inactiveClosedChunkSystem.UnloadedChunks) {
                ILoadedChunk loadedChunk = cachedChunks[unloadedConduitTileChunk.Position];
                foreach (List<IChunkPartition> conduitTileChunkPartitionList in unloadedConduitTileChunk.Partitions) {
                    foreach (IConduitTileChunkPartition partition in conduitTileChunkPartitionList) {
                        partition.activate(loadedChunk);
                    }
                }
            }
            GameObject portViewerController = new GameObject();
            portViewerController.name = "Conduit Port View Controller";
            portViewerController.transform.SetParent(transform);
            viewerController = portViewerController.AddComponent<PortViewerController>();

            syncConduitTileMap(TileMapType.ItemConduit);
            syncConduitTileMap(TileMapType.FluidConduit);
            syncConduitTileMap(TileMapType.EnergyConduit);
            syncConduitTileMap(TileMapType.SignalConduit);
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
            Debug.Log("Conduit Closed Chunk System '" + name + "' Loaded " + cachedChunks.Count + " Chunks");
        }

        public ConduitSystemManager getManager(ConduitType conduitType) {
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
    }
}
