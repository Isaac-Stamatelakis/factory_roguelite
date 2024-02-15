using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using ConduitModule;
using ConduitModule.ConduitSystemModule;
using UnityEngine.Tilemaps;
using ChunkModule.ClosedChunkSystemModule;

namespace ConduitModule.PortViewer {
    public class ConduitPortViewer : MonoBehaviour
    {
        private ConduitSystemManager systemManager;
        private Tilemap mTilemap;
        private TilemapRenderer mTilemapRender;
        private Dictionary<ConduitPortType,TileBase> portTypeToTile;
        private Grid mGrid;
        public ConduitType Type {get => systemManager.Type;}
        public void initalize(ConduitSystemManager systemManager, Vector3Int referenceFrame) {
            this.systemManager = systemManager;
            mTilemap = gameObject.AddComponent<Tilemap>();
            mTilemapRender = gameObject.AddComponent<TilemapRenderer>();
            mTilemapRender.material = Resources.Load<Material>("Material/ShadedMaterial");
            mGrid = gameObject.AddComponent<Grid>();
            mGrid.cellSize = new Vector3(0.5f,0.5f,1f);
            initTiles();
            foreach (KeyValuePair<Vector2Int,List<ConduitPortData>> kvp in systemManager.ChunkConduitPorts) {
                foreach (ConduitPortData portData in kvp.Value) {
                    Vector3Int position = new Vector3Int(portData.position.x,portData.position.y,0)+referenceFrame;
                    mTilemap.SetTile(position,portTypeToTile[portData.portType]);
                }
            }
        }

        private void initTiles() {
            portTypeToTile = new Dictionary<ConduitPortType, TileBase>();
            string path = "PortTiles/" + Type.ToString();
            TileBase[] tiles = Resources.LoadAll<TileBase>(path);
            foreach (TileBase tileBase in tiles) {
                if (tileBase is IIDTile) {
                    string id = ((IIDTile) tileBase).getId();
                    switch (id) {
                        case "All":
                            portTypeToTile[ConduitPortType.All] = tileBase;
                            break;
                        case "Input":
                            portTypeToTile[ConduitPortType.Input] = tileBase;
                            break;
                        case "Output":
                            portTypeToTile[ConduitPortType.Output] = tileBase;
                            break;
                    }
                }
            }
        }
    }
}

