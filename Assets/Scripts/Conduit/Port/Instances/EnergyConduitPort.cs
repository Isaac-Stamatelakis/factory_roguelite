using System.Collections;
using System.Collections.Generic;
using Conduits.Systems;
using Item.Slot;
using Items;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;

namespace Conduits.Ports {
    public class EnergyTileEntityPort : TileEntityConduitPort<IEnergyConduitInteractable, PriorityConduitPortData, ConduitPortData, ResourceConduitItem>
    {
        public EnergyTileEntityPort(IEnergyConduitInteractable interactable, Vector2Int position, PriorityConduitPortData inputPort, ConduitPortData outputPort, ResourceConduitItem resourceConduitItem) 
            : base(interactable, position, inputPort, outputPort, resourceConduitItem)
        {
            
        }

        public uint GetExtractionRate()
        {
            return ConduitItem.maxSpeed;
        }
    }
}
