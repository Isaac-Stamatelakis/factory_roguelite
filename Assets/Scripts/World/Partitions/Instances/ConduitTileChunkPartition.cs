using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Type;
using Conduits;
using Chunks.IO;
using Conduits.Ports;
using Items;
using TileEntity.Instances.CompactMachines;
using Chunks.Systems;
using Tiles;

namespace Chunks.Partitions {
    public interface IConduitTileChunkPartition {
        public void GetConduits(ConduitType conduitType,Dictionary<Vector2Int,IConduit> conduitDict, Vector2Int referenceChunk,Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts);
        public bool GetConduitLoaded();
        public void SetConduitLoaded(bool val);
        public void SoftLoadTileEntities();
        public Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetEntityPorts(ConduitType conduitType,Vector2Int referenceChunk);
        public void SetConduits(Dictionary<ConduitType, IConduit[,]> conduits);
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
        public ConduitChunkPartition(WorldTileConduitData data, Vector2Int position, IChunk parent) : base(data, position, parent)
        {
        }

        public void GetConduits(ConduitType conduitType, Dictionary<Vector2Int,IConduit> conduitDict, Vector2Int referenceChunk, Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts)
        {
            WorldTileConduitData serializedTileConduitData = (WorldTileConduitData) data;
            switch (conduitType) {
                case ConduitType.Item:
                    GetConduitsFromData(serializedTileConduitData.itemConduitData,conduitDict,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Fluid:
                    GetConduitsFromData(serializedTileConduitData.fluidConduitData,conduitDict,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Energy:
                    GetConduitsFromData(serializedTileConduitData.energyConduitData,conduitDict,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Signal:
                    GetConduitsFromData(serializedTileConduitData.signalConduitData,conduitDict,referenceChunk,tileEntityPorts);
                    return;
                case ConduitType.Matrix:
                    GetConduitsFromData(serializedTileConduitData.matrixConduitData,conduitDict,referenceChunk,tileEntityPorts);
                    return;
            }
            Debug.LogError("ConduitTileChunkPartition method 'getConduits' did not handle case for type '" + conduitType.ToString() + "'");
        }
        /// <summary>
        /// Loads Tile Entities which are tickable. Note these are always active even if the partition is not in sight of the player
        /// </summary>
        public void SoftLoadTileEntities() {
            if (tickLoaded) {
                Debug.LogError("Attempted to tick load partition which is already ticked loaded");
                return;
            }
            tileEntities ??= new ITileEntityInstance[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
            tickableTileEntities = new List<ITickableTileEntity>();
            loadTickableTileEntityLayer(data.baseData);
            tickLoaded = true;
        }

        public Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetEntityPorts(ConduitType type, Vector2Int referenceFrame) {
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> ports = new Dictionary<ITileEntityInstance, List<TileEntityPortData>>();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    ITileEntityInstance tileEntity = tileEntities[x,y];
                    if (tileEntity is not IConduitPortTileEntity conduitInteractable) {
                        continue;
                    }
                    var entityPorts = ConduitPortFactory.GetEntityPorts(conduitInteractable, type);
                    if (entityPorts == null) {
                        continue;
                    }
                    ports[tileEntity] = entityPorts;
                }
            }
            return ports;
        }
        private void loadTickableTileEntityLayer(SerializedBaseTileData data) {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = data.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.GetTileItem(id);
                    if (ReferenceEquals(tileItem?.tileEntity,null)) {
                        continue;
                    }
                    tileEntities[x,y] = PlaceSoftLoadableTileEntity(tileItem,data.sTileEntityOptions[x,y],new Vector2Int(x,y));
                    if (tileEntities[x, y] is ITickableTileEntity tickableTileEntity)
                    {
                        tickableTileEntities.Add(tickableTileEntity);
                    }
                }
            }
        }
        protected ITileEntityInstance PlaceSoftLoadableTileEntity(TileItem tileItem, string options, Vector2Int positionInPartition)
        {
            /* This check is inefficient but this is only ever called at the start of the game so its fine
             * The other alternative is assigning soft load to the tile entity directly but this leaves lots of room for human error
             */
            ITileEntityInstance instance = tileItem.tileEntity.CreateInstance(Vector2Int.zero, tileItem, parent);
            if (instance is not ISoftLoadableTileEntity) {
                return null;
            }
            Vector2Int cellPosition = this.position * Global.ChunkPartitionSize + positionInPartition;
            return TileEntityUtils.placeTileEntity(tileItem,cellPosition,parent,false,unserialize:true, data:options);
        }

        private void GetConduitsFromData(SeralizedChunkConduitData data,Dictionary<Vector2Int,IConduit> conduitDict,Vector2Int referenceChunk, Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPorts) {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            Vector2Int partitionOffset = GetRealPosition()*Global.ChunkPartitionSize;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    string id = data.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
                    if (ReferenceEquals(conduitItem,null)) {
                        continue;
                    }
                    Vector2Int cellPosition = new Vector2Int(x,y)+partitionOffset;
                    ITileEntityInstance tileEntity = null;
                    EntityPortType? port = null;
                    foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPortData>> kvp in tileEntityPorts) {
                        if (tileEntity != null) {
                            break;
                        }
                        foreach (TileEntityPortData tileEntityPort in kvp.Value) {
                            if (kvp.Key.getCellPosition() + tileEntityPort.position == cellPosition) {
                                tileEntity = kvp.Key;
                                port = tileEntityPort.portType;
                                break;
                            }
                        }
                    }
                    IConduit conduit = ConduitFactory.DeserializeConduit(
                        cellPosition: cellPosition,
                        conduitItem: conduitItem,
                        conduitOptionData: data.conduitOptions[x,y],
                        tileEntity : tileEntity,
                        portType: port
                    );
                    conduitDict[cellPosition] = conduit;
                }
            }
        }

        public override void Save()
        {
            base.Save();
            WorldTileConduitData data = (WorldTileConduitData) GetData();

            if (conduits == null) return;
            foreach (KeyValuePair<ConduitType, IConduit[,]> kvp in conduits) {
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        IConduit conduit = kvp.Value[x,y];
                        ConduitFactory.SerializeConduit(conduit, kvp.Key,data,x,y);
                    }
                }
            }
        }
        
        protected override void iterateLoad(int x, int y, ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Vector2Int realPosition)
        {
            base.iterateLoad(x, y, itemRegistry, tileGridMaps, realPosition);
            Vector2Int partitionPosition = new Vector2Int(x,y);
            WorldTileConduitData data = (WorldTileConduitData) GetData();
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

        private void place(string id, string sConduitOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (ReferenceEquals(conduitItem, null)) return;
            IWorldTileMap iWorldTileGridMap = tileGridMaps[conduitItem.GetConduitType().ToTileMapType()];
            iWorldTileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                conduitItem
            );
        }

        public bool GetConduitLoaded()
        {
            return tickLoaded;
        }

        public void SetConduitLoaded(bool val)
        {
            tickLoaded = val;
        }

       

        public void SetConduits(Dictionary<ConduitType, IConduit[,]> conduits)
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
            WorldTileConduitData serializedTileConduitData = (WorldTileConduitData)GetData();
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
                multiBlockTileEntity.AssembleMultiBlock();
            }
        }
    }
}
