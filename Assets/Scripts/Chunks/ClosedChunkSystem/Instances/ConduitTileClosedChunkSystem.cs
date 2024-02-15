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

namespace ChunkModule.ClosedChunkSystemModule {
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private Dictionary<TileMapType, ConduitSystemManager> conduitSystemsDict;
        private PortViewerController viewerController;
        
        public void Update() {

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
            GameObject portViewerController = new GameObject();
            portViewerController.name = "Conduit Port View Controller";
            portViewerController.transform.SetParent(transform);
            viewerController = portViewerController.AddComponent<PortViewerController>();
            base.Awake();
        }

        private void initConduitSystemManagers() {
            conduitSystemsDict = new Dictionary<TileMapType, ConduitSystemManager>();
            initConduitSystemManager(TileMapType.ItemConduit);
            initConduitSystemManager(TileMapType.FluidConduit);
            initConduitSystemManager(TileMapType.EnergyConduit);
            initConduitSystemManager(TileMapType.SignalConduit);
        }
        public void addToConduitSystem(ConduitItem conduitItem, IConduitOptions conduitOptions, Vector2Int position) {

        }

        private void initConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            conduitSystemsDict[conduitMapType] = new ConduitSystemManager(
                conduitType: conduitType,
                conduits: getConduits(conduitType),
                size: getSize(),
                chunkConduitPorts: getTileEntityPorts(conduitType)
            );
        }
        /// <summary>
        /// Returns a list of spots conduits can connect to tile entities of each chunk
        /// </summary>
        private Dictionary<Vector2Int, List<ConduitPortData>> getTileEntityPorts(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
            Dictionary<Vector2Int, List<ConduitPortData>> chunkConduitPortData = new Dictionary<Vector2Int, List<ConduitPortData>>();
            for (int x = coveredArea.X.LowerBound; x <= coveredArea.X.UpperBound; x++) {
                for (int y = coveredArea.Y.LowerBound; y <= coveredArea.Y.UpperBound; y++) {
                    Vector2Int chunkPosition = new Vector2Int(x,y);
                    if (!cachedChunks.ContainsKey(chunkPosition)) {
                        Debug.LogError("Attempted to load uncached chunk into conduit system");
                        continue;
                    }
                    IChunk chunk = cachedChunks[chunkPosition];
                    List<ConduitPortData> ports = new List<ConduitPortData>();
                    foreach (List<IChunkPartition> partionList in chunk.getChunkPartitions()) {
                        foreach (IChunkPartition partition in partionList) {
                            if (partition is not IConduitTileChunkPartition) {
                                Debug.LogError("Attempted to load non-conduit partition into conduit system");
                                continue;
                            }
                            ports.AddRange(((IConduitTileChunkPartition) partition).getEntityPorts(conduitType,chunkFrameOfReference));
                        }
                    }
                    chunkConduitPortData[new Vector2Int(x,y)] = ports;
                }
            }
            return chunkConduitPortData;
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
            if (!conduitSystemsDict.ContainsKey(tileMapType)) {
                Debug.LogError("ConduitTileClosedChunkSystem did not have " + conduitType.ToString() + " inside managed conduit systems");
                return null;
            }
            return conduitSystemsDict[tileMapType];
        }

        public Vector2Int getBottomLeftCorner() {
            return new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
        }
    }
}
