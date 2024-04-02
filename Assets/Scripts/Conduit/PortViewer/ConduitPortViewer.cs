using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using ConduitModule;
using ConduitModule.Systems;
using UnityEngine.Tilemaps;
using ChunkModule.ClosedChunkSystemModule;
using TileEntityModule;

namespace ConduitModule.PortViewer {
    public class ConduitPortViewer : MonoBehaviour
    {
        private IConduitSystemManager systemManager;
        private Tilemap mTilemap;
        private TilemapRenderer mTilemapRender;
        private Dictionary<EntityPortType,TileBase> portTypeToTile;
        private Grid mGrid;
        public ConduitType Type {get => systemManager.getConduitType();}
        public void initalize(IConduitSystemManager systemManager, Vector3Int referenceFrame) {
            this.systemManager = systemManager;
            mTilemap = gameObject.AddComponent<Tilemap>();
            mTilemapRender = gameObject.AddComponent<TilemapRenderer>();
            mTilemapRender.material = Resources.Load<Material>("Material/ShadedMaterial");
            mGrid = gameObject.AddComponent<Grid>();
            mGrid.cellSize = new Vector3(0.5f,0.5f,1f);
            initTiles();
            foreach (KeyValuePair<TileEntity,List<TileEntityPort>> kvp in systemManager.getTileEntityPorts()) {
                foreach (TileEntityPort portData in kvp.Value) {
                    Vector3Int position = (Vector3Int)(kvp.Key.getCellPosition() + portData.position);
                    mTilemap.SetTile(position,portTypeToTile[portData.portType]);
                }
            }
        }

        private void initTiles() {
            portTypeToTile = new Dictionary<EntityPortType, TileBase>();
            string path = "PortTiles/" + Type.ToString();
            TileBase[] tiles = Resources.LoadAll<TileBase>(path);
            foreach (TileBase tileBase in tiles) {
                if (tileBase is IIDTile) {
                    string id = ((IIDTile) tileBase).getId();
                    switch (id) {
                        case "All":
                            portTypeToTile[EntityPortType.All] = tileBase;
                            break;
                        case "Input":
                            portTypeToTile[EntityPortType.Input] = tileBase;
                            break;
                        case "Output":
                            portTypeToTile[EntityPortType.Output] = tileBase;
                            break;
                    }
                }
            }
        }
    }
}

