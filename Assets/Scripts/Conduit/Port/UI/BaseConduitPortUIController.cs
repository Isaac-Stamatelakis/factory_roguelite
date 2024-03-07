using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using UnityEngine.UI;

namespace ConduitModule.Ports.UI {
    public interface IConduitPortUIController {
        public void initalize(IConduit conduit);
    }
    public class BaseConduitPortUIController : ConduitPortUIController, IConduitPortUIController 
    {
        [SerializeField] public Button toggleInsertButton;
        [SerializeField] public Button toggleExtractButton;
        [SerializeField] public Button toggleColorInsertButton;
        [SerializeField] public Button toggleColorExtractButton;
        [SerializeField] public Image backgroundPanel;
        [SerializeField] public GameObject insertDisable;
        [SerializeField] public GameObject extractDisable;
        

        public override void initalize(IConduit conduit)
        {
            base.initalize(conduit);
            toggleInsertButton.onClick.AddListener(insertToggle);
            toggleExtractButton.onClick.AddListener(extractToggle);
            if (port.getInputPort() != null) {
                if (port.getInputPort() is IColorPort colorPort) {
                    PortColorButton insertColor = toggleColorInsertButton.GetComponent<PortColorButton>();
                    insertColor.initalize(colorPort,conduit);
                }
            }

            if (port.GetOutputPort() != null) {
                if (port.GetOutputPort() is IColorPort colorPort) {
                    PortColorButton extractColor = toggleColorExtractButton.GetComponent<PortColorButton>();
                    extractColor.initalize(colorPort,conduit);
                }
            }
        }

        public void setBackground(Sprite sprite) {
            this.backgroundPanel.sprite = sprite;
        }

        public void toggleInsertCover() {
            insertDisable.SetActive(!insertDisable.activeSelf);
        }
        public void toggleExtractCover() {
            extractDisable.SetActive(!extractDisable.activeSelf);
        }

        private void insertToggle() {
            if (port.getInputPort() == null) {
                return;
            }
            ITogglablePort inputPort = (ITogglablePort)port.getInputPort();
            inputPort.setEnabled(!inputPort.isEnabled());
            conduit.getConduitSystem().rebuild();
        }
        private void extractToggle() {
            if (port.GetOutputPort() == null) {
                return;
            }
            ITogglablePort outputPort = (ITogglablePort)port.GetOutputPort();
            outputPort.setEnabled(!outputPort.isEnabled());
            conduit.getConduitSystem().rebuild();
        }
    }

}
