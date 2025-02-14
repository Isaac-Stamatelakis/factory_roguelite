using System;
using Conduit.View;
using Conduits.Ports;
using Conduits.PortViewer;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPortIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image portImage;
        private ConduitViewOptions currentViewOptions;
        public void Display(ConduitViewOptions conduitViewOptions)
        {
            this.currentViewOptions = conduitViewOptions;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Port View Mode:  {currentViewOptions?.PortViewMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
