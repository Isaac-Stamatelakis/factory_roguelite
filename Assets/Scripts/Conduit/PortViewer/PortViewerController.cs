using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;
using Chunks.Systems;
using Items;
using UnityEngine.Tilemaps;
using TileEntity;
using Conduits.Ports;
using UI;
using Conduits.Systems;
using Player;
using TileMaps.Type;
using TileMaps;

namespace Conduits.PortViewer {
    public class PortViewerController : MonoBehaviour
    {
        public UIAssetManager AssetManager;
        [SerializeField] private ConduitPortTiles portConduitTiles;
        private ConduitTileClosedChunkSystem closedChunkSystem;
        private ConduitPortViewer portViewer;
        private PlayerInventory playerInventory;
        private ItemRegistry itemRegistry;
        
        public void initalize(ConduitTileClosedChunkSystem closedChunkSystem) {
            this.closedChunkSystem = closedChunkSystem;
            playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            itemRegistry = ItemRegistry.GetInstance();
            portViewer = GetComponentInChildren<ConduitPortViewer>();
            if (portViewer == null) {
                Debug.LogWarning("Conduit Port Controller has no viewer child");
            }
            AssetManager.load();
        }

        public void Update() {
            string id = playerInventory.getSelectedId();
            if (id == null) {
                portViewer.deactive();
                return;
            }
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (ReferenceEquals(conduitItem,null)) {
                portViewer.deactive();
                return;
            }
            if (!portViewer.Active) {
                activateViewer(conduitItem.GetConduitType());
                return;
            }
            if (conduitItem.GetConduitType() != portViewer.Type) {
                portViewer.deactive();
                activateViewer(conduitItem.GetConduitType());
                return;
            }
        }


        private void activateViewer(ConduitType conduitType) {
            portViewer.name = $"{conduitType} Port Viewer";
            portViewer.gameObject.SetActive(true);

            Vector3Int referenceFrame = (Vector3Int)closedChunkSystem.GetBottomLeftCorner();
            Color color = getConduitPortColor(conduitType);

            Dictionary<EntityPortType,TileBase> portTypeToTile = new Dictionary<EntityPortType, TileBase>();
            portTypeToTile[EntityPortType.All] = portConduitTiles.AnyTile;
            portTypeToTile[EntityPortType.Input] = portConduitTiles.InputTile;
            portTypeToTile[EntityPortType.Output] = portConduitTiles.OutputTile;

            IConduitSystemManager conduitSystemManager = closedChunkSystem.GetManager(conduitType);
            TileMapType tileMapType = conduitType.ToTileMapType();
            TileMaps.IWorldTileMap tilemap = closedChunkSystem.GetTileMap(tileMapType);
            
            portViewer.display(conduitSystemManager,referenceFrame,portTypeToTile,color,tilemap);
        }

        private Color getConduitPortColor(ConduitType conduitType) {
            switch (conduitType) {
                case ConduitType.Item:
                    return Color.green;
                case ConduitType.Fluid:
                    return Color.blue;
                case ConduitType.Energy:
                    return Color.yellow;
                case ConduitType.Signal:
                    return Color.red;
                case ConduitType.Matrix:
                    return Color.magenta;
            }
            throw new System.Exception($"Did not cover case for ConduitType {conduitType}");
        }
    }

    [System.Serializable]
    public struct ConduitPortTiles {
        public TileBase InputTile;
        public TileBase OutputTile;
        public TileBase AnyTile;
    }
}

