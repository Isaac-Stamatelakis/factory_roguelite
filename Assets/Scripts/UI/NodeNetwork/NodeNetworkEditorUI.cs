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
            Vector2 mousePosition = canvasCamera.ScreenToWorldPoint(Input.mousePosition);
            Transform contentContainer = nodeNetworkUI.GetContentContainer();
            Vector2 gridPosition = SnapGrid(mousePosition,contentContainer.position,contentContainer.localScale.x);
            spawnedNodeObject.transform.position = gridPosition;
            if (Input.GetMouseButton(0)) {
                nodeNetworkUI.PlaceNewNode(spawnedNodeObject.transform.localPosition);
                nodeNetworkUI.Display();
                spawnedNodeObject = null;
            }
        }

        private static Vector2 SnapGrid(Vector2 mousePosition, Vector2 containerPosition, float containerScale) {
            float scaledGrid = NodeNetworkConfig.GRID_SIZE*containerScale;
            float snappedX = Mathf.Round((mousePosition.x - containerPosition.x) / scaledGrid) * scaledGrid + containerPosition.x;
            float snappedY = Mathf.Round((mousePosition.y - containerPosition.y) / scaledGrid) * scaledGrid + containerPosition.y;
            return new Vector2(snappedX, snappedY);
        }
    }
}

