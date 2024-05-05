using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.NodeNetwork {
    public interface INodeNetworkUI {
        public void selectNode(INodeUI nodeUI);
        public NodeNetworkUIMode getMode();
        public void setMode(NodeNetworkUIMode mode);
        public void modifyConnection(INode node);
        public void display();
        public void displayLines();
        public void addNode(INodeUI nodeUI);
        public Transform getContentContainer();
        public void placeNewNode(Vector2 position);
        public GameObject generateNewNodeObject();
        public Transform getNodeContainer();
    }
    public enum NodeNetworkUIMode {
        View,
        EditConnection
    }
    public abstract class NodeNetworkUI<Node,Network,EditorUI> : MonoBehaviour, INodeNetworkUI 
        where Node : INode where EditorUI : INodeNetworkEditController where Network : INodeNetwork<Node>
    {
        [SerializeField] protected Transform nodeContainer;
        [SerializeField] protected Transform lineContainer;
        [SerializeField] protected Transform contentMaskContainer;
        private EditorUI editController;
        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => transform;}
        public Transform ContentMaskContainer {get => contentMaskContainer;}
        public Network NodeNetwork { get => nodeNetwork; set => nodeNetwork = value; }
        public NodeNetworkUIMode Mode { get => mode; set => mode = value; }
        public INodeUI CurrentSelected { get => selectedNode; set => selectedNode = value; }
        protected Network nodeNetwork;
        private NodeNetworkUIMode mode = NodeNetworkUIMode.View;
        private INodeUI selectedNode;
        public void selectNode(INodeUI nodeUI) {
            if (CurrentSelected != null) {
                CurrentSelected.setSelect(false);
            }
            CurrentSelected = nodeUI;
            CurrentSelected.setSelect(true);
        }
        /// <summary>
        /// Displays the network
        /// Ensure that the network is initalized prior to calling
        /// </summary>
        public void display() {
            GlobalHelper.deleteAllChildren(nodeContainer);
            GlobalHelper.deleteAllChildren(lineContainer);
            foreach (Node node in nodeNetwork.getNodes()) {
                generateNode(node);
            }
            displayLines();
        }

        protected abstract void generateNode(Node node);

        public abstract void displayLines();

        protected abstract bool nodeDiscovered(Node node);

        public void FixedUpdate() {
            handleRightClick();
        }

        public void Update() {
            handleZoom();
        }

        protected abstract void initEditMode();
        private void handleZoom() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = ContentContainer;
                Vector3 newScale = containerTransform.localScale + Vector3.one * scrollInput * NodeNetworkConfig.ZOOMSPEED;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, NodeNetworkConfig.MINSCALE, NodeNetworkConfig.MAXSCALE),
                    Mathf.Clamp(newScale.y, NodeNetworkConfig.MINSCALE, NodeNetworkConfig.MAXSCALE),
                    Mathf.Clamp(newScale.z, NodeNetworkConfig.MINSCALE, NodeNetworkConfig.MAXSCALE)
                );
                Vector3 scaleChange = (newScale - containerTransform.localScale);
                Vector3 newOffset = scaleChange.x/newScale.x *  (containerTransform.position - mousePosition);
                containerTransform.localScale = newScale;
                containerTransform.position = containerTransform.position + newOffset;
            }
        }

        private void handleRightClick() {
            if (!Input.GetMouseButton(1)) {
                return;
            }
            int maxPosition = 2000;
            int maxSpeed = 250;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 dif = (mousePosition - ContentMaskContainer.transform.position);
            dif.x = Mathf.Clamp(dif.x,-maxSpeed,maxSpeed);
            dif.y = Mathf.Clamp(dif.y,-maxSpeed,maxSpeed);
            Vector2 newPosition = transform.position - dif * 0.05f;
            float clampedX = Mathf.Clamp(newPosition.x,-maxPosition,maxPosition);
            float clampedY = Mathf.Clamp(newPosition.y,-maxPosition,maxPosition);
            transform.position = new Vector3(clampedX,clampedY);
        }

        public NodeNetworkUIMode getMode()
        {
            return mode;
        }
        public void modifyConnection(INode clickedNode)
        {
            if (CurrentSelected == null) {
                return;
            }
            INode selectedNode = CurrentSelected.getNode();
            if (selectedNode == null) {
                return;
            }
            if (selectedNode.getId() == clickedNode.getId()) {
                return;
            }
            HashSet<int> preReq = CurrentSelected.getNode().getPrerequisites();
            if (!preReq.Contains(clickedNode.getId())) {
                preReq.Add(clickedNode.getId());
            } else {
                preReq.Remove(clickedNode.getId());
            }
            
            displayLines();
        }
        public void setMode(NodeNetworkUIMode mode)
        {
            this.mode = mode;
            switch (mode) {
                case NodeNetworkUIMode.View:
                    break;
                case NodeNetworkUIMode.EditConnection:
                    break;
            }
        }

        public abstract void addNode(INodeUI nodeUI);

        public Transform getContentContainer()
        {
            return transform;
        }
        public abstract void placeNewNode(Vector2 position);
        public abstract GameObject generateNewNodeObject();

        public Transform getNodeContainer()
        {
            return nodeContainer;
        }
    }
}

