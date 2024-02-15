using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileMapModule;
using TileMapModule.Layer;
using TileMapModule.Type;
using ConduitModule;
using ChunkModule.IO;
using ConduitModule.Ports;

namespace ChunkModule.PartitionModule {
    public interface IConduitTileChunkPartition {
        public void getConduits(ConduitType conduitType,IConduit[,] systemConduits, Vector2Int referenceChunk);
        public bool getConduitLoaded();
        public void setConduitLoaded(bool val);
        public void loadTickableTileEntities();
        public List<ConduitPortData> getEntityPorts(ConduitType conduitType,Vector2Int referenceChunk);
    }
    public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData>, IConduitTileChunkPartition where T : SerializedTileConduitData
    {
        protected bool tickLoaded;
        private Dictionary<TileMapLayer, IConduit[,]> conduitArrayDict = new Dictionary<TileMapLayer, IConduit[,]>();
        public ConduitChunkPartition(SerializedTileConduitData data, Vector2Int position, Chunk parent) : base(data, position, parent)
        {
        }

        public void getConduits(ConduitType conduitType, IConduit[,] systemConduits, Vector2Int referenceChunk)
        {
            SerializedTileConduitData serializedTileConduitData = (SerializedTileConduitData) data;
            switch (conduitType) {
                case ConduitType.Item:
                    getConduitsFromData(serializedTileConduitData.itemConduitData,systemConduits,referenceChunk);
                    return;
                case ConduitType.Fluid:
                    getConduitsFromData(serializedTileConduitData.fluidConduitData,systemConduits,referenceChunk);
                    return;
                case ConduitType.Energy:
                    getConduitsFromData(serializedTileConduitData.energyConduitData,systemConduits,referenceChunk);
                    return;
                case ConduitType.Signal:
                    getConduitsFromData(serializedTileConduitData.signalConduitData,systemConduits,referenceChunk);
                    return;
            }
            Debug.LogError("ConduitTileChunkPartition method 'getConduits' did not handle case for type '" + conduitType.ToString() + "'");
        }
        /// <summary>
        /// Loads Tile Entities which are tickable. Note these are always active even if the partition is not in sight of the player
        /// </summary>
        public void loadTickableTileEntities() {
            if (tickLoaded) {
                Debug.LogError("Attempted to tick load partition which is already ticked loaded");
                return;
            }
            if (tileEntities == null) {
                tileEntities = new Dictionary<TileMapLayer, TileEntity[,]>();
            }
            
            loadTickableTileEntityLayer(TileMapLayer.Base,data.baseData);
            loadTickableTileEntityLayer(TileMapLayer.Background,data.backgroundData);
            tickLoaded = true;
        }

        public List<ConduitPortData> getEntityPorts(ConduitType type, Vector2Int referenceFrame) {
            List<ConduitPortData> ports = new List<ConduitPortData>();
            foreach (TileMapLayer layer in tileEntities.Keys) {
                TileEntity[,] entityArray = tileEntities[layer];
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        TileEntity tileEntity = entityArray[x,y];
                        if (tileEntity == null) {
                            continue;
                        }
                        if (tileEntity is not IConduitInteractable) {
                            continue;
                        }
                        ConduitPortLayout layout = ((IConduitInteractable) tileEntity).getConduitPortLayout();
                        List<ConduitPortData> entityPorts = null;
                        switch (type) {
                            case ConduitType.Item:
                                entityPorts = layout.itemPorts;
                                break;
                            case ConduitType.Fluid:
                                entityPorts = layout.fluidPorts;
                                break;
                            case ConduitType.Energy:
                                entityPorts = layout.energyPorts;
                                break;
                            case ConduitType.Signal:
                                entityPorts = layout.signalPorts;
                                break;
                        }
                        if (entityPorts == null) {
                            continue;
                        }

                        foreach (ConduitPortData conduitPortData in entityPorts) {
                            ConduitPortData positionedPort = new ConduitPortData(
                                conduitPortData.portType,
                                new Vector2Int(x,y)+getRealPosition() * Global.ChunkPartitionSize - referenceFrame
                            );
                            ports.Add(positionedPort);
                        }
                    }
                }
            }
            return ports;
        }  
        private void loadTickableTileEntityLayer(TileMapLayer layer, SeralizedChunkTileData data) {
            if (!tileEntities.ContainsKey(layer)) {
                tileEntities[layer] = new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            TileEntity[,] tileEntityArray = tileEntities[layer];
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = data.ids[x][y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.getTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    TileEntity tileEntity = tileItem.tileEntity;
                    if (tileEntity != null) {
                        tileEntityArray[x,y] = placeTickableTileEntity(tileItem,data.sTileEntityOptions[x][y],new Vector2Int(x,y));
                    }
                    
                }
            }
        }
        protected TileEntity placeTickableTileEntity(TileItem tileItem, string options, Vector2Int positionInPartition) {
            if (tileItem.tileEntity is not ITickableTileEntity) {
                return null;
            }
            return placeTileEntity(tileItem,options,positionInPartition);
        }
        protected override void placeTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, TileEntity[,] tileEntityArray, int x, int y)
        {
            if (tileItem.tileEntity is ITickableTileEntity) {
                return;
            }
            base.placeTileEntityFromLoad(tileItem, options, positionInPartition, tileEntityArray, x, y);
        }

        protected override bool unloadTileEntity(TileEntity[,] array, int x, int y)
        {
            TileEntity tileEntity = array[x,y];
            if (tileEntity is ITickableTileEntity) {
                return false;
            }
            return base.unloadTileEntity(array,x,y);
        }
        private void getConduitsFromData(SeralizedChunkConduitData data,IConduit[,] systemConduits,Vector2Int referenceChunk) {
            IConduit[,] conduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Vector2Int partitionOffset = getRealPosition()*Global.ChunkPartitionSize;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = data.ids[x][y];
                    if (id == null) {
                        continue;
                    }
                    string options = data.conduitOptions[x][y];
                    int systemX = x+partitionOffset.x-referenceChunk.x;
                    int systemY = y+partitionOffset.y-referenceChunk.y;
                    IConduit conduit = ConduitFactory.deseralize(systemX,systemY,getRealPosition(),id,options,itemRegistry);
                    systemConduits[systemX,systemY] = conduit;
                }
            }
        }

        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle)
        {
            yield return base.load(tileGridMaps,angle);
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
        protected override void unloadTileEntities()
        {
            
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

        public bool getConduitLoaded()
        {
            return tickLoaded;
        }

        public void setConduitLoaded(bool val)
        {
            tickLoaded = val;
        }
    }
}
