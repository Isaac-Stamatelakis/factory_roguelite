using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using TileMapModule.Layer;
using ChunkModule.PartitionModule;
using TileEntityModule;

namespace ConduitModule {
    public static class ConduitFactory {
        public static IConduit deseralize(int x, int y, string id, string conduitOptionData, ItemRegistry itemRegistry, TileEntity tileEntity) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return null;
            }
            ConduitType conduitType = conduitItem.getType();
            IConduitPort port = ConduitPortFactory.deseralize(conduitOptionData,conduitType,tileEntity);
            switch (conduitType) {
                case ConduitType.Item:
                    return new ItemConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: (ItemConduitPort) port
                    );
                case ConduitType.Fluid:
                    return new FluidConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Energy:
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Signal:
                    return new SignalConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: port
                    );    
            }
            return null;
            
        }

        public static IConduit[,] deseralizePartition(IChunkPartition partition, TileMapLayer layer, TileEntity[,] tileEntities) {
            if (partition is not ConduitChunkPartition<SerializedTileConduitData>) {
                Debug.LogError("Partition which was not conduit partition passed into Conduit Factory deseralizePartition " + partition.getRealPosition());
                return null;
            }
            SerializedTileConduitData data = (SerializedTileConduitData) partition.getData();
            Vector2Int partitionOffset = partition.getRealPosition() * Global.ChunkPartitionSize;
            switch (layer) {
                case TileMapLayer.Item:
                    return deseralizePartitionData(data.itemConduitData.ids,data.itemConduitData.conduitOptions,partitionOffset,tileEntities);
                case TileMapLayer.Fluid:
                    return deseralizePartitionData(data.fluidConduitData.ids,data.fluidConduitData.conduitOptions,partitionOffset,tileEntities);
                case TileMapLayer.Energy:
                    return deseralizePartitionData(data.energyConduitData.ids,data.energyConduitData.conduitOptions,partitionOffset,tileEntities);
                case TileMapLayer.Signal:
                    return deseralizePartitionData(data.signalConduitData.ids,data.signalConduitData.conduitOptions,partitionOffset,tileEntities);
            }
            return null;
        }
        private static IConduit[,] deseralizePartitionData(List<List<string>> ids, List<List<string>> conduitOptionDataList, Vector2Int partitionOffset, TileEntity[,] tileEntities) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            IConduit[,] conduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = ids[x][y];
                    if (id == null) {
                        continue;
                    }
                    string conduitOptionData = conduitOptionDataList[x][y];
                    IConduit conduit = deseralize(x + partitionOffset.x,y + partitionOffset.y, id, conduitOptionData, itemRegistry, tileEntities[x,y]);
                    conduits[x,y] = conduit;
                }
            }
            return conduits;
        }

        /// <summary>
        /// Sets the port of given conduit to default port
        /// </summary
        public static IConduit create(ConduitItem conduitItem, EntityPortType portType, int x, int y, TileEntity tileEntity) {

            ConduitType conduitType = conduitItem.getType();
            switch (conduitType) {
                case ConduitType.Item:
                    ItemConduitPort itemConduitPort = (ItemConduitPort)ConduitPortFactory.createDefault(conduitType,portType,tileEntity);
                    return new ItemConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: itemConduitPort
                    );
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            return null;
        }
    }
}
