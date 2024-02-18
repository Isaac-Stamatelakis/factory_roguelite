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
            foreach (IChunk chunk in cachedChunks.Values) {
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
        private void initConduitSystemManagers() {
            conduitSystemManagersDict = new Dictionary<TileMapType, ConduitSystemManager>();
            initConduitSystemManager(TileMapType.ItemConduit);
            initConduitSystemManager(TileMapType.FluidConduit);
            initConduitSystemManager(TileMapType.EnergyConduit);
            initConduitSystemManager(TileMapType.SignalConduit);

            GameObject portViewerController = new GameObject();
            portViewerController.name = "Conduit Port View Controller";
            portViewerController.transform.SetParent(transform);
            viewerController = portViewerController.AddComponent<PortViewerController>();
        }

        private void initConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            ConduitSystemManager manager = new ConduitSystemManager(
                conduitType: conduitType,
                conduits: getConduits(conduitType),
                size: getSize(),
                chunkConduitPorts: getTileEntityPorts(conduitType),
                referencePosition: getBottomLeftCorner()
            );
            conduitSystemManagersDict[conduitMapType] = manager;
            ITileMap tileMap = tileGridMaps[conduitMapType];
            if (tileMap is not ConduitTileMap) {
                Debug.LogError("Attempted to assign conduit manager to a non conduit tile map");
            }
            ConduitTileMap conduitTileMap = (ConduitTileMap) tileMap;
            conduitTileMap.ConduitSystemManager = manager;
        }
        /// <summary>
        /// Returns a list of spots conduits can connect to tile entities of each chunk
        /// </summary>
        private Dictionary<TileEntity, List<TileEntityPort>> getTileEntityPorts(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
            Dictionary<TileEntity, List<TileEntityPort>> tileEntityPortData = new Dictionary<TileEntity, List<TileEntityPort>>();
            for (int x = coveredArea.X.LowerBound; x <= coveredArea.X.UpperBound; x++) {
                for (int y = coveredArea.Y.LowerBound; y <= coveredArea.Y.UpperBound; y++) {
                    Vector2Int chunkPosition = new Vector2Int(x,y);
                    if (!cachedChunks.ContainsKey(chunkPosition)) {
                        Debug.LogError("Attempted to load uncached chunk into conduit system");
                        continue;
                    }
                    IChunk chunk = cachedChunks[chunkPosition];
                    foreach (List<IChunkPartition> partionList in chunk.getChunkPartitions()) {
                        foreach (IChunkPartition partition in partionList) {
                            if (partition is not IConduitTileChunkPartition) {
                                Debug.LogError("Attempted to load non-conduit partition into conduit system");
                                continue;
                            }
                            Dictionary<TileEntity, List<TileEntityPort>> partitionPorts = ((IConduitTileChunkPartition) partition).getEntityPorts(conduitType,chunkFrameOfReference);
                            foreach (KeyValuePair<TileEntity, List<TileEntityPort>> kvp in partitionPorts) {
                                tileEntityPortData[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }
            return tileEntityPortData;
        }
        private IConduit[,] getConduits(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = getBottomLeftCorner();
            IConduit[,] conduits = new IConduit[size.x,size.y];
            for (int x = coveredArea.X.LowerBound; x <= coveredArea.X.UpperBound; x++) {
                for (int y = coveredArea.Y.LowerBound; y <= coveredArea.Y.UpperBound; y++) {
                    Vector2Int chunkPosition = new Vector2Int(x,y);
                    if (!cachedChunks.ContainsKey(chunkPosition)) {
                        Debug.LogError("Attempted to load uncached chunk into conduit system");
                        continue;
                    }
                    IChunk chunk = cachedChunks[chunkPosition];
                    foreach (List<IChunkPartition> partionList in chunk.getChunkPartitions()) {
                        foreach (IChunkPartition partition in partionList) {
                            if (partition is not IConduitTileChunkPartition) {
                                Debug.LogError("Attempted to load non-conduit partition into conduit system");
                                continue;
                            }
                            ((IConduitTileChunkPartition) partition).getConduits(conduitType,conduits,chunkFrameOfReference);
                        }
                    }
                }
            }
            return conduits;
        }
        public override void initalize(Transform dimTransform, IntervalVector coveredArea, int dim)
        {
            base.initalize(dimTransform, coveredArea, dim);
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
        public override IEnumerator initalLoadChunks()
        {
            for (int x = coveredArea.X.LowerBound; x <= coveredArea.X.UpperBound; x++) {
                for (int y = coveredArea.Y.LowerBound; y <= coveredArea.Y.UpperBound; y++) {
                    addChunk(ChunkIO.getChunkFromJson(new Vector2Int(x,y), this));
                }
            }
            yield return null;
            Debug.Log("Conduit Closed Chunk System '" + name + "' Chunk Loaded");
            loadTickableTileEntities();
            initConduitSystemManagers();
        }

        private void loadTickableTileEntities() {
            foreach (IChunk chunk in cachedChunks.Values) {
                foreach (List<IChunkPartition>  partitionList in chunk.getChunkPartitions()) {
                    foreach (IChunkPartition partition in partitionList) {
                        if (partition is not IConduitTileChunkPartition) {
                            Debug.LogError("Attempted to tick load non conduit tile chunk partition");
                        }
                        ((IConduitTileChunkPartition) partition).loadTickableTileEntities();
                    }
                    
                }
            }
            Debug.Log("Conduit Closed Chunk System '" + name + "' Tickable Tile Entities Loaded");
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
