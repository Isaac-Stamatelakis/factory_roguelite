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
using ItemModule;

namespace ChunkModule.PartitionModule {
    public interface IConduitTileChunkPartition {
        public void getConduits(ConduitType conduitType,IConduit[,] systemConduits, Vector2Int referenceChunk);
        public bool getConduitLoaded();
        public void setConduitLoaded(bool val);
        public void loadTickableTileEntities();
        public Dictionary<TileEntity, List<TileEntityPort>> getEntityPorts(ConduitType conduitType,Vector2Int referenceChunk);
        public void setConduits(Dictionary<ConduitType, IConduit[,]> conduits);
        public ConduitItem getConduitItemAtPosition(Vector2Int positionInPartition, ConduitType type);
        public void setConduitItem(Vector2Int position, ConduitType type, ConduitItem item);
        public void activate(ILoadedChunk loadedChunk);
    }
    public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData>, IConduitTileChunkPartition where T : SerializedTileConduitData
    {
        protected bool tickLoaded;
        protected Dictionary<ConduitType, IConduit[,]> conduits;
        private Dictionary<TileMapLayer, IConduit[,]> conduitArrayDict = new Dictionary<TileMapLayer, IConduit[,]>();
        public ConduitChunkPartition(SerializedTileConduitData data, Vector2Int position, IChunk parent) : base(data, position, parent)
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
                tileEntities = new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            
            loadTickableTileEntityLayer(data.baseData);
            tickLoaded = true;
        }

        public Dictionary<TileEntity, List<TileEntityPort>> getEntityPorts(ConduitType type, Vector2Int referenceFrame) {
            Dictionary<TileEntity, List<TileEntityPort>> ports = new Dictionary<TileEntity, List<TileEntityPort>>();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    TileEntity tileEntity = tileEntities[x,y];
                    if (tileEntity == null) {
                        continue;
                    }
                    if (tileEntity is not IConduitInteractable) {
                        continue;
                    }
                    ConduitPortLayout layout = ((IConduitInteractable) tileEntity).getConduitPortLayout();
                    if (layout == null) {
                        continue;
                    }
                    List<TileEntityPort> entityPorts = null;
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
                    ports[tileEntity] = entityPorts;
                }
            }
            return ports;
        }  
        private void loadTickableTileEntityLayer(SerializedBaseTileData data) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = data.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.getTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    TileEntity tileEntity = tileItem.tileEntity;
                    if (tileEntity != null) {
                        tileEntities[x,y] = placeTickableTileEntity(tileItem,data.sTileEntityOptions[x,y],new Vector2Int(x,y));
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
                    string id = data.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    string options = data.conduitOptions[x,y];
                    int systemX = x+partitionOffset.x-referenceChunk.x;
                    int systemY = y+partitionOffset.y-referenceChunk.y;
                    IConduit conduit = ConduitFactory.deseralize(systemX,systemY,id,options,itemRegistry,tileEntities[x,y]);
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
            
            if (conduits != null) {
                foreach (KeyValuePair<ConduitType, IConduit[,]> kvp in conduits) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            IConduit conduit = kvp.Value[x,y];
                            switch (kvp.Key) {
                                case ConduitType.Item:
                                    data.itemConduitData.conduitOptions[x,y] = ConduitPortFactory.serialize(conduit);
                                    break;
                                case ConduitType.Fluid:
                                    data.fluidConduitData.conduitOptions[x,y] = ConduitPortFactory.serialize(conduit);
                                    break;
                                case ConduitType.Energy:
                                    data.energyConduitData.conduitOptions[x,y] = ConduitPortFactory.serialize(conduit);
                                    break;
                                case ConduitType.Signal:
                                    data.signalConduitData.conduitOptions[x,y] = ConduitPortFactory.serialize(conduit);
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
            string itemID = data.itemConduitData.ids[x,y];
            if (itemID != null) {
                place(
                    id: itemID, 
                    sConduitOptions: data.itemConduitData.conduitOptions[x,y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Item
                );
            }

            string fluidID = data.fluidConduitData.ids[x,y];
            if (fluidID != null) {
                place(
                    id: fluidID, 
                    sConduitOptions: data.fluidConduitData.conduitOptions[x,y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Fluid
                );
            }

            string energyID = data.energyConduitData.ids[x,y];
            if (energyID != null) {
                place(
                    id: energyID, 
                    sConduitOptions: data.energyConduitData.conduitOptions[x,y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Energy
                );
            }

            string signalID = data.signalConduitData.ids[x,y];
            if (signalID != null) {
                place(
                    id: signalID, 
                    sConduitOptions: data.signalConduitData.conduitOptions[x,y],
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
            tileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                conduitItem
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

       

        public void setConduits(Dictionary<ConduitType, IConduit[,]> conduits)
        {
            this.conduits = conduits;
        }

        public ConduitItem getConduitItemAtPosition(Vector2Int positionInPartition, ConduitType type)
        {
            IConduit[,] conduitArray = conduits[type];
            return conduitArray[positionInPartition.x,positionInPartition.y].getConduitItem();
        }

        public void setConduitItem(Vector2Int position, ConduitType type, ConduitItem item)
        {
            string id = null;
            if (item != null) {
                id = item.id;
            }
            SerializedTileConduitData serializedTileConduitData = (SerializedTileConduitData)getData();
            switch (type) {
                case ConduitType.Item:
                    serializedTileConduitData.itemConduitData.ids[position.x,position.y] = id;
                    break;
                case ConduitType.Fluid:
                    serializedTileConduitData.fluidConduitData.ids[position.x,position.y] = id;
                    break;
                case ConduitType.Energy:
                    serializedTileConduitData.energyConduitData.ids[position.x,position.y] = id;
                    break;
                case ConduitType.Signal:
                    serializedTileConduitData.signalConduitData.ids[position.x,position.y] = id;
                    break;

            }
        }

        public void activate(ILoadedChunk loadedChunk)
        {
            this.parent = loadedChunk;
            foreach (TileEntity tileEntity in tileEntities) {
                if (tileEntity == null) {
                    continue;
                }
                tileEntity.setChunk(loadedChunk);
            }
        }
    }
}
