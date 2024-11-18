using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using TileMaps.Layer;
using Chunks.Partitions;
using TileEntityModule;
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
                if (port == null) {
                    port = ConduitPortFactory.CreateDefault(conduitType,(EntityPortType)portType,tileEntity,conduitItem);
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

        private static IConduit DeserializeMatrixConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity) {
            MatrixConduitData conduitData = JsonConvert.DeserializeObject<MatrixConduitData>(conduitOptionData);
            IMatrixConduitInteractable matrixConduitInteractable = null;
            if (tileEntity is IMatrixConduitInteractable matrixConduitInteractable1) {
                matrixConduitInteractable = matrixConduitInteractable1;
            }
            
            return new MatrixConduit(
                x : cellPosition.x,
                y : cellPosition.y,
                item: (MatrixConduitItem)conduitItem,
                state: conduitData.State % 16,
                matrixConduitInteractable: matrixConduitInteractable
            );
        }


        /// <summary>
        /// Sets the port of given conduit to default port
        /// </summary
        public static IConduit Create(ConduitItem conduitItem, EntityPortType portType, int x, int y, int state, ITileEntityInstance tileEntity) {
            ConduitType conduitType = conduitItem.GetConduitType();
            switch (conduitType) {
                case ConduitType.Item:
                    SolidItemConduitPort itemConduitPort = (SolidItemConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,tileEntity,conduitItem);
                    return new ItemConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: itemConduitPort
                    );
                case ConduitType.Fluid:
                    FluidItemConduitPort fluidConduitPort = (FluidItemConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,tileEntity,conduitItem);
                    return new FluidConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: fluidConduitPort
                    );
                case ConduitType.Energy:
                    EnergyConduitPort energyConduitPort = (EnergyConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,tileEntity,conduitItem);
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: energyConduitPort
                    );
                case ConduitType.Signal:
                    SignalConduitPort signalConduitPort = (SignalConduitPort)ConduitPortFactory.CreateDefault(conduitType,portType,tileEntity,conduitItem);
                    return new SignalConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: signalConduitPort
                    );
                case ConduitType.Matrix:
                    IMatrixConduitInteractable matrixConduitInteractable = null;
                    if (tileEntity is IMatrixConduitInteractable matrixConduitInteractable1) {
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
