using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NodeNetwork {
    public class NodeNetworkEditorUI: MonoBehaviour, INodeNetworkEditController
    {
        [SerializeField] private Button addNode;
        [SerializeField] private Button toggleConnectionMode;
        [SerializeField] private Image toggleConnectionPanel;
        private GameObject spawnedNodeObject;
        protected INodeNetworkUI nodeNetworkUI;
        private Camera canvasCamera;
        public void Start()
        {
            canvasCamera = GetComponentInParent<Canvas>().worldCamera;
        }
        public void Initialize(INodeNetworkUI nodeNetworkUI) {
            addNode.onClick.RemoveAllListeners();
            toggleConnectionMode.onClick.RemoveAllListeners();
            
            this.nodeNetworkUI = nodeNetworkUI;
            addNode.onClick.AddListener(AddButtonClick);
            SetConnectionButtonColor();
            toggleConnectionMode.onClick.AddListener(() => {
                NodeNetworkUIMode mode = nodeNetworkUI.GetMode();
                mode = mode == NodeNetworkUIMode.EditConnection
                    ? NodeNetworkUIMode.View
                    : NodeNetworkUIMode.EditConnection;
                nodeNetworkUI.SetMode(mode);
                SetConnectionButtonColor();
            });
        }

        private void SetConnectionButtonColor()
        {
            NodeNetworkUIMode mode = nodeNetworkUI.GetMode();
            nodeNetworkUI.SelectNode(null);
            switch (mode) {
                case NodeNetworkUIMode.View:
                    toggleConnectionPanel.color = new Color(0,1f,0f,200f/255f);
                    break;
                case NodeNetworkUIMode.EditConnection:
                    toggleConnectionPanel.color = new Color(1f,0f,0f,200f/255f);
                    break;
            }
        }

        public void OnDestroy() {
            addNode.onClick.RemoveAllListeners();
        }
        
        private void AddButtonClick() {
            spawnedNodeObject = nodeNetworkUI.GenerateNewNodeObject();
            spawnedNodeObject.transform.SetParent(nodeNetworkUI.GetNodeContainer(),false);
        }

        public void Update() {
            SpawnNodePlacement();
        }

        private void SpawnNodePlacement() {
            if (!spawnedNodeObject) {
                return;
            }
            
            RectTransform parentRect = (RectTransform)transform.parent;
            Transform contentContainer = nodeNetworkUI.GetContentContainer();
            Vector2 offset = parentRect.anchoredPosition/contentContainer.localScale.x;
            Vector2 mousePosition = new Vector2(Screen.width, Screen.height) * ((Vector2)canvasCamera.ScreenToViewportPoint(Input.mousePosition) - Vector2.one * 0.5f) / contentContainer.localScale.x - offset;
            Vector2 gridPosition = SnapGrid(mousePosition,((RectTransform)contentContainer).anchoredPosition,contentContainer.localScale.x);
            RectTransform rectTransform = spawnedNodeObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = gridPosition;
            if (Input.GetMouseButton(0)) {
                nodeNetworkUI.PlaceNewNode(spawnedNodeObject.transform.localPosition);
                nodeNetworkUI.Display();
                spawnedNodeObject = null;
            }
        }

        private static Vector2 SnapGrid(Vector2 mousePosition, Vector2 containerPosition, float containerScale) {
            float scaledGrid = NodeNetworkConfig.GRID_SIZE*1;
            float snappedX = Mathf.Round((mousePosition.x-containerPosition.x/containerScale) / scaledGrid) * scaledGrid;
            float snappedY = Mathf.Round((mousePosition.y-containerPosition.y/containerScale) / scaledGrid) * scaledGrid;
            return new Vector2(snappedX, snappedY);
        }
    }
}

