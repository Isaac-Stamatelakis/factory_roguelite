using System;
using Conduit.View;
using Conduits.Ports;
using Conduits.PortViewer;
using Player;
using PlayerModule.KeyPress;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitPortIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image portImage;
        
        private PlayerScript playerScript;
        public void Display(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            Refresh();
        }

        public void Refresh()
        {
            portImage.color = GetDisplayColor(playerScript.ConduitViewOptions);
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
            ToolTipController.Instance.ShowToolTip(transform.position, $"Port View Mode:  {playerScript.ConduitViewOptions?.PortViewMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            playerScript.GetComponent<PlayerKeyPress>().ChangePortModePress();
        }
    }
}
