using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using ConduitModule.ConduitSystemModule;
using ChunkModule.PartitionModule;
using ConduitModule;
using TileMapModule.Layer;
using ChunkModule.IO;


namespace ChunkModule.ClosedChunkSystemModule {
    public class ConduitTileClosedChunkSystem : ClosedChunkSystem
    {
        private Dictionary<TileMapType, ConduitSystemManager> conduitSystemsDict; 
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

        private void initConduitSystemManagers() {
            conduitSystemsDict = new Dictionary<TileMapType, ConduitSystemManager>();
            initConduitSystemManager(TileMapType.ItemConduit);
            //conduitSystemsDict[TileMapType.FluidConduit] = new ConduitSystemManager(ConduitType.Fluid);
            //conduitSystemsDict[TileMapType.EnergyConduit] = new ConduitSystemManager(ConduitType.Energy);
            //conduitSystemsDict[TileMapType.SignalConduit] = new ConduitSystemManager(ConduitType.Signal);
        }
        public void addToConduitSystem(ConduitItem conduitItem, IConduitOptions conduitOptions, Vector2Int position) {

        }

        private void initConduitSystemManager(TileMapType conduitMapType) {
            ConduitType conduitType = conduitMapType.toConduitType();
            conduitSystemsDict[conduitMapType] = new ConduitSystemManager(conduitType,getConduits(conduitType),getSize());
        }

        private IConduit[,] getConduits(ConduitType conduitType) {
            Vector2Int size = getSize();
            Vector2Int chunkFrameOfReference = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
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
    }

}
