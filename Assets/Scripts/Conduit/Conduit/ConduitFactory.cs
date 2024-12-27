using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using TileMaps.Layer;
using Chunks.Partitions;
using TileEntity;
using Items;
using Chunks.Systems;
using Newtonsoft.Json;

namespace Conduits {
    public static class ConduitFactory {
        private static readonly HashSet<ConduitType> portConduitTypes = new HashSet<ConduitType>{ConduitType.Item,ConduitType.Energy,ConduitType.Fluid,ConduitType.Signal};
        public static IConduit DeserializeConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity, EntityPortType? portType) {
            ConduitType type = conduitItem.GetConduitType();
            cellPosition -= referencePosition;
            bool isPortConduit = portConduitTypes.Contains(type);
            if (isPortConduit) {
                return DeserializePortConduit(cellPosition,referencePosition,conduitItem,conduitOptionData,tileEntity,portType);
            }
            bool isMatrixConduit = type == ConduitType.Matrix;
            if (isMatrixConduit) {
                return DeserializeMatrixConduit(cellPosition,referencePosition,conduitItem,conduitOptionData,tileEntity);
            }
            Debug.LogError("Did not handle deseralizaiton case for " + type);
            return null;
        
        }
        private static IConduit DeserializePortConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity, EntityPortType? portType) {
            ConduitType conduitType = conduitItem.GetConduitType();
            if (conduitOptionData == null)
            {
                return null;
            }
            PortConduitData conduitData = JsonConvert.DeserializeObject<PortConduitData>(conduitOptionData);
            int state = conduitData.State % 16;
            IConduitPort port = ConduitPortFactory.Deserialize(conduitData.PortData,conduitType,conduitItem);
            if (tileEntity != null && portType != null) {
                if (port == null)
                {
                    IConduitInteractable interactable = GetInteractableFromTileEntity(tileEntity, conduitType);
                    port = ConduitPortFactory.CreateDefault(conduitType,(EntityPortType)portType,interactable,conduitItem);
                }
                if (port == null) {
                    return null;
                }
                port.setTileEntity(tileEntity);
                Vector2Int relativePosition = cellPosition + referencePosition - tileEntity.getCellPosition();
                port.setPosition(relativePosition);
            }
            switch (conduitType) {
                case ConduitType.Item:
                    return new ItemConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        state: state,
                        port: (SolidItemConduitPort) port
                    );
                case ConduitType.Fluid:
                    return new FluidConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        state: state,
                        conduitItem: conduitItem,
                        port: (FluidItemConduitPort) port
                    );
                case ConduitType.Energy:
                    return new EnergyConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        state: state,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Signal:
                    return new SignalConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        state: state,
                        conduitItem: conduitItem,
                        port: port
                    );    
            }
            return null;
            
        }

        public static IConduitInteractable GetInteractableFromTileEntity(ITileEntityInstance tileEntityInstance, ConduitType conduitType)
        {
            if (tileEntityInstance is IConduitTileEntityAggregator aggregator)
            {
                return aggregator.GetConduitInteractable(conduitType);
            }

            if (tileEntityInstance is IConduitInteractable conduitInteractable)
            {
                return conduitInteractable;
            }

            return null;
        }

        private static IConduit DeserializeMatrixConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity)
        {
            int state = 15;
            if (conduitOptionData != null)
            {
                MatrixConduitData conduitData = JsonConvert.DeserializeObject<MatrixConduitData>(conduitOptionData);
                state = conduitData.State % 16;
            }
            
            IMatrixConduitInteractable matrixConduitInteractable = null;
            if (tileEntity is IMatrixConduitInteractable matrixConduitInteractable1) {
                matrixConduitInteractable = matrixConduitInteractable1;
            }
            
            return new MatrixConduit(
                x : cellPosition.x,
                y : cellPosition.y,
                item: (MatrixConduitItem)conduitItem,
                state: state,
                matrixConduitInteractable: matrixConduitInteractable
            );
        }

        public static void SerializeConduit(IConduit conduit, ConduitType conduitType, WorldTileConduitData data, int x, int y)
        {
            switch (conduitType) {
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
                        return;
                    }
                    bool attached = matrixConduit.HasTileEntity;
                    MatrixConduitData matrixConduitData = new MatrixConduitData(conduit.GetState(), true);
                    data.matrixConduitData.conduitOptions[x,y] = JsonConvert.SerializeObject(matrixConduitData);
                    break;
            }
        }


        /// <summary>
        /// Sets the port of given conduit to default port
        /// </summary
        public static IConduit Create(ConduitItem conduitItem, EntityPortType portType, int x, int y, int state, IConduitInteractable interactable) {
            ConduitType conduitType = conduitItem.GetConduitType();
            switch (conduitType) {
                case ConduitType.Item:
                    SolidItemConduitPort itemConduitPort = (SolidItemConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,interactable,conduitItem);
                    return new ItemConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: itemConduitPort
                    );
                case ConduitType.Fluid:
                    FluidItemConduitPort fluidConduitPort = (FluidItemConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,interactable,conduitItem);
                    return new FluidConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: fluidConduitPort
                    );
                case ConduitType.Energy:
                    EnergyConduitPort energyConduitPort = (EnergyConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,interactable,conduitItem);
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: energyConduitPort
                    );
                case ConduitType.Signal:
                    SignalConduitPort signalConduitPort = (SignalConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,interactable,conduitItem);
                    return new SignalConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: signalConduitPort
                    );
                case ConduitType.Matrix:
                    IMatrixConduitInteractable matrixConduitInteractable = null;
                    if (interactable is IMatrixConduitInteractable matrixConduitInteractable1) {
                        matrixConduitInteractable = matrixConduitInteractable1;
                    }
                    return new MatrixConduit(
                        x: x,
                        y: y,
                        item: (MatrixConduitItem)conduitItem,
                        state: state,
                        matrixConduitInteractable
                    );
            }
            Debug.LogError("Did not handle creation for type " + conduitType);
            return null;
        }
    }
}
