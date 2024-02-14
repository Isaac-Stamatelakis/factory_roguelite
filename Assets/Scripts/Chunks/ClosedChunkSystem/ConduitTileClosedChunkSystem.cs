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
            conduitSystemsDict[TileMapType.ItemConduit] = new ConduitSystemManager(ConduitType.Item);
            conduitSystemsDict[TileMapType.FluidConduit] = new ConduitSystemManager(ConduitType.Fluid);
            conduitSystemsDict[TileMapType.EnergyConduit] = new ConduitSystemManager(ConduitType.Energy);
            conduitSystemsDict[TileMapType.SignalConduit] = new ConduitSystemManager(ConduitType.Signal);
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
            if (chunkPartition is not IConduitTileChunkPartition) {
                Debug.LogError("Tried to load conduits for non-conduit partition " + chunkPartition.getRealPosition());
                return;
            }
            IConduitTileChunkPartition conduitTileChunkPartition = (IConduitTileChunkPartition) chunkPartition;
            
            IConduit[,] itemConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Item);
            ConduitSystemManager itemConduitSystemManager = conduitSystemsDict[TileMapType.ItemConduit];
            conduitTileChunkPartition.setConduits(TileMapLayer.Item,itemConduits);
            itemConduitSystemManager.addConduitPartition(itemConduits);

            IConduit[,] fluidConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Fluid);
            ConduitSystemManager fluidConduitSystemManager = conduitSystemsDict[TileMapType.ItemConduit];
            conduitTileChunkPartition.setConduits(TileMapLayer.Fluid,itemConduits);
            fluidConduitSystemManager.addConduitPartition(itemConduits);

            IConduit[,] energyConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Energy);
            ConduitSystemManager energyConduitSystemManager = conduitSystemsDict[TileMapType.ItemConduit];
            conduitTileChunkPartition.setConduits(TileMapLayer.Energy,itemConduits);
            energyConduitSystemManager.addConduitPartition(itemConduits);

            IConduit[,] signalConduits = ConduitFactory.deseralizePartition(chunkPartition,TileMapLayer.Signal);
            ConduitSystemManager signalConduitSystemManager = conduitSystemsDict[TileMapType.ItemConduit];
            conduitTileChunkPartition.setConduits(TileMapLayer.Signal,itemConduits);
            signalConduitSystemManager.addConduitPartition(itemConduits);
        }
    }

}
