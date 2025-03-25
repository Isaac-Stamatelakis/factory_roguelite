using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using TileMaps.Layer;
using Chunks.Partitions;
using TileEntity;
using Items;
using Chunks.Systems;
using Conduits.Systems;
using Newtonsoft.Json;
using UnityEditor.Hardware;

namespace Conduits {
    public static class ConduitFactory {
        public static IConduit DeserializeConduit(Vector2Int cellPosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity, EntityPortType? portType) {
            ConduitType type = conduitItem.GetConduitType();
            return type switch
            {
                ConduitType.Item or ConduitType.Fluid or ConduitType.Energy or ConduitType.Signal =>
                    DeserializePortConduit(cellPosition, conduitItem, conduitOptionData, tileEntity, portType),
                ConduitType.Matrix => DeserializeMatrixConduit(cellPosition, conduitItem, conduitOptionData, tileEntity),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private static IConduit DeserializePortConduit(Vector2Int cellPosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity, EntityPortType? portType) {
            ConduitType conduitType = conduitItem.GetConduitType();
            if (conduitOptionData == null)
            {
                return null;
            }
            PortConduitData conduitData = JsonConvert.DeserializeObject<PortConduitData>(conduitOptionData);
            int state = conduitData.State % 16;
            IConduitPort port = ConduitPortFactory.Deserialize(conduitData.PortData,conduitType,conduitItem, tileEntity,cellPosition);
            if (port == null && portType.HasValue)
            {
                port = ConduitPortFactory.CreateDefault(conduitType, portType.Value, tileEntity, conduitItem,cellPosition);
            }
            return CreatePortConduit(conduitItem, cellPosition.x, cellPosition.y, state, tileEntity, port);
            
        }

        public static IConduitInteractable GetInteractableFromTileEntity(ITileEntityInstance tileEntityInstance, ConduitType conduitType)
        {
            if (conduitType == ConduitType.Item)
            {
                Debug.Log($"{tileEntityInstance.GetName()} {tileEntityInstance is ISolidItemPortTileEntityAggregator}");
            }
            return tileEntityInstance switch
            {
                null => null,
                IConduitPortTileEntityAggregator aggregator => aggregator.GetConduitInteractable(conduitType),
                ISolidItemPortTileEntityAggregator solidItemAggregator when conduitType == ConduitType.Item => solidItemAggregator.GetSolidItemConduitInteractable(),
                IFluidItemPortTileEntityAggregator fluidItemAggregator when conduitType == ConduitType.Fluid => fluidItemAggregator.GetFluidItemConduitInteractable(),
                IEnergyPortTileEntityAggregator energyAggregator when conduitType == ConduitType.Energy => energyAggregator.GetEnergyConduitInteractable(),
                ISignalPortTileEntityAggregator signalAggregator when conduitType == ConduitType.Signal => signalAggregator.GetSignalConduitInteractable(),
                IMatrixPortTileEntityAggregator matrixAggregator when conduitType == ConduitType.Matrix => matrixAggregator.GetMatrixConduitInteractable(),
                IConduitInteractable conduitInteractable => conduitInteractable,
                _ => null
            };
        }

        private static IConduit DeserializeMatrixConduit(Vector2Int cellPosition, ConduitItem conduitItem, string conduitOptionData, ITileEntityInstance tileEntity)
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
            if (conduit == null) return;
            switch (conduitType) {
                case ConduitType.Item:
                    data.itemConduitData.conduitOptions[x,y] = ConduitPortFactory.SerializePortConduit(conduit, conduitType);
                    break;
                case ConduitType.Fluid:
                    data.fluidConduitData.conduitOptions[x,y] = ConduitPortFactory.SerializePortConduit(conduit, conduitType);
                    break;
                case ConduitType.Energy:
                    data.energyConduitData.conduitOptions[x,y] = ConduitPortFactory.SerializePortConduit(conduit, conduitType);
                    break;
                case ConduitType.Signal:
                    data.signalConduitData.conduitOptions[x,y] = ConduitPortFactory.SerializePortConduit(conduit, conduitType);
                    break;
                case ConduitType.Matrix:
                    MatrixConduitData matrixConduitData = new MatrixConduitData(conduit.GetState(), true);
                    data.matrixConduitData.conduitOptions[x,y] = JsonConvert.SerializeObject(matrixConduitData);
                    break;
            }
        }


        /// <summary>
        /// Sets the portData of given conduit to default portData
        /// </summary
        public static IConduit CreateNew(ConduitItem conduitItem, EntityPortType portType, int x, int y, int state, ITileEntityInstance tileEntity) {
            ConduitType conduitType = conduitItem.GetConduitType();
            switch (conduitType)
            {
                case ConduitType.Item:
                case ConduitType.Fluid:
                case ConduitType.Energy:
                case ConduitType.Signal:
                    IConduitPort port = ConduitPortFactory.CreateDefault(conduitType, portType, tileEntity, conduitItem, new Vector2Int(x,y));
                    return CreatePortConduit(conduitItem, x, y, state, tileEntity, port);
                case ConduitType.Matrix:
                    return CreateMatrix(conduitItem, portType, x, y, state, tileEntity);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IConduit CreateMatrix(ConduitItem conduitItem, EntityPortType portType, int x, int y, int state, ITileEntityInstance tileEntityInstance, IConduitPort port = null)
        {
            IConduitInteractable interactable = GetInteractableFromTileEntity(tileEntityInstance, ConduitType.Matrix);
            IMatrixConduitInteractable matrixConduitInteractable = interactable as IMatrixConduitInteractable;
            return new MatrixConduit(
                x: x,
                y: y,
                item: (MatrixConduitItem)conduitItem,
                state: state,
                matrixConduitInteractable
            );
        }

        private static IConduit CreatePortConduit(ConduitItem conduitItem, int x, int y, int state, ITileEntityInstance tileEntityInstance, IConduitPort port)
        {
            ConduitType conduitType = conduitItem.GetConduitType();
            
            switch (conduitType)
            {
                case ConduitType.Item:
                    ItemTileEntityPort itemTileEntityPort = port as ItemTileEntityPort;
                    return new ItemConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: itemTileEntityPort
                    );
                case ConduitType.Fluid:
                    ItemTileEntityPort fluidItemEntityPort = port as ItemTileEntityPort;
                    return new FluidConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: fluidItemEntityPort
                    );
                case ConduitType.Energy:
                    EnergyTileEntityPort energyTileEntityPort = port as EnergyTileEntityPort;
                    return new EnergyConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: energyTileEntityPort
                    );
                case ConduitType.Signal:
                    SignalTileEntityPort signalTileEntityPort = port as SignalTileEntityPort;
                    return new SignalConduit(
                        x: x,
                        y: y,
                        state: state,
                        conduitItem: conduitItem,
                        port: signalTileEntityPort
                    );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
