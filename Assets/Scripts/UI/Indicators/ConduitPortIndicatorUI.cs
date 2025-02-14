using System;
using Conduit.View;
using Conduits.Ports;
using Conduits.PortViewer;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPortIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image portImage;
        public void Display(ConduitViewOptions conduitViewOptions)
        {

            portImage.color = GetDisplayColor(conduitViewOptions);
        }

        private Color GetDisplayColor(ConduitViewOptions conduitViewOptions)
        {
            switch (conduitViewOptions.PortViewMode)
            {
                case PortViewMode.Auto:
                    return Color.cyan;
                case PortViewMode.None:
                    return Color.gray;
                case PortViewMode.Item:
                    return ConduitPortFactory.GetConduitPortColor(ConduitType.Item);
                case PortViewMode.Fluid:
                    return ConduitPortFactory.GetConduitPortColor(ConduitType.Fluid);
                case PortViewMode.Energy:
                    return ConduitPortFactory.GetConduitPortColor(ConduitType.Energy);
                case PortViewMode.Signal:
                    return ConduitPortFactory.GetConduitPortColor(ConduitType.Signal);
                case PortViewMode.Matrix:
                    return ConduitPortFactory.GetConduitPortColor(ConduitType.Matrix);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
