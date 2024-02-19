using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;
using ChunkModule.ClosedChunkSystemModule;

namespace ConduitModule.PortViewer {
    public class PortViewerController : MonoBehaviour
    {
        private ConduitTileClosedChunkSystem closedChunkSystem;
        private ConduitPortViewer portViewer;
        private PlayerInventory playerInventory;
        private ItemRegistry itemRegistry;
        
        public void Start() {
            playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
            itemRegistry = ItemRegistry.getInstance();
            closedChunkSystem = transform.parent.GetComponent<ConduitTileClosedChunkSystem>();
        }

        public void Update() {
            string id = playerInventory.getSelectedId();
            if (id == null) {
                deactiveViewer();
                return;
            }
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (conduitItem == null) {
                deactiveViewer();
                return;
            }
            if (portViewer == null || conduitItem.getType() != portViewer.Type) {
                deactiveViewer();
                activateViewer(conduitItem.getType());
                return;
            }
        }

        private void deactiveViewer() {
            if (portViewer == null) {
                return;
            }
            GameObject.Destroy(portViewer.gameObject);
            portViewer = null;
        }

        private void activateViewer(ConduitType conduitType) {
            GameObject viewer = new GameObject();
            viewer.name = conduitType.ToString() + " Port Viewer";
            viewer.transform.SetParent(transform);
            portViewer = viewer.AddComponent<ConduitPortViewer>();
            Vector3Int referenceFrame = (Vector3Int)closedChunkSystem.getBottomLeftCorner();
            portViewer.initalize(closedChunkSystem.getManager(conduitType),referenceFrame);
        }
    }
}

