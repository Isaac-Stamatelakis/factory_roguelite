using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Conduits.Ports.UI {
    public class PortColorButton : MonoBehaviour, IPointerClickHandler
    {
        private IColorPort colorPort;
        private IConduit conduit;
        private Image colorImage;
        public void initalize(IColorPort colorInputPort, IConduit conduit) {
            this.colorPort = colorInputPort;
            this.conduit = conduit;
            colorImage = GetComponent<Image>();
            colorImage.color = ConduitPortUIFactory.getColorFromInt(colorPort.getColor());
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (colorPort == null) {
                Debug.LogError(name + " colorInputPort is null");
                return;
            }
            int color = colorPort.getColor();
            if (eventData.button == PointerEventData.InputButton.Left) {
                color++;
                if (color >= 15) {
                    color = 0;
                }
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                color--;
                if (color < 0) {
                    color = 15;
                }
            }
            colorPort.setColor(color);
            conduit.GetConduitSystem().Rebuild();
            colorImage.color = ConduitPortUIFactory.getColorFromInt(color);
        }
    }
}

