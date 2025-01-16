using System;
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

        public void RefreshNode(TNode node)
        {
            nodeUIDict[node].DisplayImage();
        }

        public void MoveNode(TNode node, Direction direction)
        {
            INodeUI nodeUI = nodeUIDict[node];
            const int change = 64;
            Vector3 position = node.getPosition();
            Vector3 changeVector = Vector3.zero;
            switch (direction)
            {
                case Direction.Left:
                    changeVector = -new Vector3(change, 0);
                    break;
                case Direction.Right:
                    changeVector = new Vector3(change, 0);
                    break;
                case Direction.Down:
                    changeVector = -new Vector3(0, change);
                    break;
                case Direction.Up:
                    changeVector = new Vector3(0, change);
                    break;
                case Direction.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            node.SetPosition(position+changeVector);
            Vector3 nodeUIPosition = nodeUI.GetGameObject().transform.position;
            nodeUIPosition += changeVector * transform.localScale.x;
            nodeUI.GetGameObject().transform.position = nodeUIPosition;
            DisplayLines();

        }

        public abstract void DisplayLines();

        protected abstract bool nodeDiscovered(TNode node);

        public void FixedUpdate() {
            HandleRightClick();
        }

        public void Update() {
            HandleZoom();
            if (selectedNode == null) return;
            if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)))
            {
                DeleteNode((TNode)selectedNode.GetNode());
            }

            
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveNode((TNode)selectedNode?.GetNode(),Direction.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveNode((TNode)selectedNode?.GetNode(),Direction.Right);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveNode((TNode)selectedNode?.GetNode(),Direction.Up);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveNode((TNode)selectedNode?.GetNode(),Direction.Down);
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
            INode selectedNodeElement = CurrentSelected.GetNode();
            if (selectedNodeElement == null) {
                return;
            }
            if (selectedNodeElement.getId() == clickedNode.getId()) {
                return;
            }

            HashSet<int> clickedPreReqs = clickedNode.getPrerequisites();
            HashSet<int> selectedPreReqs = CurrentSelected.GetNode().getPrerequisites();
            
            bool connected = clickedPreReqs.Contains(selectedNodeElement.getId()) || selectedPreReqs.Contains(clickedNode.getId());
            if (connected)
            {
                clickedPreReqs.Remove(selectedNodeElement.getId());
                selectedPreReqs.Remove(clickedNode.getId());
            }
            else
            {
                selectedPreReqs.Add(clickedNode.getId());
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

