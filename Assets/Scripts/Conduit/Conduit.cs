using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.PartitionModule;
using TileMapModule.Layer;
using ConduitModule.Ports;

namespace ConduitModule {
    public interface IConduit {
        
        public int getX();
        public int getY();
        public int getPartitionX();
        public int getPartitionY();
        public ConduitItem getConduitItem();
        public IConduitPort getPort();
        public string getId();

    }
    public abstract class Conduit<Options> : IConduit where Options : IConduitOptions
    {
        private int x;
        private int y;
        private int partitionX;
        private int partitionY;
        private ConduitItem conduitItem;
        private IConduitPort port;
        public Conduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitPort port) {
            this.x = x;
            this.y = y;
            this.partitionX = partitionX;
            this.partitionY = partitionY;
            this.conduitItem = conduitItem;
            this.port = port;
        }

        public ConduitItem getConduitItem()
        {
            return conduitItem;
        }

        public IConduitPort getPort()
        {
            return port;
        }

        public string getId()
        {
            return conduitItem.id;
        }

        public int getPartitionX()
        {
            return partitionX;
        }

        public int getPartitionY()
        {
            return partitionY;
        }

        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }
    }

    public class ItemConduit : Conduit<ItemConduitOptions>
    {
        public ItemConduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitPort port) : base(x, y, partitionX, partitionY, conduitItem, port)
        {
        }
    }

    public class FluidConduit : Conduit<FluidItemConduitOptions>
    {
        public FluidConduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitPort port) : base(x, y, partitionX, partitionY, conduitItem, port)
        {
        }
    }

    public class SignalConduit : Conduit<SignalConduitOptions>
    {
        public SignalConduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitPort port) : base(x, y, partitionX, partitionY, conduitItem, port)
        {
        }
    }
    public class EnergyConduit : Conduit<EnergyConduitOptions>
    {
        public EnergyConduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitPort port) : base(x, y, partitionX, partitionY, conduitItem, port)
        {
        }
    }
    public static class ConduitFactory {
        public static IConduit deseralize(int x, int y, Vector2Int partitionPosition, string id, string conduitOptionData, ItemRegistry itemRegistry) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return null;
            }
            ConduitType conduitType = conduitItem.getType();
            IConduitPort port = ConduitPortFactory.deseralize(conduitOptionData,conduitType);
            switch (conduitType) {
                case ConduitType.Item:
                    return new ItemConduit(
                        x: x,
                        y: y,
                        partitionX: partitionPosition.x,
                        partitionY: partitionPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Fluid:
                    return new FluidConduit(
                        x: x,
                        y: y,
                        partitionX: partitionPosition.x,
                        partitionY: partitionPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Energy:
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        partitionX: partitionPosition.x,
                        partitionY: partitionPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Signal:
                    return new SignalConduit(
                        x: x,
                        y: y,
                        partitionX: partitionPosition.x,
                        partitionY: partitionPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );    
            }
            return null;
            
        }

        public static IConduit[,] deseralizePartition(IChunkPartition partition, TileMapLayer layer) {
            if (partition is not ConduitChunkPartition<SerializedTileConduitData>) {
                Debug.LogError("Partition which was not conduit partition passed into Conduit Factory deseralizePartition " + partition.getRealPosition());
                return null;
            }
            SerializedTileConduitData data = (SerializedTileConduitData) partition.getData();
            Vector2Int partitionOffset = partition.getRealPosition() * Global.ChunkPartitionSize;
            switch (layer) {
                case TileMapLayer.Item:
                    return deseralizePartitionData(data.itemConduitData.ids,data.itemConduitData.conduitOptions,partitionOffset);
                case TileMapLayer.Fluid:
                    return deseralizePartitionData(data.fluidConduitData.ids,data.fluidConduitData.conduitOptions,partitionOffset);
                case TileMapLayer.Energy:
                    return deseralizePartitionData(data.energyConduitData.ids,data.energyConduitData.conduitOptions,partitionOffset);
                case TileMapLayer.Signal:
                    return deseralizePartitionData(data.signalConduitData.ids,data.signalConduitData.conduitOptions,partitionOffset);
            }
            return null;
        }
        private static IConduit[,] deseralizePartitionData(List<List<string>> ids, List<List<string>> conduitOptionDataList, Vector2Int partitionOffset) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            IConduit[,] conduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = ids[x][y];
                    if (id == null) {
                        continue;
                    }
                    string conduitOptionData = conduitOptionDataList[x][y];
                    IConduit conduit = deseralize(x + partitionOffset.x,y + partitionOffset.y, partitionOffset, id, conduitOptionData, itemRegistry);
                    conduits[x,y] = conduit;
                }
            }
            return conduits;
        }
    }

}
