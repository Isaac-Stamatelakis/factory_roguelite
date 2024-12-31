using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Items;

namespace Conduits.Systems {
    public static class ConduitSystemFactory {
        public static IConduitSystem Create(IConduit conduit, IConduitSystemManager manager) {
            ConduitItem conduitItem = conduit.GetConduitItem();
            ConduitType type = conduitItem.GetConduitType();
            return type switch
            {
                ConduitType.Item => new ItemConduitSystem(conduitItem.id, manager, ItemState.Solid),
                ConduitType.Fluid => new ItemConduitSystem(conduitItem.id, manager, ItemState.Fluid),
                ConduitType.Energy => new EnergyConduitSystem(conduitItem.id, manager),
                ConduitType.Signal => new SignalConduitSystem(conduitItem.id, manager),
                ConduitType.Matrix => new MatrixConduitSystem(conduitItem.id, manager),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}