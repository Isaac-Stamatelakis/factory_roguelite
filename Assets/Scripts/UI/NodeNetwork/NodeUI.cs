using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.NodeNetwork {
    public interface INodeUI {
        public void setSelect(bool val);
        public INode getNode();
    }
    public abstract class NodeUI<Node,NetworkUI> : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ILongClickable, INodeUI
    where Node : INode where NetworkUI : INodeNetworkUI
    {
        [SerializeField] protected Image image;
        [SerializeField] protected Button button;
        [SerializeField] protected Image panel;
        private LongClickHandler holdClickInstance;
        protected Node node;
        protected NetworkUI nodeNetwork;
        
        public void init(Node node, NetworkUI nodeNetwork) {
            this.node = node;
            this.nodeNetwork = nodeNetwork;
            holdClickInstance = new LongClickHandler(this);
            setImage();
            transform.position = node.getPosition();
            
        }

        protected abstract void setImage();
        public void Update() {
            if (holdClickInstance != null) {
                holdClickInstance.checkHoldStatus();
            }
            
        }

        public void setSelect(bool val) {
            panel.color = val == false ? new Color(192f/255f,192f/255f,192f/255f,1f) : Color.magenta;
        }

        public void longClick()
        {
            nodeNetwork.selectNode(this);
        }

        public void OnDestroy() {
            button.onClick.RemoveAllListeners();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                switch (nodeNetwork.getMode()) {
                    case NodeNetworkUIMode.View:
                        openContent();
                        break;
                    case NodeNetworkUIMode.EditConnection:
                        nodeNetwork.modifyConnection(node);
                        break;
                }
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (holdClickInstance == null) {
                return;
            }
            holdClickInstance.click();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (holdClickInstance == null) {
                return;
            }
            holdClickInstance.release();
        }
        protected abstract void openContent();

        public INode getNode()
        {
            return node;
        }
    }
}

