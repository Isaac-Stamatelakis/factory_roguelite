using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileMapModule;
using TileMapModule.Layer;
using TileMapModule.Type;
using ConduitModule;

namespace ChunkModule.PartitionModule {
    public interface IConduitTileChunkPartition {
        public void setConduits(TileMapLayer layer, IConduit[,] conduits);
    }
    public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData>, IConduitTileChunkPartition where T : SerializedTileConduitData
    {
        private Dictionary<TileMapLayer, IConduit[,]> conduitArrayDict = new Dictionary<TileMapLayer, IConduit[,]>();
        public ConduitChunkPartition(SerializedTileConduitData data, Vector2Int position, Chunk parent) : base(data, position, parent)
        {
        }

        public Dictionary<TileMapLayer, IConduit[,]> Conduits { get => conduitArrayDict; set => conduitArrayDict = value; }

        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle)
        {
            yield return base.load(tileGridMaps,angle);
            Conduits = new Dictionary<TileMapLayer, IConduit[,]>();

        }

        public override void save(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            base.save(tileGridMaps);
            Vector2Int position = getRealPosition();
            SerializedTileConduitData data = (SerializedTileConduitData) getData();
            
            // Clear data
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    data.itemConduitData.ids[x][y] = null;
                    data.itemConduitData.conduitOptions[x][y] = null;
                    data.fluidConduitData.ids[x][y] = null;
                    data.fluidConduitData.conduitOptions[x][y] = null;
                    data.energyConduitData.ids[x][y] = null;
                    data.energyConduitData.conduitOptions[x][y] = null;
                    data.signalConduitData.ids[x][y] = null;
                    data.signalConduitData.conduitOptions[x][y] = null;
                }
            }
            // Iterate through tilemaps
            foreach (ITileMap tileMap in tileGridMaps.Values) {
                TileMapType tileMapType = tileMap.getType();
                if (!tileMapType.isConduit()) { // type is valid tile type
                    continue;
                }
                // get layer to serialze in (base or background)
                TileMapLayer layer = tileMapType.toLayer();
                IPlacedItemObject[,] tileItemdata = tileMap.getPartitionData(position);
                if (tileItemdata == null) {
                    continue;
                }
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        if (tileItemdata[x,y] != null) {
                            ConduitData conduitData = (ConduitData) tileItemdata[x,y];
                            ConduitItem conduitItem = (ConduitItem) conduitData.getItemObject();
                            switch (layer) {
                                case TileMapLayer.Item:
                                    data.itemConduitData.ids[x][y] = conduitItem.id;
                                    data.itemConduitData.conduitOptions[x][y] = null;
                                    break;
                                case TileMapLayer.Fluid:
                                    data.fluidConduitData.ids[x][y] = conduitItem.id;
                                    data.fluidConduitData.conduitOptions[x][y] = null;
                                    break;
                                case TileMapLayer.Energy:
                                    data.energyConduitData.ids[x][y] = conduitItem.id;
                                    data.energyConduitData.conduitOptions[x][y] = null;
                                    break;
                                case TileMapLayer.Signal:
                                    data.signalConduitData.ids[x][y] = conduitItem.id;
                                    data.signalConduitData.conduitOptions[x][y] = null;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void setConduits(TileMapLayer layer,IConduit[,] conduits)
        {
            conduitArrayDict[layer] = conduits;
        }

        public override IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            yield return base.unload(tileGridMaps);
            foreach (IConduit[,] array in conduitArrayDict.Values) {
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        array[x,y] = null;
                    }
                }
            }
            conduitArrayDict = null;
        }

        public override IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            return base.unloadTiles(tileGridMaps);
        }

        protected override void iterateLoad(int x, int y, ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition)
        {
            base.iterateLoad(x, y, itemRegistry, tileGridMaps, realPosition);
            Vector2Int partitionPosition = new Vector2Int(x,y);
            SerializedTileConduitData data = (SerializedTileConduitData) getData();
            string itemID = data.itemConduitData.ids[x][y];
            if (itemID != null) {
                place(
                    id: itemID, 
                    sConduitOptions: data.itemConduitData.conduitOptions[x][y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Item
                );
            }

            string fluidID = data.fluidConduitData.ids[x][y];
            if (fluidID != null) {
                place(
                    id: fluidID, 
                    sConduitOptions: data.fluidConduitData.conduitOptions[x][y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Fluid
                );
            }

            string energyID = data.energyConduitData.ids[x][y];
            if (energyID != null) {
                place(
                    id: energyID, 
                    sConduitOptions: data.energyConduitData.conduitOptions[x][y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Energy
                );
            }

            string signalID = data.signalConduitData.ids[x][y];
            if (signalID != null) {
                place(
                    id: signalID, 
                    sConduitOptions: data.signalConduitData.conduitOptions[x][y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Signal
                );
            }
        }

        private void place(string id, string sConduitOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return;
            }
            ITileMap tileGridMap = tileGridMaps[conduitItem.getType().toTileMapType()];
            ConduitData conduitData = new ConduitData(conduitItem);
            tileGridMap.placeTileAtLocation(
                realPosition,
                positionInPartition,
                conduitData
            );
        }
    }
}
