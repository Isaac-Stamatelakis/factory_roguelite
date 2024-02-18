using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace ConduitModule.Ports {
    public static class ConduitPortFactory
    {
        public static IConduitPort deseralize(string data, ConduitType conduitType, TileEntity tileEntity) {
            if (data == null) {
                return null;
            }
            switch (conduitType) {
                case ConduitType.Item:
                    ItemConduitPort itemConduitPort = JsonConvert.DeserializeObject<ItemConduitPort>(data);
                    if (tileEntity is not IItemConduitInteractable itemConduitInteractable) {
                        return null;
                    }
                    if (itemConduitPort == null) {
                        return null;
                    }
                    if (itemConduitPort.inputPort != null) {
                        itemConduitPort.inputPort.TileEntity = itemConduitInteractable;
                    }
                    if (itemConduitPort.outputPort != null) {
                        itemConduitPort.outputPort.TileEntity = itemConduitInteractable;
                    }
                    return itemConduitPort;
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
        }

        public static string serialize(IConduit conduit) {
            if (conduit == null) {
                return null;
            }
            return JsonConvert.SerializeObject(conduit.getPort());
        }

        public static IConduitPort createDefault(ConduitType conduitType, EntityPortType portType, TileEntity tileEntity) {
            switch (conduitType) {
                case ConduitType.Item:
                    if (tileEntity is not IItemConduitInteractable itemConduitInteractable) {
                        return null;
                    }
                    ItemConduitInputPort inputPort = null;
                    ItemConduitOutputPort outputPort = null;
                    if (portType == EntityPortType.All || portType == EntityPortType.Input) {
                        inputPort = new ItemConduitInputPort(itemConduitInteractable);
                    }
                    if (portType == EntityPortType.All || portType == EntityPortType.Output) {
                        outputPort = new ItemConduitOutputPort(itemConduitInteractable);
                    }
                    ItemConduitPort itemConduitPort = new ItemConduitPort(inputPort,outputPort);
                    return itemConduitPort;
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
            
        }
    }

}
