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
        public ConduitType? Type {get => systemManager?.GetConduitType();}
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Tilemap nullConduitTileMap;
        public bool Active {get => systemManager != null;}
        private TileMaps.IWorldTileMap conduitIWorldTileMap;

        private HashSet<Vector2Int> loadedPartitions;
        public void Display(IConduitSystemManager systemManager, Dictionary<EntityPortType,TileBase> portTypeToTile, Color color, TileMaps.IWorldTileMap conduitIWorldTileMap, Vector2Int playerChunkPosition) {
            this.systemManager = systemManager;
            this.conduitIWorldTileMap = conduitIWorldTileMap;
            conduitIWorldTileMap.setHighlight(true);
            tilemap.color = color;
            nullConduitTileMap.color = color * 0.8f;
            const int CHUNK_VIEW_RANGE = 2;
            Vector2Int lowerBoundView = Global.CHUNK_SIZE * (playerChunkPosition - CHUNK_VIEW_RANGE * Vector2Int.one) + Global.CHUNK_SIZE/2 * Vector2Int.one; 
            Vector2Int upperBoundView = Global.CHUNK_SIZE * (playerChunkPosition + CHUNK_VIEW_RANGE * Vector2Int.one) + Global.CHUNK_SIZE/2 * Vector2Int.one; 
            foreach (KeyValuePair<ITileEntityInstance,List<TileEntityPortData>> kvp in systemManager.GetTileEntityPorts()) {
                foreach (TileEntityPortData portData in kvp.Value) {
                    Vector3Int position = (Vector3Int)(kvp.Key.getCellPosition() + portData.position);
                    if (position.x < lowerBoundView.x || position.x > upperBoundView.x || position.y < lowerBoundView.y || position.y > upperBoundView.y) continue;
                    IConduit conduit = this.systemManager.GetConduitAtCellPosition((Vector2Int)position);
                    TileBase tile = portTypeToTile[portData.portType];
                    if (conduit == null)
                    {
                        nullConduitTileMap.SetTile(position,tile);
                    }
                    else
                    {
                        tilemap.SetTile(position,tile);
                    }
                }
            }
        }
        
        public void Deactive() {
            if (!gameObject.activeInHierarchy || systemManager == null) {
                return;
            }
            conduitIWorldTileMap.setHighlight(false);
            tilemap.ClearAllTiles();
            nullConduitTileMap.ClearAllTiles();
            gameObject.SetActive(false);
            
            systemManager = null;
        }
    }
}

