using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NodeNetwork {
    public abstract class NodeNetworkEditorUI<Node> : MonoBehaviour, INodeNetworkEditController where Node : INode
    {
        [SerializeField] private Button addNode;
        [SerializeField] private Button toggleConnectionMode;
        [SerializeField] private Image toggleConnectionPanel;
        private GameObject spawnedNodeObject;
        protected INodeNetworkUI nodeNetworkUI;
        public void init(INodeNetworkUI nodeNetworkUI) {
            addNode.onClick.RemoveAllListeners();
            toggleConnectionMode.onClick.RemoveAllListeners();
            
            this.nodeNetworkUI = nodeNetworkUI;
            addNode.onClick.AddListener(addButtonClick);
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
        
        private void addButtonClick() {
            spawnedNodeObject = nodeNetworkUI.GenerateNewNodeObject();
            spawnedNodeObject.transform.SetParent(nodeNetworkUI.GetNodeContainer(),false);
        }

        public void Update() {
            spawnNodePlacement();
        }

        private void spawnNodePlacement() {
            if (spawnedNodeObject == null) {
                return;
            }
            Vector2 mousePosition = Input.mousePosition;
            Transform contentContainer = nodeNetworkUI.GetContentContainer();
            Vector2 gridPosition = snapGrid(mousePosition,contentContainer.position,contentContainer.localScale.x);
            spawnedNodeObject.transform.position = gridPosition;
            if (Input.GetMouseButton(0)) {
                nodeNetworkUI.PlaceNewNode(spawnedNodeObject.transform.localPosition);
                nodeNetworkUI.Display();
                spawnedNodeObject = null;
            }
        }

        private Vector2 snapGrid(Vector2 mousePosition, Vector2 containerPosition, float containerScale) {
            float scaledGrid = NodeNetworkConfig.GRIDSIZE*containerScale;
            float snappedX = Mathf.Round((mousePosition.x - containerPosition.x) / scaledGrid) * scaledGrid + containerPosition.x;
            float snappedY = Mathf.Round((mousePosition.y - containerPosition.y) / scaledGrid) * scaledGrid + containerPosition.y;
            return new Vector2(snappedX, snappedY);
        }
    }
}

