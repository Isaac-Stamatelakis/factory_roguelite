using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.NodeNetwork {
    public interface INodeUI {
        public void SetSelect(bool val);
        public INode GetNode();
        public GameObject GetGameObject();
        public void DisplayImage();
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
        
        public virtual void Initialize(Node node, NetworkUI nodeNetwork) {
            this.node = node;
            this.nodeNetwork = nodeNetwork;
            holdClickInstance = new LongClickHandler(this);
            DisplayImage();
            transform.position = node.GetPosition();
            
        }

        public abstract void DisplayImage();
        public void Update()
        {
            holdClickInstance?.checkHoldStatus();
        }

        public void SetSelect(bool val) {
            panel.color = val == false ? new Color(192f/255f,192f/255f,192f/255f,1f) : Color.magenta;
        }

        public void longClick()
        {
            if (nodeNetwork.GetMode() != NodeNetworkUIMode.EditConnection) return;
            nodeNetwork.SelectNode(this);
        }

        public void OnDestroy() {
            button.onClick.RemoveAllListeners();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                switch (nodeNetwork.GetMode()) {
                    case NodeNetworkUIMode.View:
                        openContent();
                        break;
                    case NodeNetworkUIMode.EditConnection:
                        nodeNetwork.ModifyConnection(node);
                        break;
                }
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            holdClickInstance?.click();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            holdClickInstance?.release();
        }
        protected abstract void openContent();

        public INode GetNode()
        {
            return node;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}

