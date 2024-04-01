using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using TileMapModule.Layer;
using ChunkModule.PartitionModule;
using TileEntityModule;
using ItemModule;
using ChunkModule.ClosedChunkSystemModule;

namespace ConduitModule {
    public static class ConduitFactory {

        public static IConduit deseralizeConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, TileEntity tileEntity, EntityPortType? portType) {
            ConduitType type = conduitItem.getType();
            cellPosition -= referencePosition;
            bool isPortConduit = type == ConduitType.Item || type == ConduitType.Fluid || type == ConduitType.Energy || type == ConduitType.Signal;
            if (isPortConduit) {
                return deseralizePortConduit(cellPosition,referencePosition,conduitItem,conduitOptionData,tileEntity,portType);
            }
            bool isMatrixConduit = type == ConduitType.Matrix;
            if (isMatrixConduit) {
                return deseralizeMatrixConduit(cellPosition,referencePosition,conduitItem,conduitOptionData,tileEntity);
            }
            Debug.LogError("Did not handle deseralizaiton case for " + type);
            return null;
        
        }
        private static IConduit deseralizePortConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, TileEntity tileEntity, EntityPortType? portType) {
            ConduitType conduitType = conduitItem.getType();
            IConduitPort port = ConduitPortFactory.deseralize(conduitOptionData,conduitType,conduitItem);
            if (tileEntity != null && portType != null) {
                if (port == null) {
                    port = ConduitPortFactory.createDefault(conduitType,(EntityPortType)portType,tileEntity,conduitItem);
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
                        port: (SolidItemConduitPort) port
                    );
                case ConduitType.Fluid:
                    return new FluidConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        port: (FluidItemConduitPort) port
                    );
                case ConduitType.Energy:
                    return new EnergyConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );
                case ConduitType.Signal:
                    return new SignalConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        port: port
                    );    
            }
            return null;
            
        }

        private static IConduit deseralizeMatrixConduit(Vector2Int cellPosition, Vector2Int referencePosition, ConduitItem conduitItem, string conduitOptionData, TileEntity tileEntity) {
            IMatrixConduitInteractable matrixConduitInteractable = null;
            if (tileEntity is IMatrixConduitInteractable matrixConduitInteractable1) {
                matrixConduitInteractable = matrixConduitInteractable1;
                Debug.Log(tileEntity.name);
            }
            
            return new MatrixConduit(
                x : cellPosition.x,
                y : cellPosition.y,
                item: (MatrixConduitItem)conduitItem,
                matrixConduitInteractable: matrixConduitInteractable
            );
        }


        /// <summary>
        /// Sets the port of given conduit to default port
        /// </summary
        public static IConduit create(ConduitItem conduitItem, EntityPortType portType, int x, int y, TileEntity tileEntity) {
            ConduitType conduitType = conduitItem.getType();
            switch (conduitType) {
                case ConduitType.Item:
                    SolidItemConduitPort itemConduitPort = (SolidItemConduitPort)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
                    return new ItemConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: itemConduitPort
                    );
                case ConduitType.Fluid:
                    FluidItemConduitPort fluidConduitPort = (FluidItemConduitPort)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
                    return new FluidConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: fluidConduitPort
                    );
                case ConduitType.Energy:
                    EnergyConduitPort energyConduitPort = (EnergyConduitPort)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: energyConduitPort
                    );
                case ConduitType.Signal:
                    SignalConduitPort signalConduitPort = (SignalConduitPort)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
                    return new SignalConduit(
                        x: x,
                        y: y,
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
                        matrixConduitInteractable
                    );
            }
            Debug.LogError("Did not handle creation for type " + conduitType);
            return null;
        }
    }
}
