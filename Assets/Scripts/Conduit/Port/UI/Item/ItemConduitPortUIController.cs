using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ConduitModule.Ports.UI {
    public class ItemConduitPortUIController : ConduitPortUIController<ItemConduit>
    {
        [SerializeField] public Button plusPriorityButton;
        [SerializeField] public Button minusPriorityButton;
        [SerializeField] public Button insertColorButton;
        [SerializeField] public Button extractColorButton;
        [SerializeField] public Button roundRobinButton;
        [SerializeField] public TextMeshProUGUI priorityText;
        protected ItemConduit conduit;
        protected ItemConduitPort port;

        public override void initalize(ItemConduit conduit)
        {
            this.conduit = conduit;
            this.port = (ItemConduitPort) conduit.getPort();

            toggleInsertButton.onClick.AddListener(insertToggle);
            toggleExtractButton.onClick.AddListener(extractToggle);
            minusPriorityButton.onClick.AddListener(priorityMinus);
            plusPriorityButton.onClick.AddListener(priorityPlus);

            if (port.inputPort != null) {
                if (port.inputPort is IColorPort colorPort) {
                    PortColorButton insertColor = insertColorButton.GetComponent<PortColorButton>();
                    insertColor.initalize(colorPort,conduit);
                }
                
                setPriorityText();
            }

            if (port.outputPort != null) {
                if (port.outputPort is IColorPort colorPort) {
                    PortColorButton extractColor = extractColorButton.GetComponent<PortColorButton>();
                    extractColor.initalize(colorPort,conduit);
                }
            }
            
        }

        private void insertToggle() {
            if (port.inputPort == null) {
                return;
            }
            port.inputPort.Enabled=!port.inputPort.Enabled;
            conduit.getConduitSystem().rebuild();
        }
        private void extractToggle() {
            if (port.outputPort == null) {
                return;
            }
            port.outputPort.Enabled=!port.outputPort.Enabled;
            conduit.getConduitSystem().rebuild();
        }
        
        private void priorityMinus() {
            if (port.inputPort == null) {
                return;
            }
            port.inputPort.priority--;
            conduit.getConduitSystem().rebuild();
            setPriorityText();
        }
        private void priorityPlus() {
            if (port.inputPort == null) {
                return;
            }
            port.inputPort.priority++;
            conduit.getConduitSystem().rebuild();
            setPriorityText();
        }
        private void setPriorityText() {
            priorityText.text = port.inputPort.priority.ToString();
        }
    }
}

