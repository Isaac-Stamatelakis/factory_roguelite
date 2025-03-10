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
using Player;
using PlayerModule.KeyPress;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;

namespace TileMaps.Previewer {
    public class TilePlacePreviewer : MonoBehaviour
    {
        private TilePlacementRecord placementRecord;
        private Tilemap tilemap;
        private Tilemap unhighlightedTileMap;
        private Tilemap tileOverlayMap;
        private DevMode devMode;
        private Color placableColor = new Color(111f/255f,180f/255f,248f/255f);
        private Color nonPlacableColor = new Color(255f/255f,153f/255f,153/255f);
        private Camera mainCamera;
        private PlayerScript playerScript;
        private Transform playerTransform;

        private int lastMousePlacement;
        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;
            tilemap = GetComponent<Tilemap>();
            
            GameObject unhighlightedContainer = new GameObject();
            unhighlightedContainer.transform.SetParent(transform,false);
            unhighlightedContainer.name = "UnhighlightedTilemap";
            unhighlightedTileMap = unhighlightedContainer.AddComponent<Tilemap>();
            unhighlightedContainer.AddComponent<TilemapRenderer>();
            unhighlightedContainer.transform.localPosition = new Vector3(0, 0, 2f);
            
            GameObject tileOverlayContainer = new GameObject();
            tileOverlayContainer.transform.SetParent(transform,false);
            tileOverlayContainer.name = "Overlay map";
            tileOverlayMap = tileOverlayContainer.AddComponent<Tilemap>();
            tileOverlayContainer.AddComponent<TilemapRenderer>();
            tileOverlayContainer.transform.localPosition = new Vector3(0, 0, -1f);
            
            
        }

        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (PlayerKeyPressUtils.BlockKeyInput)
            {
                if (placementRecord == null) return;
                placementRecord.Clear();
                placementRecord = null;

                return;
            }
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            PreviewTile(playerScript?.PlayerInventory.GetSelectedId(), mousePosition);
            
        }   
        public void PreviewTile(string id, Vector2 position) {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            if (itemObject is not IPlacableItem placableItem) {
                placementRecord?.Clear();
                return;
            }

            TileBase tileBase = placableItem.getTile();
            if (ReferenceEquals(tileBase, null)) return;
            
            int mousePlacement = MousePositionUtils.GetMousePlacement(position);
            
            Vector3Int placePosition = PlaceTile.getItemPlacePosition(itemObject,position);
            
            if (tileBase is not INoDelayPreviewTile && placementRecord != null && placementRecord.RecordMatch(placePosition,itemObject.id) && mousePlacement == lastMousePlacement) {
                return;
            }
            lastMousePlacement = mousePlacement;
            placementRecord?.Clear();
            
            if (tileBase is ConduitStateTile conduitStateTile)
            {
                placementRecord = PreviewConduitTile(conduitStateTile,itemObject,placePosition);
            }
            else
            {
                placementRecord = PreviewStandardTile(playerScript.TilePlacementOptions, itemObject as TileItem, tileBase, placePosition, position);
            }

            tilemap.color = GetPlaceColor(position, itemObject);
            
        }

        private Color GetPlaceColor(Vector2 position, ItemObject itemObject)
        {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            switch (itemObject)
            {
                case TileItem tileItem:
                    return PlaceTile.TilePlacable(new TilePlacementData(playerScript.TilePlacementOptions.Rotation, playerScript.TilePlacementOptions.State), tileItem, position, closedChunkSystem) ? placableColor : nonPlacableColor;
                case ConduitItem conduitItem:
                    TileMapType tileMapType = conduitItem.GetConduitType().ToTileMapType();
                    IWorldTileMap conduitMap = closedChunkSystem.GetTileMap(tileMapType);
                    if (conduitMap is not ConduitTileMap conduitTileMap) return nonPlacableColor;
                    return PlaceTile.ConduitPlacable(conduitItem, position, conduitTileMap) ? Color.white : nonPlacableColor;
                default:
                    return Color.white;
            }
        }

        private SingleTilePlacementRecord PreviewStandardTile(PlayerTilePlacementOptions tilePlacementOptions, TileItem tileItem, TileBase itemTileBase, Vector3Int placePosition, Vector2 position)
        {
            int state = tilePlacementOptions.State;
            if (itemTileBase is IMousePositionStateTile restrictedTile) {
                state = restrictedTile.GetStateAtPosition(position);
            }

            TileBase tileBase;
            if (itemTileBase is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(state);
            } else {
                tileBase = itemTileBase;
            }

            SingleTilePlacementRecord record =  new SingleTilePlacementRecord(tileItem.id, placePosition,tilemap,tileOverlayMap);
            bool rotatable = tileItem.tileOptions.rotatable;
            TileBase overlayTile = tileItem.tileOptions.Overlay.Tile;
            if (overlayTile)
            {
                if (rotatable)
                {
                    PlaceTile.RotateTileInMap(tileOverlayMap, overlayTile, placePosition,tilePlacementOptions.Rotation,false);
                }
                else
                {
                    tileOverlayMap.SetTile(placePosition,overlayTile);
                }
                tileOverlayMap.color = tileItem.tileOptions.Overlay.Color;
            }
            else
            {
                tileOverlayMap.color = Color.white;
            }
            if (!rotatable)
            {
                tilemap.SetTile(placePosition,tileBase);
                return record;
            }
            
            int rotation = tilePlacementOptions.Rotation;
            if (itemTileBase is HammerTile)
            {
                int hammerRotation = MousePositionUtils.CalculateHammerTileRotation(position,tilePlacementOptions.State);
                if (hammerRotation > 0) rotation = hammerRotation;
            }

            if (tileBase is IStateRotationTile stateRotationTile)
            {
                tilemap.SetTile(placePosition,stateRotationTile.getTile(rotation,false));
            }
            else
            {
                PlaceTile.RotateTileInMap(tilemap, tileBase, placePosition,rotation,false);
            }
            
            return record;
        }
        
        

        

        private MultiMapPlacementRecord PreviewConduitTile(ConduitStateTile conduitStateTile, ItemObject itemObject, Vector3Int position)
        {
            if (itemObject is not ConduitItem conduitItem)
            {
                return null;
            }
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
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
            
            int previewState = manager.GetNewState((Vector2Int)position, playerScript.ConduitPlacementOptions, itemObject.id);
            bool showAllStates = previewState == 0 && manager.GetConduitAtCellPosition((Vector2Int)position) == null && manager.GetAdjacentConduitCount((Vector2Int)position,itemObject.id) == 0;
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
                if (!playerScript.ConduitPlacementOptions.CanConnect(conduit)) continue;
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