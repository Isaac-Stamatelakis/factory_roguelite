using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.PartitionModule;
using TileMapModule.Layer;

namespace ConduitModule {
    public interface IConduit {
    
    }
    public class Conduit : IConduit
    {
        private int x;
        private int y;
        private int partitionX;
        private int partitionY;
        private ConduitItem conduitItem;
        private IConduitOptions conduitOptions;
        public Conduit(int x, int y, int partitionX, int partitionY, ConduitItem conduitItem, IConduitOptions conduitOptions) {
            this.X = x;
            this.Y = y;
            this.partitionX = partitionX;
            this.partitionY = partitionY;
            this.ConduitOptions = conduitOptions;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public ConduitItem ConduitItem { get => conduitItem; set => conduitItem = value; }
        public IConduitOptions ConduitOptions { get => conduitOptions; set => conduitOptions = value; }
        public int PartitionX { get => partitionX; set => partitionX = value; }
        public int PartitionY { get => partitionY; set => partitionY = value; }
    }
    public static class ConduitFactory {
        public static Conduit deseralize(int x, int y, int partitionX, int partitionY, string id, string conduitOptionData, ItemRegistry itemRegistry) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return null;
            }
            IConduitOptions conduitOptions = ConduitOptionsFactory.deseralizeOption(conduitItem,conduitOptionData);
            return new Conduit(
                x: x,
                y: y,
                partitionX: partitionX,
                partitionY: partitionY,
                conduitItem: conduitItem,
                conduitOptions: conduitOptions
            );
        }

        public static Conduit[,] deseralizePartition(IChunkPartition partition, TileMapLayer layer) {
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
        private static Conduit[,] deseralizePartitionData(List<List<string>> ids, List<List<string>> conduitOptionDataList, Vector2Int partitionOffset) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Conduit[,] conduits = new Conduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = ids[x][y];
                    if (id == null) {
                        continue;
                    }
                    string conduitOptionData = conduitOptionDataList[x][y];
                    conduits[x,y] = deseralize(x + partitionOffset.x,y + partitionOffset.y, x, y, id, conduitOptionData, itemRegistry);
                }
            }
            return conduits;
        }
    }

}
