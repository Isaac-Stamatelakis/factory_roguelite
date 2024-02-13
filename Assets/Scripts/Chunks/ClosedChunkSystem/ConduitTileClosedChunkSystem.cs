using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using ConduitModule.ConduitSystemModule;
using ChunkModule.PartitionModule;
using ConduitModule;
using TileMapModule.Layer;

namespace ChunkModule.ClosedChunkSystemModule {
    public class ConduitTileClosedChunkSystem : TileClosedChunkSystem
    {
        private Dictionary<TileMapType, ConduitSystemManager> conduitSystemsDict; 
        public override void Awake()
        {
            initConduitSystemManagers();
            initTileMapContainer(TileMapType.ItemConduit);
            initTileMapContainer(TileMapType.FluidConduit);
            initTileMapContainer(TileMapType.EnergyConduit);
            initTileMapContainer(TileMapType.SignalConduit);
            base.Awake();
        }

        private void initConduitSystemManagers() {
            conduitSystemsDict = new Dictionary<TileMapType, ConduitSystemManager>();
            conduitSystemsDict[TileMapType.ItemConduit] = new ConduitSystemManager();
            conduitSystemsDict[TileMapType.FluidConduit] = new ConduitSystemManager();
            conduitSystemsDict[TileMapType.EnergyConduit] = new ConduitSystemManager();
            conduitSystemsDict[TileMapType.SignalConduit] = new ConduitSystemManager();
        }
        public void addToConduitSystem(ConduitItem conduitItem, IConduitOptions conduitOptions, Vector2Int position) {

        }
        public override void initalize(Transform dimTransform, IntervalVector coveredArea, int dim)
        {
            base.initalize(dimTransform, coveredArea, dim);
        }


        public override IEnumerator loadChunkPartition(IChunkPartition chunkPartition, double angle)
        {
            yield return base.loadChunkPartition(chunkPartition, angle);
            loadPartitionIntoConduitSystem(chunkPartition);
        }

        public override IEnumerator unloadChunkPartition(IChunkPartition chunkPartition)
        {
            yield return base.unloadChunkPartition(chunkPartition);
            
        }

        private void loadPartitionIntoConduitSystem(IChunkPartition chunkPartition) {
            Conduit[,] itemConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Item);
            ConduitSystemManager itemConduitSystemManager = conduitSystemsDict[TileMapType.ItemConduit];
            itemConduitSystemManager.addConduitPartition(itemConduits);

            Conduit[,] fluidConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Fluid);

            Conduit[,] energyConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Energy);

            Conduit[,] signalConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Signal);
        }
    }

}
