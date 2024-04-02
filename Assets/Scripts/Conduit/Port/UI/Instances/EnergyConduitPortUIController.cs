using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ConduitModule.Ports.UI {
    public class EnergyConduitPortUIController : ConduitPortUIController
    {
        [SerializeField] public Button plusPriorityButton;
        [SerializeField] public Button minusPriorityButton;
        [SerializeField] public TextMeshProUGUI priorityText;

        public override void initalize(IPortConduit conduit)
        {
            base.initalize(conduit);
            minusPriorityButton.onClick.AddListener(priorityMinus);
            plusPriorityButton.onClick.AddListener(priorityPlus);

            if (port.getInputPort() != null) {
                setPriorityText();
            }
        }

        
        private void priorityMinus() {
            if (port.getInputPort() == null) {
                return;
            }
            object inputPort = port.getInputPort();
            if (inputPort is not IPriorityPort itemPort) {
                Debug.LogError("ItemConduitPortUI assigned to non PriorityPort");
                return;
            }
            int priority = itemPort.getPriority();
            priority--;
            itemPort.setPriority(priority);
            conduit.getConduitSystem().rebuild();
            setPriorityText();
        }
        private void priorityPlus() {
            object inputPort = port.getInputPort();
            if (inputPort is not IPriorityPort itemPort) {
                Debug.LogError("ItemConduitPortUI assigned to non PriorityPort");
                return;
            }
            int priority = itemPort.getPriority();
            priority++;
            itemPort.setPriority(priority);
            conduit.getConduitSystem().rebuild();
            setPriorityText();
        }
        private void setPriorityText() {
            object inputPort = port.getInputPort();
            if (inputPort is not IPriorityPort itemPort) {
                Debug.LogError("ItemConduitPortUI assigned to non ItemConduitInputPort");
                return;
            }
            priorityText.text = itemPort.getPriority().ToString();
        }
    }
}

