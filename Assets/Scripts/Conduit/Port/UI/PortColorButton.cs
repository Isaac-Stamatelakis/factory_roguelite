using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Systems;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Conduits.Ports.UI {
    public class PortColorButton : MonoBehaviour, IPointerClickHandler
    {
        private ConduitPortData portData;
        private IConduit conduit;
        private Image colorImage;
        public void Initialize(ConduitPortData portData, IConduit conduit) {
            this.conduit = conduit;
            this.portData = portData;
            colorImage = GetComponent<Image>();
            colorImage.color = ConduitPortFactory.GetColorFromInt(portData.Color);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (portData == null) {
                return;
            }
            int color = portData.Color;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    color++;
                    if (color >= 15) {
                        color = 0;
                    }
                    break;
                case PointerEventData.InputButton.Right:
                    color--;
                    if (color < 0) {
                        color = 15;
                    }
                    break;
            }
            portData.Color = color;
            conduit?.GetConduitSystem().Rebuild();
            colorImage.color = ConduitPortFactory.GetColorFromInt(color);
        }
    }
}

