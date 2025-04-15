using System;
using System.Collections.Generic;
using Conduit.Port.UI;
using Conduits.Ports;
using UnityEngine;

namespace Conduit.Placement.LoadOut
{
    public class IOConduitPortData
    {
        public ConduitPortData InputData;
        public ConduitPortData OutputData;
    }

    public enum LoadOutConduitType
    {
        ItemFluid = 0,
        Energy = 1,
        Signal = 2
    }
    public static class LoadOutConduitTypeExtension {
        public static ConduitType ToConduitType(this LoadOutConduitType type)
        {
            switch (type)
            {
                case LoadOutConduitType.ItemFluid: // Item and fluid share port data constructors so the return type doesn't matter here
                    return ConduitType.Item;
                case LoadOutConduitType.Energy:
                    return ConduitType.Energy;
                case LoadOutConduitType.Signal:
                    return ConduitType.Signal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
    public class ConduitLoadOutEditorUI : MonoBehaviour
    {
        public enum LoadOutType
        {
            ItemFluid,
            Energy,
            Signal
        }

        

        [SerializeField] private IOConduitPortUI mConduitPortUI;

        public void Display(List<IOConduitPortData> portData, LoadOutType loadOutType, int current)
        {
            mConduitPortUI.Display(portData[current]);
        }
    }
}
