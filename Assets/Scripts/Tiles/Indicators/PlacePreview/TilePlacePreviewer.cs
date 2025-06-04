using System.Collections;
using System.Collections.Generic;
using Chunks.Systems;
using Conduits;
using Conduits.Systems;
using Dimensions;
using Item.ItemObjects.Interfaces;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using PlayerModule;
using Tiles;
using Items;
using Items.Transmutable;
using Player;
using Player.Mouse.TilePlaceSearcher;
using PlayerModule.KeyPress;
using PlayerModule.Mouse;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using Tiles.Options.Overlay;
using Tiles.TileMap;
using Tiles.TileMap.Platform;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.InputSystem;

namespace TileMaps.Previewer {
    public class TilePlacePreviewer : MonoBehaviour
    {
        private TilePlacementRecord placementRecord;
        private TilemapRenderer tilemapRenderer;
        private Tilemap tilemap;
        private Tilemap unhighlightedTileMap;
        private Tilemap tileOverlayMap;
        [SerializeField] private Material defaultMaterialShader;
        private DevMode devMode;
        private Color placableColor = new Color(111f/255f,180f/255f,248f/255f);
        private Color nonPlacableColor = new Color(255f/255f,153f/255f,153/255f);
        private Camera mainCamera;
        private PlayerScript playerScript;
        private PlayerMouse playerMouse;
        private Transform playerTransform;
        private TilemapRenderer overlayRenderer;
        private int lastMousePlacement;
        public const int MULTI_TILE_PLACE_OFFSET = 7;
        
        void Start()
        {
            mainCamera = Camera.main;
            tilemap = GetComponent<Tilemap>();

            tilemapRenderer = GetComponent<TilemapRenderer>();
            GameObject unhighlightedContainer = new GameObject();
            unhighlightedContainer.transform.SetParent(transform,false);
            unhighlightedContainer.name = "UnhighlightedTilemap";
            unhighlightedTileMap = unhighlightedContainer.AddComponent<Tilemap>();
            
            TilemapRenderer unhighlightRenderer = unhighlightedContainer.AddComponent<TilemapRenderer>();
            unhighlightRenderer.material = defaultMaterialShader;
            unhighlightedContainer.transform.localPosition = new Vector3(0, 0, 2f);
            
            GameObject tileOverlayContainer = new GameObject();
            tileOverlayContainer.transform.SetParent(transform,false);
            tileOverlayContainer.name = "Overlay map";
            tileOverlayMap = tileOverlayContainer.AddComponent<Tilemap>();
            overlayRenderer = tileOverlayContainer.AddComponent<TilemapRenderer>();
            overlayRenderer.material = defaultMaterialShader;
            tileOverlayContainer.transform.localPosition = new Vector3(0, 0, -1f);

        }

        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            playerMouse = playerScript.PlayerMouse;
        }

        
        void Update()
        {
            if (CanvasController.Instance.IsActive || (Mouse.current.leftButton.isPressed && !(playerScript?.TilePlacementOptions?.AutoPlace ?? false)))
            {
                if (placementRecord == null) return;
                placementRecord.Clear();
                placementRecord = null;
                return;
            }
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            PreviewTile(playerScript?.PlayerInventory.GetSelectedId(), mousePosition);
            
        }   
        public void PreviewTile(string id, Vector2 position) {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            if (itemObject is not IPlacableItem placableItem) {
                placementRecord?.Clear();
                return;
            }

            TileBase tileBase = placableItem.GetTile();
            if (ReferenceEquals(tileBase, null)) return;
            
            int mousePlacement = MousePositionUtils.GetMousePlacement(position);
            
            Vector2Int mouseCellPosition = Global.WorldToCell(position);
            Vector3Int mouseCellPositionVector3 = new Vector3Int(mouseCellPosition.x,mouseCellPosition.y,0);
            
            if (tileBase is not INoDelayPreviewTile && placementRecord != null && placementRecord.RecordMatch(mouseCellPositionVector3,itemObject.id) && mousePlacement == lastMousePlacement) {
                return;
            }
            
            Vector2 tilePlacementWorld = playerMouse.TileSearchResultCacher?.GetResult() ?? position;
            Vector2Int cellPosition = Global.WorldToCell(tilePlacementWorld);
            Vector3Int placePosition = new Vector3Int(cellPosition.x,cellPosition.y,0);
            
            lastMousePlacement = mousePlacement;
            placementRecord?.Clear();
            tilemapRenderer.material = defaultMaterialShader;
            
            if (tileBase is ConduitStateTile conduitStateTile)
            {
                placementRecord = PreviewConduitTile(conduitStateTile,itemObject,placePosition);
            } else if (itemObject is FluidTileItem fluidTileItem)
            {
                placementRecord = PreviewFluidTile(fluidTileItem, tileBase, placePosition);
            }
            else if (itemObject is TileItem tileItem)
            {
                var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
                if (transmutableMaterial && transmutableMaterial.HasShaders)
                {
                    tilemapRenderer.material = ItemRegistry.GetInstance().GetTransmutationUIMaterial(transmutableMaterial);
                }
                if (tileItem.tile is PlatformStateTile)
                {
                    placementRecord = PreviewPlatformTile(playerScript.TilePlacementOptions, tileItem, placePosition, tilePlacementWorld);
                }
                else
                {
                    placementRecord = PreviewStandardTile(playerScript.TilePlacementOptions, tileItem, tileBase, placePosition, tilePlacementWorld);
                }
                
            }

            bool canPlace = CanPlace(tilePlacementWorld, itemObject);
            if (itemObject is IColorableItem colorableItem && colorableItem.Color != Color.white)
            {
                Color baseColor = colorableItem.Color;
                
                Color highlightColor = canPlace ? baseColor : Color.Lerp(baseColor, nonPlacableColor, 0.4f);
                
                tilemap.color = tileOverlayMap.color = highlightColor;
                return;
            }
            
            if (!canPlace)
            {
                tilemap.color = tileOverlayMap.color = nonPlacableColor;
            }
            else
            {
                tilemap.color = tileOverlayMap.color = placableColor;
            }
            
        }

        private bool CanPlace(Vector2 position, ItemObject itemObject)
        {
            ClosedChunkSystem closedChunkSystem = playerScript.CurrentSystem;
            if (!closedChunkSystem) return false;
            switch (itemObject)
            {
                case TileItem tileItem:
                    return TilePlaceUtils.TilePlaceable(new TilePlacementData(playerScript.TilePlacementOptions.Rotation, playerScript.TilePlacementOptions.State), tileItem, position, closedChunkSystem);
                case ConduitItem conduitItem:
                    TileMapType tileMapType = conduitItem.GetConduitType().ToTileMapType();
                    IWorldTileMap conduitMap = closedChunkSystem.GetTileMap(tileMapType);
                    if (conduitMap is not ConduitTileMap conduitTileMap) return false;
                    return TilePlaceUtils.ConduitPlacable(conduitItem, position, conduitTileMap);
                case FluidTileItem fluidTileItem:
                    return TilePlaceUtils.FluidPlacable(fluidTileItem, position, closedChunkSystem.GetFluidTileMap());
                default:
                    return false;
            }
        }

        private SingleTilePlacementRecord PreviewFluidTile(FluidTileItem fluidTileItem, TileBase tileBase, Vector3Int placePosition)
        {
            tilemap.SetTile(placePosition,tileBase);
            if (fluidTileItem.fluidOptions.MaterialColorOverride)
            {
                TransmutationShaderPair shaderPair = ItemRegistry.GetInstance().GetTransmutationMaterial(fluidTileItem.fluidOptions.MaterialColorOverride);
                tilemapRenderer.material = fluidTileItem.fluidOptions.Lit ? shaderPair.UIMaterial : shaderPair.WorldMaterial;
            }
            return new SingleTilePlacementRecord(fluidTileItem.id, placePosition, tilemap, null);
        }

        private MultiStateTilePlacementRecord PreviewPlatformTile(PlayerTilePlacementOptions tilePlacementOptions, TileItem tileItem, Vector3Int placePosition, Vector2 position)
        {
            PlatformTileMap platformTileMap = (PlatformTileMap)playerScript.CurrentSystem.GetTileMap(TileMapType.Platform);
            
            List<Vector3Int> additionalPlacementPositions = new List<Vector3Int>();
            PlaceTile(placePosition,tileItem.tile as PlatformStateTile,tilePlacementOptions,platformTileMap.GetTilemap(),platformTileMap.GetSlopeTilemap(SlopeRotation.Left),platformTileMap.GetSlopeTilemap(SlopeRotation.Right));
            if (!platformTileMap.HasTile(placePosition))
            {
                PlaceAdditionalTile(Vector3Int.left);
                PlaceAdditionalTile(Vector3Int.left+Vector3Int.up);
                PlaceAdditionalTile(Vector3Int.left+Vector3Int.down);
                
                PlaceAdditionalTile(Vector3Int.right);
                PlaceAdditionalTile(Vector3Int.right+Vector3Int.up);
                PlaceAdditionalTile(Vector3Int.right+Vector3Int.down);
            }
            
            return new MultiStateTilePlacementRecord(tileItem.id, tilemap, 3, placePosition, additionalPlacementPositions);

            void PlaceAdditionalTile(Vector3Int direction)
            {
                Vector3Int adjacentPosition = placePosition + direction;
                TileItem adjacentTileItem = platformTileMap.getTileItem((Vector2Int)adjacentPosition);
                if (adjacentTileItem?.tile is not PlatformStateTile platformStateTile) return;
                BaseTileData baseTileData = platformTileMap.GetBaseTileData(adjacentPosition.x, adjacentPosition.y);
                bool flat = baseTileData.state < (int)PlatformTileState.SlopeDeco;
                if (flat) return;
                PlayerTilePlacementOptions placementOptions = new PlayerTilePlacementOptions
                {
                    State = baseTileData.state,
                    Rotation = (PlayerTileRotation)baseTileData.rotation
                };
                PlaceTile(adjacentPosition,platformStateTile,placementOptions,tilemap,tilemap,tilemap);
                additionalPlacementPositions.Add(adjacentPosition);
                
            }
            void PlaceTile(Vector3Int cellPosition, PlatformStateTile platformStateTile, PlayerTilePlacementOptions placementOptions, Tilemap flatMap, Tilemap leftSlopeMap, Tilemap rightSlopeMap)
            {
                int rotation;
                int state = placementOptions.State;
                if (placementOptions.AutoPlace)
                {
                    placementOptions.Rotation = (PlayerTileRotation)placementOptions.AutoBaseTileData.rotation;
                    placementOptions.State = placementOptions.AutoBaseTileData.state;
                }
                else
                {
                    if (placementOptions.State == (int)PlatformTileState.SlopeDeco && cellPosition == placePosition)
                    {
                        rotation = (int)placementOptions.Rotation;
                        if (placementOptions.Rotation == PlayerTileRotation.Auto)
                        {
                            int mousePosition = MousePositionUtils.GetMousePlacement(position);
                            rotation = MousePositionUtils.MouseBiasDirection(mousePosition, MousePlacement.Left) ? 0 : 1;
                        }
                        placementOptions.Rotation = (PlayerTileRotation)rotation;
                    }
                }
                
                TilePlacementData tilePlacementData = new(placementOptions.Rotation, placementOptions.State);
                state = (int)TilePlaceUtils.GetPlacementPlatformState(cellPosition,tilePlacementData,flatMap,leftSlopeMap,rightSlopeMap);
                tilePlacementData.State = state;
                rotation = TilePlaceUtils.GetPlacementPlatformRotation(cellPosition,tilePlacementData,flatMap,leftSlopeMap,rightSlopeMap);
                
                TileBase[] result = new TileBase[3];
                platformStateTile.GetTiles(state,result);
                
                for (int i = 0; i < 3; i++)
                {
                    TileBase tileBase = result[i];
                    Vector3Int tilePlacePosition = cellPosition + Vector3Int.down * (i * MULTI_TILE_PLACE_OFFSET);
                    tilePlacePosition.z = 0;
                    
                    tilemap.SetTile(tilePlacePosition,tileBase);
                    Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(tilePlacePosition);
                    
                    float offset = MULTI_TILE_PLACE_OFFSET*i * Global.TILE_SIZE;
                    
                    const int SLOPE_DECO_INDEX = 2;
                    if (i == SLOPE_DECO_INDEX) offset -= Global.TILE_SIZE; // Deco is one lower
                    transformMatrix.SetTRS(new Vector3(0,offset,0),Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    tilemap.SetTransformMatrix(tilePlacePosition,transformMatrix);
                }

            }

        }
        
        private SingleTilePlacementRecord PreviewStandardTile(PlayerTilePlacementOptions tilePlacementOptions, TileItem tileItem, TileBase itemTileBase, Vector3Int placePosition, Vector2 position)
        {
            
            int state = tilePlacementOptions.State;
            if (itemTileBase is IMousePositionStateTile restrictedTile) {
                state = restrictedTile.GetStateAtPosition(position);
            }
            
            bool rotatable = tileItem.tileOptions.rotatable;
            
            DisplayTilePreview(tilemap,itemTileBase,state,placePosition,position,rotatable,tilePlacementOptions);
            SingleTilePlacementRecord record =  new SingleTilePlacementRecord(tileItem.id, placePosition,tilemap,tileOverlayMap);
            
            var tileOverlay = tileItem.tileOptions.overlayData;
            if (tileOverlay)
            {
                DisplayTilePreview(tileOverlayMap,tileOverlay.GetTile(),state,placePosition,position,rotatable,tilePlacementOptions);
                tileOverlayMap.color = tileOverlay.GetColor();
                
                if (tileOverlay is IShaderTileOverlay shaderTileOverlay)
                {
                    Material material = shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World);
                    overlayRenderer.material = material ? material : defaultMaterialShader;
                }
                else
                {
                    overlayRenderer.material = defaultMaterialShader;
                }
            }
            else
            {
                tileOverlayMap.color = Color.white;
                overlayRenderer.material = defaultMaterialShader;
            }
            
            return record;
        }
        

        private void DisplayTilePreview(Tilemap placementTilemap, TileBase tileBase, int autoState, Vector3Int placePosition, Vector2 position, bool rotatable, PlayerTilePlacementOptions tilePlacementOptions)
        {
            TileBase placementTileBase = tileBase is IStateTileSingle stateTile ? stateTile.GetTileAtState(autoState) : tileBase;
            if (!rotatable)
            {
                placementTilemap.SetTile(placePosition,tileBase);
                return;
            }
            
            PlayerTileRotation tileRotation = tilePlacementOptions.Rotation;

            int rotation = 0;
            if (tileRotation == PlayerTileRotation.Auto)
            {
                if (tileBase is HammerTile)
                {
                    int hammerRotation = MousePositionUtils.CalculateHammerTileRotation(position,tilePlacementOptions.State);
                    if (hammerRotation > 0) rotation = hammerRotation;
                }
            }
            else
            {
                rotation = (int)tileRotation;   
            }
            
            if (placementTileBase is IStateRotationTile stateRotationTile)
            {
                placementTilemap.SetTile(placePosition,stateRotationTile.getTile(rotation,false));
            }
            else
            {
                TilePlaceUtils.RotateTileInMap(placementTilemap, placementTileBase, placePosition,rotation,false);
            }
        }
        
        
        private MultiMapPlacementRecord PreviewConduitTile(ConduitStateTile conduitStateTile, ItemObject itemObject, Vector3Int position)
        {
            if (itemObject is not ConduitItem conduitItem)
            {
                return null;
            }
            ClosedChunkSystem closedChunkSystem = playerScript.CurrentSystem;
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) return null;
            
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
            TileBase previewTile = conduitStateTile.GetTileAtState(previewState);
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
                
                TileBase tile = adjStateTile.GetTileAtState(state);
                unhighlightedTileMap.SetTile(position + direction, tile);
                placePositions.Add(position + direction);
            }
            
            return new MultiMapPlacementRecord(itemObject.id, tilemap, unhighlightedTileMap, position, placePositions);
        }

        public void ClearPlacementRecord()
        {
            placementRecord?.Clear();
            placementRecord = null;
        }
        
        
    }
}