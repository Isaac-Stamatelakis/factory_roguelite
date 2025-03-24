using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook;

namespace UI.NodeNetwork {
    public interface INodeNetworkUI {
        public void SelectNode(INodeUI nodeUI);
        public NodeNetworkUIMode GetMode();
        public void SetMode(NodeNetworkUIMode mode);
        public void ModifyConnection(INode node);
        public void Display();
        public void DisplayLines();
        public Transform GetContentContainer();
        public void PlaceNewNode(Vector2 position);
        public GameObject GenerateNewNodeObject();
        public Transform GetNodeContainer();
    }
    public enum NodeNetworkUIMode {
        View,
        EditConnection
    }
    public abstract class NodeNetworkUI<TNode,TNetwork> : MonoBehaviour, INodeNetworkUI 
        where TNode : INode where TNetwork : INodeNetwork<TNode>
    {
        [SerializeField] protected Transform nodeContainer;
        [SerializeField] protected Transform lineContainer;
        [SerializeField] protected Transform contentMaskContainer;
        [SerializeField] protected NodeNetworkEditorUI editController;
        
        [SerializeField] private GameObject linePrefab;
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
        private float moveCounter = 0;
        private RightClickEvent rightClickEvent;
        protected bool movementEnabled = true;
        

        private readonly Dictionary<KeyCode[], Direction> MOVE_DIRECTIONS = new Dictionary<KeyCode[], Direction>
        {
            { new KeyCode[] { KeyCode.LeftArrow, KeyCode.A }, Direction.Left },
            { new KeyCode[] { KeyCode.RightArrow, KeyCode.D }, Direction.Right },
            { new KeyCode[] { KeyCode.DownArrow, KeyCode.S }, Direction.Down },
            { new KeyCode[] { KeyCode.UpArrow, KeyCode.W }, Direction.Up }
        };
        
        public void SelectNode(INodeUI nodeUI)
        {
            if (nodeUI != null && nodeUI.Equals(CurrentSelected))
            {
                nodeUI.SetSelect(false);
                return;
            }

            if (CurrentSelected?.GetGameObject())
            {
                CurrentSelected?.SetSelect(false);
            }
            
            CurrentSelected = nodeUI;
            CurrentSelected?.SetSelect(true);
        }
        /// <summary>
        /// Displays the network
        /// Ensure that the network is initalized prior to calling
        /// </summary>
        public void Display() {
            GlobalHelper.DeleteAllChildren(nodeContainer);
            
            nodeUIDict = new Dictionary<TNode, INodeUI>();
            foreach (TNode node in nodeNetwork.GetNodes()) {
                INodeUI nodeUI = GenerateNode(node);
                nodeUIDict[node] = nodeUI;
            }
            DisplayLines();
        }

        protected abstract INodeUI GenerateNode(TNode node);
        public abstract bool ShowAllComplete();
        public abstract void OnDeleteSelectedNode();

        public void DeleteNode(TNode node)
        {
            if (ReferenceEquals(CurrentSelected?.GetNode(),node))
            {
                OnDeleteSelectedNode();
            }
            CurrentSelected = null;
            nodeNetwork.GetNodes().Remove(node);
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
            Vector3 position = node.GetPosition();
            Vector3 changeVector = change * GetDirectionVector(direction);
            

            node.SetPosition(position+changeVector);
            Vector3 nodeUIPosition = nodeUI.GetGameObject().transform.position;
            nodeUIPosition += changeVector * transform.localScale.x;
            nodeUI.GetGameObject().transform.position = nodeUIPosition;
            DisplayLines();

        }

        private Vector3 GetDirectionVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return Vector3.left;
                case Direction.Right:
                    return Vector3.right;
                case Direction.Down:
                    return Vector3.down;
                case Direction.Up:
                    return Vector3.up;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void DisplayLines()
        {
            GlobalHelper.DeleteAllChildren(LineContainer);
            foreach (TNode node in nodeNetwork.GetNodes())
            {
                List<int> preRequites = node.GetPrerequisites();
                if (preRequites == null) continue;
                foreach (int id in node.GetPrerequisites()) {
                    TNode otherNode = LookUpNode(id);
                    if (otherNode == null) continue;
                    bool complete = ShowAllComplete() || otherNode.IsCompleted();
                    QuestBookUIFactory.GenerateLine(otherNode.GetPosition(),node.GetPosition(),LineContainer,complete,linePrefab);
                }
            }
        }

        public abstract TNode LookUpNode(int id);
        
        private bool IsPressed(KeyCode[] keyCodes, bool hold = true)
        {
            foreach (var keycode in keyCodes)
            {
                if (hold)
                {
                    if (Input.GetKey(keycode)) return true;
                }
                else
                {
                    if (Input.GetKeyDown(keycode)) return true;
                }
                
            }

            return false;
        }

        public void Update() {
            HandleZoom();
            HandleRightClick();
            if (selectedNode == null)
            {
                KeyPressMoveUpdate();
                return;
            }
            
            if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)))
            {
                DeleteNode((TNode)selectedNode.GetNode());
            }
            const float delay = 0.2f;
            foreach (var (keycodes, direction) in MOVE_DIRECTIONS)
            {
                if (!IsPressed(keycodes,hold:false)) continue;
          
                moveCounter += delay;
                break;
            }
            moveCounter += Time.deltaTime;
            
            if (moveCounter < delay) return;
            
            moveCounter = 0;
            foreach (var (keycodes, direction) in MOVE_DIRECTIONS)
            {
                if (!IsPressed(keycodes)) continue;
                MoveNode((TNode)selectedNode?.GetNode(),direction);
            }
        }

        private void KeyPressMoveUpdate()
        {
            if (!movementEnabled) return;
            foreach (var (keycodes, direction) in MOVE_DIRECTIONS)
            {
                if (!IsPressed(keycodes)) continue;
                Vector3 position = transform.position;
                const int SPEED = 5;
                position -= SPEED*GetDirectionVector(direction);
                transform.position = position;
            }
        }
        
        
        private void HandleZoom()
        {
            if (!movementEnabled) return;
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = ContentContainer;
                Vector3 newScale = containerTransform.localScale + Vector3.one * (scrollInput * NodeNetworkConfig.ZOOM_SPEED);
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, NodeNetworkConfig.MIN_SCALE, NodeNetworkConfig.MAX_SCALE),
                    Mathf.Clamp(newScale.y, NodeNetworkConfig.MIN_SCALE, NodeNetworkConfig.MAX_SCALE),
                    Mathf.Clamp(newScale.z, NodeNetworkConfig.MIN_SCALE, NodeNetworkConfig.MAX_SCALE)
                );
                Vector3 scaleChange = (newScale - containerTransform.localScale);
                Vector3 newOffset = scaleChange.x/newScale.x *  (containerTransform.position - mousePosition);
                containerTransform.localScale = newScale;
                containerTransform.position = containerTransform.position + newOffset;
            }
        }

        private void HandleRightClick() {
            if (!movementEnabled) return;
            if (Input.GetMouseButtonDown(1))
            {
                rightClickEvent = new RightClickEvent(Input.mousePosition-ContentContainer.position);
            }

            if (rightClickEvent != null)
            {
                transform.position =  rightClickEvent.GetNetworkPosition(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(1))
            {
                rightClickEvent = null;
            }
        }

        public NodeNetworkUIMode GetMode()
        {
            return mode;
        }
        public void ModifyConnection(INode clickedNode)
        {
            INode selectedNodeElement = CurrentSelected?.GetNode();
            if (selectedNodeElement == null) {
                return;
            }
            if (selectedNodeElement.GetId() == clickedNode.GetId()) {
                return;
            }
            List<int> clickedPreReqs = clickedNode.GetPrerequisites();
            List<int> selectedPreReqs = CurrentSelected.GetNode().GetPrerequisites();
            
            bool clickPreReq = clickedPreReqs.Contains(selectedNodeElement.GetId());
            
            if (clickPreReq)
            {
                clickedPreReqs.Remove(selectedNodeElement.GetId());
                selectedPreReqs.Remove(clickedNode.GetId());
            } else
            {
                selectedPreReqs.Remove(clickedNode.GetId());
                clickedPreReqs.Add(selectedNodeElement.GetId());
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

        private class RightClickEvent
        {
            public Vector2 ClickPosition;

            public Vector2 GetNetworkPosition(Vector2 mousePosition)
            {
                return mousePosition - ClickPosition;
            }

            public RightClickEvent(Vector2 clickPosition)
            {
                ClickPosition = clickPosition;
            }
        }
    }
}

