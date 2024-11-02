using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Conduits;
using Conduits.Systems;
using UnityEngine.Tilemaps;
using Chunks.Systems;
using TileEntityModule;

namespace Conduits.PortViewer {
    public class ConduitPortViewer : MonoBehaviour
    {
        private IConduitSystemManager systemManager;
        private Dictionary<EntityPortType,TileBase> portTypeToTile;
        public ConduitType Type {get => systemManager.getConduitType();}
        public bool Active {get => systemManager != null;}
        public void initalize(IConduitSystemManager systemManager, Vector3Int referenceFrame, Dictionary<EntityPortType,TileBase> portTypeToTile, Color color) {
            this.systemManager = systemManager;
            Tilemap tilemap = GetComponent<Tilemap>();
            tilemap.color = color;
            Debug.Log(systemManager.getTileEntityPorts().Count);
            foreach (KeyValuePair<ITileEntityInstance,List<TileEntityPort>> kvp in systemManager.getTileEntityPorts()) {
                foreach (TileEntityPort portData in kvp.Value) {
                    Vector3Int position = (Vector3Int)(kvp.Key.getCellPosition() + portData.position);
                    tilemap.SetTile(position,portTypeToTile[portData.portType]);
                }
            }
        }

        public void deactive() {
            if (!gameObject.activeInHierarchy) {
                return;
            }
            GetComponent<Tilemap>().ClearAllTiles();
            gameObject.SetActive(false);
            systemManager = null;
        }
    }
}

