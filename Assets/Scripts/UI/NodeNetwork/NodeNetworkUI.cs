using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DevTools.CraftingTrees.Network;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook;

namespace UI.NodeNetwork {
    public interface INodeNetworkUI {
        public void SelectNode(INodeUI nodeUI);
        public void SelectNodeValue(INode node);
        public void ModifyConnection(INode node);
        public void Display();
        public void DisplayLines();
        public Transform GetContentContainer();
        public INode PlaceNewNode(Vector2 position);
        public GameObject GenerateNewNodeObject();
        public Transform GetNodeContainer();
        public bool IsSelectingNodes();
        public void DeleteNode(INode node);
        public int GetNodeOutputs(INode node);
        public void MultiSelectNodes(Vector2 first, Vector2 second);
    }

    public abstract class NodeConnectionFilterer
    {
        protected NodeConnectionFilterer(INodeNetworkUI nodeNetworkUI)
        {
            this.nodeNetworkUI = nodeNetworkUI;
        }

        protected INodeNetworkUI nodeNetworkUI;
        public abstract bool CanConnect(INode first, INode second);
    }
    public abstract class NodeNetworkUI<TNode,TNetwork> : MonoBehaviour, INodeNetworkUI 
        where TNode : INode where TNetwork : INodeNetwork<TNode>
    {
        [SerializeField] protected Transform nodeContainer;
        [SerializeField] protected Transform lineContainer;
        [SerializeField] protected Transform contentMaskContainer;
        [SerializeField] protected NodeNetworkEditorUI editController;
        [SerializeField] private Color color = Color.yellow;
        [SerializeField] private GameObject linePrefab;
        
        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => transform;}
        public Transform ContentMaskContainer {get => contentMaskContainer;}
        public TNetwork NodeNetwork { get => nodeNetwork; set => nodeNetwork = value; }
        protected TNetwork nodeNetwork;
        protected List<INodeUI> selectedNodes = new();
        protected Dictionary<TNode, INodeUI> nodeUIDict = new Dictionary<TNode, INodeUI>();
        private float moveCounter = 0;
        private RightClickEvent rightClickEvent;
        protected bool lockVerticalMovement = false;
        protected bool lockHorizontalMovement = false;
        protected bool lockZoom = false;
        protected Bounds? viewBounds;
        private Camera canvasCamera;
        protected NodeConnectionFilterer connectionFilterer;
        public void Start()
        {
            canvasCamera = GetComponentInParent<Canvas>().worldCamera;
        }

        List<(KeyCode[], Direction)> moveDirections = new List<(KeyCode[], Direction)>
        {
            (new KeyCode[] { KeyCode.LeftArrow, KeyCode.A }, Direction.Left),
            (new KeyCode[] { KeyCode.RightArrow, KeyCode.D }, Direction.Right),
            (new KeyCode[] { KeyCode.UpArrow, KeyCode.W }, Direction.Up),
            (new KeyCode[] { KeyCode.DownArrow, KeyCode.S }, Direction.Down)
        };

        public void SelectNodeValue(INode node)
        {
            if (node == null) return;
            INodeUI nodeUI = nodeUIDict.GetValueOrDefault((TNode)node);
            SelectNode(nodeUI);
        }
        public void SelectNode(INodeUI nodeUI)
        {
            foreach (var selected in selectedNodes)
            {
                if (selected.GetGameObject())
                {
                    selected.SetSelect(false);
                }
            }

            selectedNodes.Clear();
            if (nodeUI == null) return;
            selectedNodes.Add(nodeUI);
            nodeUI.SetSelect(true);
        }
        /// <summary>
        /// Displays the network
        /// Ensure that the network is initalized prior to calling
        /// </summary>
        public void Display() {
            GlobalHelper.DeleteAllChildren(nodeContainer);
            List<int> selectedIds = new List<int>();
            foreach (var selected in selectedNodes)
            {
                selectedIds.Add(selected.GetNode().GetId());
            }
            selectedNodes.Clear();
            nodeUIDict = new Dictionary<TNode, INodeUI>();
            foreach (TNode node in nodeNetwork.GetNodes()) {
                INodeUI nodeUI = GenerateNode(node);
                nodeUIDict[node] = nodeUI;
                if (selectedIds.Contains(node.GetId()))
                {
                    selectedNodes.Add(nodeUI);
                    nodeUI.SetSelect(true);
                }
            }
            
            DisplayLines();
        }

        protected abstract INodeUI GenerateNode(TNode node);
        public abstract bool ShowAllComplete();
        public abstract void OnDeleteSelectedNode();

        protected void SetViewBounds()
        {
            Bounds newBounds = new Bounds();
            foreach (TNode node in nodeNetwork.GetNodes())
            {
                Vector2 position = node.GetPosition();
                
                if (position.x > newBounds.max.x)
                {
                    var max = newBounds.max;
                    max.x = position.x;
                    newBounds.max = max;
                }
                if (position.y > newBounds.max.y)
                {
                    var max = newBounds.max;
                    max.y = position.y;
                    newBounds.max = max;
                }
                
                if (position.x < newBounds.min.x)
                {
                    var min = newBounds.min;
                    min.x = position.x;
                    newBounds.min = min;
                }
                if (position.y < newBounds.min.y)
                {
                    var min = newBounds.min;
                    min.y = position.y;
                    newBounds.min = min;
                }
            }

            newBounds.min += transform.position;
            newBounds.max += transform.position;
            viewBounds = newBounds;
            
        }

        public bool IsSelectingNodes()
        {
            return selectedNodes.Count > 0;
        }

        public void DeleteNode(INode node)
        {
            if (node == null) return;
            
            TNode typeNode = (TNode)node;
            nodeNetwork.GetNodes().Remove(typeNode);
            INodeUI nodeUI = nodeUIDict[typeNode];
            int nodeId = node.GetId();
            foreach (var otherNode in nodeNetwork.GetNodes())
            {
                if (otherNode.GetPrerequisites().Contains(nodeId))
                {
                    otherNode.GetPrerequisites().Remove(nodeId);
                }
            }
            GameObject.Destroy(nodeUI.GetGameObject());
            DisplayLines();
        }

        public int GetNodeOutputs(INode node)
        {
            int count = 0;
            int nodeId = node.GetId();
            foreach (var otherNode in nodeNetwork.GetNodes())
            {
                if (otherNode.GetPrerequisites().Contains(nodeId)) count++;
            }

            return count;
        }

        public void MultiSelectNodes(Vector2 first, Vector2 second)
        {
            Vector2 firstWorld = first-(Vector2)canvasCamera.WorldToScreenPoint(ContentContainer.position);
            Vector2 secondWorld = second-(Vector2)canvasCamera.WorldToScreenPoint(ContentContainer.position);
            Vector2 xBounds = firstWorld.x > secondWorld.x 
                ? new Vector2(secondWorld.x, firstWorld.x) 
                : new Vector2(firstWorld.x, secondWorld.x);
    
            Vector2 yBounds = firstWorld.y > secondWorld.y 
                ? new Vector2(secondWorld.y, firstWorld.y) 
                : new Vector2(firstWorld.y, secondWorld.y);

            foreach (var nodeUI in selectedNodes)
            {
                if (!nodeUI.GetGameObject()) continue;
                nodeUI.SetSelect(false);
            }
            selectedNodes.Clear();
            const float EPILSON = 32;
            foreach (TNode node in nodeNetwork.GetNodes())
            {
                Vector2 position = node.GetPosition();
                if (position.x >= xBounds.x-EPILSON && position.x <= xBounds.y+EPILSON &&
                    position.y >= yBounds.x-EPILSON && position.y <= yBounds.y+EPILSON)
                {
                    var nodeUI = nodeUIDict.GetValueOrDefault(node);
                    if (nodeUI == null) continue;
                    nodeUI.SetSelect(true);
                    selectedNodes.Add(nodeUI);
                }
            }

            if (selectedNodes.Count == 1)
            {
                var nodeUI = selectedNodes[0];
                if (nodeUI is IOnSelectActionNodeUI actionNodeUI)
                {
                    actionNodeUI.OnSelect();
                }
            }
            
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
            RectTransform rectTransform = (RectTransform)nodeUI.GetGameObject().transform;
            Vector3 nodeUIPosition = rectTransform.anchoredPosition;
            nodeUIPosition += changeVector;
            ((RectTransform)nodeUI.GetGameObject().transform).anchoredPosition = nodeUIPosition;
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
                    QuestBookUIFactory.GenerateLine(otherNode.GetPosition(),node.GetPosition(),LineContainer,complete,linePrefab,ref color);
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
            bool selectingNodes = selectedNodes.Count > 0;
            if (CanvasController.Instance.IsTyping) return;
            if (!selectingNodes) KeyPressMoveUpdate();
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SelectNode(null);
            }
            ClampPosition();
            if (!selectingNodes) return;
            
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.E))
            {
                foreach (var node in selectedNodes)
                {
                    DeleteNode(node.GetNode());
                }
                OnDeleteSelectedNode();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (selectedNodes.Count == 1)
                {
                    selectedNodes[0].OpenContent(NodeUIContentOpenMode.KeyPress);
                }
            }
            const float delay = 0.2f;
            foreach (var (keycodes, direction) in moveDirections)
            {
                if (!IsPressed(keycodes,hold:false)) continue;
          
                moveCounter += delay;
                break;
            }
            moveCounter += Time.deltaTime;
            
            if (moveCounter < delay) return;
            
            moveCounter = 0;
            foreach (var (keycodes, direction) in moveDirections)
            {
                if (!IsPressed(keycodes)) continue;
                foreach (var nodeUI in selectedNodes)
                {
                    MoveNode((TNode)nodeUI?.GetNode(),direction);
                }
                
            }
            ClampPosition();


        }

        void ClampPosition()
        {
            if (!viewBounds.HasValue) return;
            Bounds boundsValue = viewBounds.Value;
            Vector3 position = transform.position;
            if (position.x > boundsValue.max.x) position.x = boundsValue.max.x;
            if (position.x < boundsValue.min.x) position.x = boundsValue.min.x;
            if (position.y > boundsValue.max.y) position.y = boundsValue.max.y;
            if (position.y < boundsValue.min.y) position.y = boundsValue.min.y;
            transform.position = position;
        }

        private void KeyPressMoveUpdate()
        {
            const int DIRECTION_COUNT = 2;
            int startIndex = lockHorizontalMovement ? DIRECTION_COUNT : 0;
            int endIndex = lockVerticalMovement ? DIRECTION_COUNT : 2*DIRECTION_COUNT;
            for (int i = startIndex; i < endIndex; i++)
            {
                var (keycodes, direction) = moveDirections[i];
                if (!IsPressed(keycodes)) continue;
                Vector3 position = transform.position;
                const float SPEED = 0.05f;
                position -= SPEED*GetDirectionVector(direction);
                transform.position = position;
            }
        }
        
        
        private void HandleZoom()
        {
            if (lockZoom) return;
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                Vector3 mousePosition = canvasCamera.ScreenToWorldPoint(Input.mousePosition);
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
            if (lockHorizontalMovement && lockVerticalMovement) return;
            
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 mouseWorldPosition = canvasCamera.ScreenToWorldPoint(Input.mousePosition);
                rightClickEvent = new RightClickEvent(mouseWorldPosition-(Vector2)ContentContainer.position);
            }

            if (rightClickEvent != null)
            {
                Vector2 mousePosition = canvasCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 newPosition = rightClickEvent.GetNetworkPosition(mousePosition);
                if (lockHorizontalMovement)
                {
                    newPosition.x = transform.position.x;
                }

                if (lockVerticalMovement)
                {
                    newPosition.y = transform.position.y;
                }
                
                transform.position = newPosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                rightClickEvent = null;
            }
        }
        
        public void ModifyConnection(INode clickedNode)
        {
            if (selectedNodes.Count != 1) return;
            INode selectedNodeElement =selectedNodes[0].GetNode();
            if (selectedNodeElement == null) {
                return;
            }
            if (selectedNodeElement.GetId() == clickedNode.GetId()) {
                return;
            }

            if (connectionFilterer != null && !connectionFilterer.CanConnect(selectedNodeElement, clickedNode)) return;
                
            List<int> clickedPreReqs = clickedNode.GetPrerequisites();
            List<int> selectedPreReqs = selectedNodeElement.GetPrerequisites();
            
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
            OnConnectionModified((TNode)clickedNode);
            OnConnectionModified((TNode)selectedNodeElement);
            DisplayLines();
        }

        public virtual void OnConnectionModified(TNode node)
        {
            
        }

        public Transform GetContentContainer()
        {
            return transform;
        }
        public abstract INode PlaceNewNode(Vector2 position);
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

