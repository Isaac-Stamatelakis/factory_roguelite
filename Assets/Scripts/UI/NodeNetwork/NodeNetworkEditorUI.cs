using System.Collections;
using System.Collections.Generic;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.NodeNetwork {
    public class NodeNetworkEditorUI: MonoBehaviour, INodeNetworkEditController
    {
        [SerializeField] private Button addNode;
        [SerializeField] private Button helpHover;
        [SerializeField] private RectTransform multiSelectIndicator;
        private GameObject spawnedNodeObject;
        protected INodeNetworkUI nodeNetworkUI;
        private Camera canvasCamera;
        private MultiSelectAction multiSelectAction;
        public void Start()
        {
            canvasCamera = GetComponentInParent<Canvas>().worldCamera;
        }
        
        public void Initialize(INodeNetworkUI nodeNetworkUI) {
            addNode.onClick.RemoveAllListeners();
            
            this.nodeNetworkUI = nodeNetworkUI;
            addNode.onClick.AddListener(AddButtonClick);
            ToolTipUIDisplayer toolTipUIDisplayer = helpHover.gameObject.AddComponent<ToolTipUIDisplayer>();
            toolTipUIDisplayer.SetMessage("" +
                                          "You are in Developer Mode\n" +
                                          "Click on nodes whilst holding LShift to select them\n" +
                                          "Move selected nodes with WASD\\Arrow Keys\n" +
                                          "Click on nodes to connect them to the selected node\n" +
                                          "Click LShift to deselect nodes\n" +
                                          "Click <b>[Q]</b> to Create New Nodes\n"+
                                          "Click <b>[E]</b> to Delete Selected Node\n" +
                                          "Click <b>[R]</b> to Recenter");
            multiSelectIndicator.transform.SetParent(this.nodeNetworkUI.GetContentContainer());
        }

        
        
        public void OnDestroy() {
            addNode.onClick.RemoveAllListeners();
        }
        
        public void AddButtonClick() {
            spawnedNodeObject = nodeNetworkUI.GenerateNewNodeObject();
            if (!spawnedNodeObject) return;
            spawnedNodeObject.transform.SetParent(nodeNetworkUI.GetNodeContainer(),false);
        }

        public void ClearSpawnedObject()
        {
            if (!spawnedNodeObject) return;
            Destroy(spawnedNodeObject);
        }

        public void Update() {
            SpawnNodePlacement();
            MultiSelect();
        }
        
        public void MultiSelect()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                multiSelectAction = new MultiSelectAction(nodeNetworkUI,Mouse.current.position.ReadValue(),multiSelectIndicator,canvasCamera);
            }
            
            multiSelectAction?.SetCurrentPosition(Mouse.current.position.ReadValue());
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                multiSelectAction?.Terminate();
                multiSelectAction = null;
            }
        }

        private void SpawnNodePlacement() {
            if (!spawnedNodeObject) {
                return;
            }
           
            RectTransform parentRect = (RectTransform)transform.parent;
            Transform contentContainer = nodeNetworkUI.GetContentContainer();
            Vector2 offset = parentRect.anchoredPosition/contentContainer.localScale.x;
            Vector2 mousePosition = new Vector2(Screen.width, Screen.height) * ((Vector2)canvasCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue()) - Vector2.one * 0.5f) / contentContainer.localScale.x - offset;
            Vector2 gridPosition = SnapGrid(mousePosition,((RectTransform)contentContainer).anchoredPosition,contentContainer.localScale.x);
            RectTransform rectTransform = spawnedNodeObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = gridPosition;
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                INode node = nodeNetworkUI.PlaceNewNode(spawnedNodeObject.transform.localPosition);
                nodeNetworkUI.Display();
                nodeNetworkUI.SelectNodeValue(node);
                spawnedNodeObject = null;
                
            }
        }

        private static Vector2 SnapGrid(Vector2 mousePosition, Vector2 containerPosition, float containerScale) {
            float scaledGrid = NodeNetworkConfig.GRID_SIZE*1;
            float snappedX = Mathf.Round((mousePosition.x-containerPosition.x/containerScale) / scaledGrid) * scaledGrid;
            float snappedY = Mathf.Round((mousePosition.y-containerPosition.y/containerScale) / scaledGrid) * scaledGrid;
            return new Vector2(snappedX, snappedY);
        }

        private class MultiSelectAction
        {
            private const float MIN_DISTANCE = 1f;
            private Vector2 InitialPosition;
            private Vector2 TerminalPosition;
            private INodeNetworkUI networkUI;
            private RectTransform indicator;
            private Camera uiCamera;

            public MultiSelectAction(INodeNetworkUI nodeNetworkUI, Vector2 initialPosition, RectTransform indicator, Camera uiCamera)
            {
                networkUI = nodeNetworkUI;
                InitialPosition = initialPosition;
                this.indicator = indicator;
                this.uiCamera = uiCamera;
                Vector2 worldPosition = uiCamera.ScreenToWorldPoint(initialPosition);
                indicator.transform.position = worldPosition;

            }

            public void SetCurrentPosition(Vector2 newScreenPosition)
            {
                TerminalPosition = newScreenPosition;
                Vector2 dif = TerminalPosition-InitialPosition;
                float magnitude = dif.magnitude;
                indicator.gameObject.SetActive(magnitude > MIN_DISTANCE);
                int rotation = 0;
                Vector2 size = new Vector2(Mathf.Abs(dif.x),Mathf.Abs(dif.y));
                switch (dif.x)
                {
                    case < 0 when dif.y < 0:
                        rotation = 180;
                        break;
                    case < 0:
                        rotation = 90;
                        (size.x,size.y) = (size.y, size.x);
                        break;
                    default:
                    {
                        if (dif.y < 0)
                        {
                            rotation = 270;
                            (size.x,size.y) = (size.y, size.x);
                        }
                        else
                        {
                            rotation = 0;
                        }

                        break;
                    }
                }
                indicator.localRotation = Quaternion.Euler(0, 0, rotation);
                Vector2 parentScale = indicator.transform.parent.localScale;
                indicator.transform.localScale = new Vector3(1/parentScale.x, 1/parentScale.y, 1); // Don't want to scale select zone
                indicator.sizeDelta = size;
            }

            public void Terminate()
            {
                Vector2 dif = InitialPosition - TerminalPosition;
                float magnitude = dif.magnitude;
                if (magnitude < MIN_DISTANCE) return;
                indicator.gameObject.SetActive(false);
                networkUI.MultiSelectNodes(InitialPosition,TerminalPosition);
            }
        }
    }
}

