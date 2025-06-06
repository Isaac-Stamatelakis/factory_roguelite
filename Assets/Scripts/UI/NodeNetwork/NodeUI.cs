using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.NodeNetwork {
    public interface INodeUI {
        public void SetSelect(bool val);
        public INode GetNode();
        public GameObject GetGameObject();
        public void DisplayImage();
        public void OpenContent(NodeUIContentOpenMode contentOpenMode);
    }
    public abstract class NodeUI<TNode, TNetworkUI> : MonoBehaviour, IPointerClickHandler, INodeUI
    where TNode : INode where TNetworkUI : INodeNetworkUI
    {
        [SerializeField] protected ItemSlotUI mItemSlotUI;
        [SerializeField] protected Button button;
        [SerializeField] protected Image panel;
        protected TNode node;
        protected TNetworkUI nodeNetwork;
        private bool instantiated = true;
        
        public virtual void Initialize(TNode node, TNetworkUI nodeNetwork) {
            this.node = node;
            this.nodeNetwork = nodeNetwork;
            DisplayImage();
            transform.position = node.GetPosition();
            
        }

        public abstract void DisplayImage();

        public void SetSelect(bool val) {
            panel.color = val == false ? new Color(192f/255f,192f/255f,192f/255f,1f) : Color.magenta;
        }
        
        public void OnDestroy()
        {
            instantiated = false;
            button.onClick.RemoveAllListeners();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (nodeNetwork == null) return;
            if (!nodeNetwork.IsSelectingNodes())
            {
                OpenContent(NodeUIContentOpenMode.Click);
            }
            else
            {
                nodeNetwork.ModifyConnection(node);
            }
        }
        
        public abstract void OpenContent(NodeUIContentOpenMode contentOpenMode);

        public INode GetNode()
        {
            return node;
        }

        public GameObject GetGameObject()
        {
            if (!instantiated) return null;
            return gameObject;
        }

        
    }
    public enum NodeUIContentOpenMode
    {
        Click,
        KeyPress
    }
}

