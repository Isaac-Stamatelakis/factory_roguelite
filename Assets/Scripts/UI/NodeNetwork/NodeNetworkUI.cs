using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.NodeNetwork {
    public interface INodeNetworkUI {
        public void SelectNode(INodeUI nodeUI);
        public NodeNetworkUIMode GetMode();
        public void SetMode(NodeNetworkUIMode mode);
        public void ModifyConnection(INode node);
        public void Display();
        public void DisplayLines();
        public void AddNode(INodeUI nodeUI);
        public Transform GetContentContainer();
        public void PlaceNewNode(Vector2 position);
        public GameObject GenerateNewNodeObject();
        public Transform GetNodeContainer();
    }
    public enum NodeNetworkUIMode {
        View,
        EditConnection
    }
    public abstract class NodeNetworkUI<TNode,TNetwork,TEditorUI> : MonoBehaviour, INodeNetworkUI 
        where TNode : INode where TEditorUI : INodeNetworkEditController where TNetwork : INodeNetwork<TNode>
    {
        [SerializeField] protected Transform nodeContainer;
        [SerializeField] protected Transform lineContainer;
        [SerializeField] protected Transform contentMaskContainer;
        private TEditorUI editController;
        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => transform;}
        public Transform ContentMaskContainer {get => contentMaskContainer;}
        public TNetwork NodeNetwork { get => nodeNetwork; set => nodeNetwork = value; }
        public NodeNetworkUIMode Mode { get => mode; set => mode = value; }
        public INodeUI CurrentSelected { get => selectedNode; set => selectedNode = value; }
        protected TNetwork nodeNetwork;
        private NodeNetworkUIMode mode = NodeNetworkUIMode.View;
        private INodeUI selectedNode;
        private Dictionary<TNode, INodeUI> nodeUIDict = new Dictionary<TNode, INodeUI>();
        public void SelectNode(INodeUI nodeUI) {
            if (CurrentSelected != null) {
                CurrentSelected.SetSelect(false);
            }
            CurrentSelected = nodeUI;
            CurrentSelected.SetSelect(true);
        }
        /// <summary>
        /// Displays the network
        /// Ensure that the network is initalized prior to calling
        /// </summary>
        public void Display() {
            GlobalHelper.deleteAllChildren(nodeContainer);
            GlobalHelper.deleteAllChildren(lineContainer);
            foreach (TNode node in nodeNetwork.getNodes()) {
                INodeUI nodeUI = GenerateNode(node);
                nodeUIDict[node] = nodeUI;
            }
            DisplayLines();
        }

        protected abstract INodeUI GenerateNode(TNode node);

        public void DeleteNode(TNode node)
        {
            nodeNetwork.getNodes().Remove(node);
            INodeUI nodeUI = nodeUIDict[node];
            GameObject.Destroy(nodeUI.GetGameObject());
            DisplayLines();
        }

        public abstract void DisplayLines();

        protected abstract bool nodeDiscovered(TNode node);

        public void FixedUpdate() {
            HandleRightClick();
        }

        public void Update() {
            HandleZoom();
            if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) && selectedNode != null)
            {
                DeleteNode((TNode)selectedNode.GetNode());
            }
        }

        protected abstract void initEditMode();
        private void HandleZoom() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = ContentContainer;
                Vector3 newScale = containerTransform.localScale + Vector3.one * (scrollInput * NodeNetworkConfig.ZOOMSPEED);
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

        private void HandleRightClick() {
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

        public NodeNetworkUIMode GetMode()
        {
            return mode;
        }
        public void ModifyConnection(INode clickedNode)
        {
            if (CurrentSelected == null) {
                return;
            }
            INode selectedNode = CurrentSelected.GetNode();
            if (selectedNode == null) {
                return;
            }
            if (selectedNode.getId() == clickedNode.getId()) {
                return;
            }
            HashSet<int> preReq = CurrentSelected.GetNode().getPrerequisites();
            if (!preReq.Contains(clickedNode.getId())) {
                preReq.Add(clickedNode.getId());
            } else {
                preReq.Remove(clickedNode.getId());
            }
            
            DisplayLines();
        }
        public void SetMode(NodeNetworkUIMode mode)
        {
            this.mode = mode;
            switch (mode) {
                case NodeNetworkUIMode.View:
                    break;
                case NodeNetworkUIMode.EditConnection:
                    break;
            }
        }

        public abstract void AddNode(INodeUI nodeUI);

        public Transform GetContentContainer()
        {
            return transform;
        }
        public abstract void PlaceNewNode(Vector2 position);
        public abstract GameObject GenerateNewNodeObject();

        public Transform GetNodeContainer()
        {
            return nodeContainer;
        }
    }
}

