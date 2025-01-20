using System.Collections;
using System.Collections.Generic;
using Chunks.Systems;
using Conduits;
using Conduits.Systems;
using Dimensions;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using PlayerModule;
using Tiles;
using Items;

namespace TileMaps.Previewer {
    public class TilePlacePreviewer : MonoBehaviour
    {
        private Vector3Int lastPlaceOrigin;
        private TilePlacementRecord placementRecord;
        private Tilemap tilemap;
        private Tilemap unhighlightedTileMap;
        private DevMode devMode;
        private PlayerInventory playerInventory;
        private Color placableColor = new Color(111f/255f,180f/255f,248f/255f);
        private Color nonPlacableColor = new Color(255f/255f,153f/255f,153/255f);
        private Camera mainCamera;

        private Transform playerTransform;
        // Start is called before the first frame update
        void Start()
        {
            playerTransform = GameObject.Find("Player").transform;
            devMode = playerTransform.GetComponent<DevMode>();
            playerInventory = playerTransform.GetComponent<PlayerInventory>();
            mainCamera = Camera.main;
            tilemap = GetComponent<Tilemap>();
            GameObject unhighlightedContainer = new GameObject();
            unhighlightedContainer.transform.SetParent(transform,false);
            unhighlightedContainer.name = "UnhighlightedTilemap";
            unhighlightedTileMap = unhighlightedContainer.AddComponent<Tilemap>();
            unhighlightedContainer.AddComponent<TilemapRenderer>();

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (devMode.placeSelectedID) {
                previewTile(devMode.placeID,mousePosition);
            } else {
                previewTile(playerInventory.getSelectedId(), mousePosition);
            }
            
        }   
        public void previewTile(string id, Vector2 position) {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            if (!itemObject || itemObject is not IPlacableItem placableTile) {
                placementRecord?.Clear();
                return;
            }
            Vector3Int placePosition = PlaceTile.getItemPlacePosition(itemObject,position);
            if (placementRecord != null && placementRecord.RecordMatch(placePosition,itemObject.id)) {
                return;
            }
            placementRecord?.Clear();
            
            TileBase itemTileBase = placableTile.getTile();
            if (itemTileBase is ConduitStateTile conduitStateTile)
            {
                placementRecord = PreviewConduitTile(conduitStateTile,itemObject,placePosition);
            }
            else
            {
                placementRecord = PreviewStandardTile(itemObject, itemTileBase, placePosition, position);
            }
            
            tilemap.color = PlaceTile.ItemPlacable(itemObject,position) ? placableColor : nonPlacableColor;

        }

        private SingleTilePlacementRecord PreviewStandardTile(ItemObject itemObject, TileBase itemTileBase, Vector3Int placePosition, Vector2 position)
        {
            int state = 0;
            if (itemTileBase is IRestrictedTile restrictedTile) {
                state = restrictedTile.getStateAtPosition(position,MousePositionFactory.getVerticalMousePosition(position),MousePositionFactory.getHorizontalMousePosition(position));
            }

            TileBase tileBase;
            if (itemTileBase is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(state);
            } else {
                tileBase = itemTileBase;
            } 
            tilemap.SetTile(placePosition,tileBase);
            return new SingleTilePlacementRecord(itemObject.id, placePosition,tilemap);
        }

        private MultiMapPlacementRecord PreviewConduitTile(ConduitStateTile conduitStateTile, ItemObject itemObject, Vector3Int position)
        {
            if (itemObject is not ConduitItem conduitItem)
            {
                return null;
            }
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem)
            {
                return null;
            }
            List<Vector3Int> placePositions = new List<Vector3Int>{};
            List<Vector3Int> directions = new List<Vector3Int>{Vector3Int.left,Vector3Int.right,Vector3Int.up,Vector3Int.down};
            
            List<ConduitDirectionState> directionStates = new List<ConduitDirectionState>
            {
                ConduitDirectionState.Right, ConduitDirectionState.Left, 
                ConduitDirectionState.Down, ConduitDirectionState.Up
            };
            ConduitType type = conduitItem.GetConduitType();
            IConduitSystemManager manager = conduitTileClosedChunkSystem.GetManager(type);
            

            int previewState = manager.GetNewState((Vector2Int)position, ConduitPlacementMode.Any, itemObject.id);
            bool showAllStates = previewState == 0 && manager.GetConduitAtCellPosition((Vector2Int)position) == null;
            if (showAllStates) // If no connections nearby show all directions cause it looks nicer
            {
                previewState = 15;
            }
            TileBase previewTile = conduitStateTile.getTileAtState(previewState);
            tilemap.SetTile(position, previewTile);
            for (int i = 0; i < directions.Count; i++)
            {
                Vector3Int direction = directions[i];
                IConduit conduit = manager.GetConduitAtCellPosition((Vector2Int)position+(Vector2Int)direction);
                if (conduit == null || conduit.GetId() != conduitItem.id) continue;
                
                ConduitStateTile adjStateTile = conduitItem.Tile;
                int state = conduit.GetState();
                var directionState = directionStates[i];
                if (!conduit.ConnectsDirection(directionState))
                {
                    state += (int) directionState;
                }
                TileBase tile = adjStateTile.getTileAtState(state);
                unhighlightedTileMap.SetTile(position + direction, tile);
                placePositions.Add(position + direction);
            }
            
            return new MultiMapPlacementRecord(itemObject.id, tilemap, unhighlightedTileMap, position, placePositions);
        }
    }
}