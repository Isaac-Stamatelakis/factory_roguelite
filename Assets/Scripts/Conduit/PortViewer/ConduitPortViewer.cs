using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Conduits;
using Conduits.Systems;
using UnityEngine.Tilemaps;
using Chunks.Systems;
using TileEntity;

namespace Conduits.PortViewer {
    public class ConduitPortViewer : MonoBehaviour
    {
        private IConduitSystemManager systemManager;
        private Dictionary<EntityPortType,TileBase> portTypeToTile;
        public ConduitType Type {get => systemManager.GetConduitType();}
        private Tilemap tilemap;
        public void Start() {
            this.tilemap = GetComponent<Tilemap>();
        }
        public bool Active {get => systemManager != null;}
        private TileMaps.ITileMap conduitTileMap;
        public void display(IConduitSystemManager systemManager, Vector3Int referenceFrame, Dictionary<EntityPortType,TileBase> portTypeToTile, Color color, TileMaps.ITileMap conduitTileMap) {
            this.systemManager = systemManager;
            this.conduitTileMap = conduitTileMap;
            conduitTileMap.setHighlight(true);
            tilemap.color = color;
            foreach (KeyValuePair<ITileEntityInstance,List<TileEntityPort>> kvp in systemManager.GetTileEntityPorts()) {
                foreach (TileEntityPort portData in kvp.Value) {
                    Vector3Int position = (Vector3Int)(kvp.Key.getCellPosition() + portData.position);
                    tilemap.SetTile(position,portTypeToTile[portData.portType]);
                }
            }
        }

        public void deactive() {
            if (!gameObject.activeInHierarchy || systemManager == null) {
                return;
            }
            conduitTileMap.setHighlight(false);
            tilemap.ClearAllTiles();
            gameObject.SetActive(false);
            
            systemManager = null;
        }
    }
}

