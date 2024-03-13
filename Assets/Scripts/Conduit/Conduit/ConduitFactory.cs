using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using TileMapModule.Layer;
using ChunkModule.PartitionModule;
using TileEntityModule;
using ItemModule;

namespace ConduitModule {
    public static class ConduitFactory {
        public static IConduit deseralize(Vector2Int cellPosition, Vector2Int referencePosition, string id, string conduitOptionData, ItemRegistry itemRegistry, TileEntity tileEntity) {
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                return null;
            }
            
            ConduitType conduitType = conduitItem.getType();
            IConduitPort port = ConduitPortFactory.deseralize(conduitOptionData,conduitType,tileEntity,conduitItem);
            Debug.Log(port==null);
            if (tileEntity != null && port != null) {
                Vector2Int relativePosition = cellPosition - tileEntity.getCellPosition();
                port.setPosition(relativePosition);
            }
            cellPosition -= referencePosition;
            switch (conduitType) {
                case ConduitType.Item:
                    return new ItemConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        port: (AbstractItemConduitPort<ISolidItemConduitInteractable,ItemFilter>) port
                    );
                case ConduitType.Fluid:
                    return new FluidConduit(
                        x: cellPosition.x,
                        y: cellPosition.y,
                        conduitItem: conduitItem,
                        port: port
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


        /// <summary>
        /// Sets the port of given conduit to default port
        /// </summary
        public static IConduit create(ConduitItem conduitItem, EntityPortType portType, int x, int y, TileEntity tileEntity) {
            ConduitType conduitType = conduitItem.getType();
            switch (conduitType) {
                case ConduitType.Item:
                    AbstractItemConduitPort<ISolidItemConduitInteractable,ItemFilter> itemConduitPort = (AbstractItemConduitPort<ISolidItemConduitInteractable,ItemFilter>)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
                    return new ItemConduit(
                        x: x,
                        y: y,
                        conduitItem: conduitItem,
                        port: itemConduitPort
                    );
                case ConduitType.Fluid:
                    AbstractItemConduitPort<IFluidConduitInteractable,FluidFilter> fluidConduitPort = (AbstractItemConduitPort<IFluidConduitInteractable,FluidFilter>)ConduitPortFactory.createDefault(conduitType,portType,tileEntity,conduitItem);
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
            }
            return null;
        }
    }
}
