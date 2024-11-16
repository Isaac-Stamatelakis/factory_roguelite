using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Type;
using Conduits;
using Chunks.IO;
using Conduits.Ports;
using Items;
using TileEntityModule.Instances.CompactMachines;
using Chunks.Systems;
using Tiles;

namespace Chunks.Partitions {
    public interface IConduitTileChunkPartition {
        public void getConduits(ConduitType conduitType,IConduit[,] systemConduits, Vector2Int referenceChunk,Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPorts);
        public bool getConduitLoaded();
        public void setConduitLoaded(bool val);
        public void softLoadTileEntities();
        public Dictionary<ITileEntityInstance, List<TileEntityPort>> getEntityPorts(ConduitType conduitType,Vector2Int referenceChunk);
        public void setConduits(Dictionary<ConduitType, IConduit[,]> conduits);
        public ConduitItem getConduitItemAtPosition(Vector2Int positionInPartition, ConduitType type);
        public void setConduitItem(Vector2Int position, ConduitType type, ConduitItem item);
        public void activate(ILoadedChunk loadedChunk);
        public void syncToCompactMachine(CompactMachineInstance compactMachine);
        public void assembleMultiBlocks();
    }
    public class ConduitChunkPartition<T> : TileChunkPartition<WorldTileConduitData>, IConduitTileChunkPartition where T : WorldTileConduitData
    {
        protected bool tickLoaded;
        protected Dictionary<ConduitType, IConduit[,]> conduits;
        private Dictionary<TileMapLayer, IConduit[,]> conduitArrayDict = new Dictionary<TileMapLayer, IConduit[,]>();
        public ConduitChunkPartition(WorldTileConduitData data, Vector2Int position, IChunk parent) : base(data, position, parent)
        {
        }

        public void getConduits(ConduitType conduitType, IConduit[,] systemConduits, Vector2Int referenceChunk, Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPorts)
        {
            WorldTileConduitData serializedTileConduitData = (WorldTileConduitData) data;
            switch (conduitType) {
                case ConduitType.Item:
                    getConduitsFromData(serializedTileConduitData.itemConduitData,systemConduits,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Fluid:
                    getConduitsFromData(serializedTileConduitData.fluidConduitData,systemConduits,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Energy:
                    getConduitsFromData(serializedTileConduitData.energyConduitData,systemConduits,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Signal:
                    getConduitsFromData(serializedTileConduitData.signalConduitData,systemConduits,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Matrix:
                    getConduitsFromData(serializedTileConduitData.matrixConduitData,systemConduits,referenceChunk,tileEntityPorts);
                    return;
            }
            Debug.LogError("ConduitTileChunkPartition method 'getConduits' did not handle case for type '" + conduitType.ToString() + "'");
        }
        /// <summary>
        /// Loads Tile Entities which are tickable. Note these are always active even if the partition is not in sight of the player
        /// </summary>
        public void softLoadTileEntities() {
            if (tickLoaded) {
                Debug.LogError("Attempted to tick load partition which is already ticked loaded");
                return;
            }
            if (tileEntities == null) {
                tileEntities = new ITileEntityInstance[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            
            loadTickableTileEntityLayer(data.baseData);
            tickLoaded = true;
        }

        

        public Dictionary<ITileEntityInstance, List<TileEntityPort>> getEntityPorts(ConduitType type, Vector2Int referenceFrame) {
            Dictionary<ITileEntityInstance, List<TileEntityPort>> ports = new Dictionary<ITileEntityInstance, List<TileEntityPort>>();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    ITileEntityInstance tileEntity = tileEntities[x,y];
                    if (tileEntity == null) {
                        continue;
                    }
                    if (tileEntity is not IConduitInteractable conduitInteractable) {
                        continue;
                    }
                    ConduitPortLayout layout = conduitInteractable.getConduitPortLayout();
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
                        case ConduitType.Matrix:
                            entityPorts = layout.matrixPorts;
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
                        tileEntities[x,y] = placeSoftLoadableTileEntity(tileItem,data.sTileEntityOptions[x,y],new Vector2Int(x,y));
                    }
                }
            }
        }
        protected ITileEntityInstance placeSoftLoadableTileEntity(TileItem tileItem, string options, Vector2Int positionInPartition) {
            if (!tileItem.tileEntity.SoftLoadable) {
                return null;
            }
            Vector2Int position = this.position * Global.ChunkPartitionSize + positionInPartition;
            return TileEntityHelper.placeTileEntity(tileItem,position,parent,false);
        }
        protected override void placeTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, ITileEntityInstance[,] tileEntityArray, int x, int y)
        {
            base.placeTileEntityFromLoad(tileItem, options, positionInPartition, tileEntityArray, x, y);
        }

        private void getConduitsFromData(SeralizedChunkConduitData data,IConduit[,] systemConduits,Vector2Int referenceChunk, Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityPorts) {
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
                    ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
                    if (conduitItem == null) {
                        continue;
                    }
                    Vector2Int cellPosition = new Vector2Int(x,y)+partitionOffset;
                    ITileEntityInstance tileEntity = null;
                    EntityPortType? port = null;
                    foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in tileEntityPorts) {
                        if (tileEntity != null) {
                            break;
                        }
                        foreach (TileEntityPort tileEntityPort in kvp.Value) {
                            if (kvp.Key.getCellPosition() + tileEntityPort.position == cellPosition) {
                                tileEntity = kvp.Key;
                                port = tileEntityPort.portType;
                                break;
                            }
                        }
                    }
                    systemConduits[systemX,systemY] = ConduitFactory.DeserializeConduit(
                        cellPosition: cellPosition,
                        referencePosition: referenceChunk,
                        conduitItem: conduitItem,
                        conduitOptionData: data.conduitOptions[x,y],
                        tileEntity : tileEntity,
                        portType: port
                    );
                    
                    
                }
            }
        }

        public override void save()
        {
            base.save();
            Vector2Int position = getRealPosition();
            WorldTileConduitData data = (WorldTileConduitData) getData();
            
            if (conduits != null) {
                foreach (KeyValuePair<ConduitType, IConduit[,]> kvp in conduits) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            IConduit conduit = kvp.Value[x,y];
                            switch (kvp.Key) {
                                case ConduitType.Item:
                                    data.itemConduitData.conduitOptions[x,y] = ConduitPortFactory.Serialize(conduit);
                                    break;
                                case ConduitType.Fluid:
                                    data.fluidConduitData.conduitOptions[x,y] = ConduitPortFactory.Serialize(conduit);
                                    break;
                                case ConduitType.Energy:
                                    data.energyConduitData.conduitOptions[x,y] = ConduitPortFactory.Serialize(conduit);
                                    break;
                                case ConduitType.Signal:
                                    data.signalConduitData.conduitOptions[x,y] = ConduitPortFactory.Serialize(conduit);
                                    break;
                                case ConduitType.Matrix:
                                    if (conduit is not MatrixConduit matrixConduit) {
                                        continue;
                                    }
                                    if (matrixConduit.HasTileEntity) {
                                        MatrixConduitData matrixConduitData = new MatrixConduitData(true);
                                        data.matrixConduitData.conduitOptions[x,y] = Newtonsoft.Json.JsonConvert.SerializeObject(matrixConduitData);
                                    } else {
                                        data.matrixConduitData.conduitOptions[x,y] = null;
                                    }
                                    
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
        
        public override IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            return base.unloadTiles(tileGridMaps);
        }

        protected override void iterateLoad(int x, int y, ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition)
        {
            base.iterateLoad(x, y, itemRegistry, tileGridMaps, realPosition);
            Vector2Int partitionPosition = new Vector2Int(x,y);
            WorldTileConduitData data = (WorldTileConduitData) getData();
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
            string matrixID = data.matrixConduitData.ids[x,y];
            if (matrixID != null) {
                place(
                    id: matrixID, 
                    sConduitOptions: data.matrixConduitData.conduitOptions[x,y],
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    layer: TileMapLayer.Matrix
                );
            }
        }

        private void place(string id, string sConduitOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return;
            }
            ITileMap tileGridMap = tileGridMaps[conduitItem.GetConduitType().ToTileMapType()];
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
            return conduitArray[positionInPartition.x,positionInPartition.y].GetConduitItem();
        }

        public void setConduitItem(Vector2Int position, ConduitType type, ConduitItem item)
        {
            string id = null;
            if (item != null) {
                id = item.id;
            }
            WorldTileConduitData serializedTileConduitData = (WorldTileConduitData)getData();
            switch (type) {
                case ConduitType.Item:
                    serializedTileConduitData.itemConduitData.ids[position.x,position.y] = id;
                    return;
                case ConduitType.Fluid:
                    serializedTileConduitData.fluidConduitData.ids[position.x,position.y] = id;
                    return;
                case ConduitType.Energy:
                    serializedTileConduitData.energyConduitData.ids[position.x,position.y] = id;
                    return;
                case ConduitType.Signal:
                    serializedTileConduitData.signalConduitData.ids[position.x,position.y] = id;
                    return;
                case ConduitType.Matrix:
                    serializedTileConduitData.matrixConduitData.ids[position.x,position.y] = id;
                    return;
            }
            Debug.LogError("Did not handle case for ConduitType " + type);
        }

        public void activate(ILoadedChunk loadedChunk)
        {
            this.parent = loadedChunk;
            foreach (ITileEntityInstance tileEntity in tileEntities) {
                if (tileEntity == null) {
                    continue;
                }
                tileEntity.setChunk(loadedChunk);
            }
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine)
        {
            foreach (ITileEntityInstance tileEntity in tileEntities) {
                if (tileEntity == null) {
                    continue;
                }
                if (tileEntity is not ICompactMachineInteractable compactMachineInteractable) {
                    continue;
                }
                compactMachineInteractable.syncToCompactMachine(compactMachine);
            }
        }

        public void assembleMultiBlocks()
        {
            foreach (ITileEntityInstance tileEntity in tileEntities) {
                if (tileEntity == null || tileEntity is not IMultiBlockTileEntity multiBlockTileEntity) {
                    continue;
                }
                multiBlockTileEntity.assembleMultiBlock();
            }
        }
    }
}
