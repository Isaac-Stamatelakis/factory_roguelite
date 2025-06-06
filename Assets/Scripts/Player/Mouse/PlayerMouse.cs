using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Chunks;
using Chunks.Systems;
using TileMaps.Layer;
using TileMaps;
using TileMaps.Place;
using Chunks.Partitions;
using Conduit.Port.UI;
using TileMaps.Type;
using Conduits.Systems;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Conduits;
using Conduits.PortViewer;
using DevTools;
using Dimensions;
using Items;
using TileEntity;
using Entities;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.ItemObjects.Interfaces;
using Item.Slot;
using Player;
using Player.Controls;
using Player.Mouse;
using Player.Mouse.TilePlaceSearcher;
using Player.Robot;
using Player.Tool;
using PlayerModule.IO;
using PlayerModule.KeyPress;
using Robot.Tool;
using Robot.Tool.Instances;
using Robot.Upgrades;
using RobotModule;
using TileEntity.AssetManagement;
using Tiles.Indicators;
using UI;
using UI.Indicators;
using UI.Indicators.General;
using UI.ToolTip;
using UnityEngine.InputSystem;
using WorldModule;
using MoveDirection = Robot.Tool.MoveDirection;

namespace PlayerModule.Mouse {
    /// <summary>
    /// Handles all player mouse interactions
    /// </summary>
    public class PlayerMouse : MonoBehaviour
    {
        public enum AutoSelectMode
        {
            None,
            Tool,
            TileEntity
        }
        private PlayerInventory playerInventory;
        public PortViewMode ConduitPortViewMode;
        private PlayerRobot playerRobot;
        private Transform playerTransform;
        private PlayerScript playerScript;
        private Camera mainCamera;
        private EventSystem eventSystem;
        private ToolClickHandlerCollection toolClickHandlerCollection;
        private ToolPreviewController previewController = new ToolPreviewController();
        private AutoTileFinder autoTileFinder;
        private TileHighlighter tileHighlighter;
        private bool autoSelectableTool;
        private AutoSelectMode autoSelectMode;
        private List<IWorldTileMap> systemTileMaps = new();
        private ClosedChunkSystem currentSystem;
        private float range;
        private Vector2 toolHitPosition;
        private CanvasController canvasController;
        private bool holdingShift;
        private Vector2? highlightPosition;
        private float timeSinceLastTilePlace;
        private float placeCooldown;
        public TileSearchResultCacher TileSearchResultCacher { get; private set; } = new();
        public const string AUTO_SELECT_KEY = "_option_auto_select";
        
        void Start()
        {
            mainCamera = Camera.main;
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerScript = GetComponent<PlayerScript>();
            playerTransform = transform;
            eventSystem = EventSystem.current;
            autoTileFinder = new AutoTileFinder(transform);
            tileHighlighter = playerScript.TileViewers.tileHighlighter;
            canvasController = CanvasController.Instance;
        }

        public void SetAutoSelect(AutoSelectMode newMode)
        {
            autoSelectMode = newMode;
            switch (autoSelectMode)
            {
                case AutoSelectMode.None:
                    tileHighlighter.Clear();
                    ToolTipController.Instance.HideToolTip(ToolTipType.World);
                    highlightPosition = null;
                    break;
                case AutoSelectMode.Tool:
                    ToolTipController.Instance.HideToolTip(ToolTipType.World);
                    highlightPosition = null;
                    break;
                case AutoSelectMode.TileEntity:
                    tileHighlighter.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AutoSelectMode GetAutoSelectMode()
        {
            return autoSelectMode;
        }

        public void SyncToClosedChunkSystem(ClosedChunkSystem closedChunkSystem)
        {
            systemTileMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Object),
            };
            currentSystem = closedChunkSystem;
            GenerateTileSearcher();
            
        }
        
        void Update()
        {
            timeSinceLastTilePlace += Time.deltaTime;
            
            bool leftClick = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            bool rightClick = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
            
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            
            if (canvasController.IsActive)
            {
                ToolTipController.Instance.HideToolTip(ToolTipType.World);
                return;
            }
            
            toolHitPosition = autoSelectableTool && autoSelectMode == AutoSelectMode.Tool ? AutoSelectTile(mousePosition) : mousePosition;

            if (!currentSystem) return;
            
            if (!canvasController.IsActive) PreviewHighlight(mousePosition);
            
            if (eventSystem.IsPointerOverGameObject()) return;
            if (playerScript.TilePlacementOptions.AutoPlace) TileSearchResultCacher.CallSearcher(mousePosition);
            
            if (!leftClick)
            {
                toolClickHandlerCollection.Terminate();
                playerRobot.SetIsUsingTool(false);
            }
            
            if (!leftClick && !rightClick)  return;
            
            if (!DevMode.Instance.NoReachLimit && Vector2.Distance(transform.position, mousePosition) > range) return;
            
            if (leftClick) {
                LeftClickUpdate(mousePosition,currentSystem);
            }
            if (rightClick) {
                RightClickUpdate(mousePosition,currentSystem);
            }
        }
        

        private Vector2 AutoSelectTile(Vector2 mousePosition)
        {
            IAutoSelectTool autoSelectTool = (IAutoSelectTool)playerInventory.CurrentTool;
            TileMapLayer layer = autoSelectTool.GetAutoSelectLayer();
            if (layer != TileMapLayer.Base) return mousePosition;
            toolHitPosition = autoTileFinder.GetTilePosition(mousePosition,range);
            return toolHitPosition;
        }
        
        public void SetRange(float range)
        {
            this.range = range;
        }

        private void PreviewHighlight(Vector2 mousePosition)
        {
            bool toolPreviewOverride = previewController.Preview(playerInventory.CurrentTool, toolHitPosition);
            playerScript.PlayerUIContainer.IndicatorManager.autoSelectIndicator.SetOverride(toolPreviewOverride);
            if (toolPreviewOverride)
            {
                tileHighlighter.SetOutlineColor(((IPreviewableTool)playerInventory.CurrentTool).GetColor());
                return;
            }
            previewController.ResetRecord();
            
            if (autoSelectMode == AutoSelectMode.Tool)
            {
                IWorldTileMap hitMap = autoTileFinder.GetHitTileMap();
                if (hitMap == null || !hitMap.GetTilemap())
                {
                    tileHighlighter.Clear();
                }
                
                if (playerInventory.CurrentTool is not IAutoSelectTool autoSelectTool) return;
                
                tileHighlighter.SetOutlineColor(autoSelectTool.GetColor());
                ITileGridMap tileGridMap = (ITileGridMap)hitMap;
                if (tileGridMap == null) return;
            
                var cellPosition = hitMap.GetHitTilePosition(toolHitPosition);
                Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
                var outlineTileMapCellData = tileGridMap.GetOutlineCellData(vector3Int);
            
                tileHighlighter.Display(cellPosition,outlineTileMapCellData);
                return;
            }
            
            
            if (autoSelectMode != AutoSelectMode.TileEntity) return;
            foreach (IWorldTileMap worldTileMap in systemTileMaps)
            {
                var result = MousePositionTileMapSearcher.GetNearestTileMapPosition(mousePosition, worldTileMap.GetTilemap(), 5);
                if (!result.HasValue) continue;
                bool highlight = TryHighlight(currentSystem, (result.Value,worldTileMap));
                if (highlight) return;
            }
            ToolTipController.Instance.HideToolTip(ToolTipType.World);
            highlightPosition = null;
            tileHighlighter.Clear();
        }
        

        private bool TryHighlight(ClosedChunkSystem system, (Vector2, IWorldTileMap) result)
        {
            (Vector2 position, IWorldTileMap tilemap) = result;
            if (tilemap is not WorldTileMap worldTileGridMap) return false;
                
            Vector3Int cellPosition = tilemap.GetTilemap().WorldToCell(position);
            ITileEntityInstance tileEntityInstance = worldTileGridMap.GetTileEntityAtPosition((Vector2Int)cellPosition);
            if (Keyboard.current.leftShiftKey.isPressed || tileEntityInstance is not IWorldToolTipTileEntity textPreviewTileEntity || !DisplayTextPreviewToolTip(textPreviewTileEntity))
            {
                ToolTipController.Instance.HideToolTip(ToolTipType.World);
            }
            
            if (!CanRightClickTileEntity(tileEntityInstance, system)) return false;
            cellPosition.z = 0;
            tileHighlighter.SetOutlineColor(Color.yellow);
            tileHighlighter.Display(new Vector2Int(cellPosition.x, cellPosition.y), worldTileGridMap.GetOutlineCellData(cellPosition));
            highlightPosition = position;
            return true;
        }

        private bool DisplayTextPreviewToolTip(IWorldToolTipTileEntity worldToolTipTileEntity)
        {
            Vector2Int spriteSize = Global.GetSpriteSize(TileItem.GetDefaultSprite(worldToolTipTileEntity.GetTile()));
            float verticalOffset = (spriteSize.y / 2 + 1.5f) * Global.TILE_SIZE;
            Vector2 worldPosition = worldToolTipTileEntity.GetWorldPosition() + Vector2.up*verticalOffset;
            
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            
            string text = worldToolTipTileEntity.GetTextPreview();
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            ToolTipController.Instance.ShowWorldToolTip(screenPosition,worldToolTipTileEntity.GetTextPreview());
            return true;
        }

        public static bool CanRightClickTileEntity(ITileEntityInstance tileEntityInstance, ClosedChunkSystem system)
        {
            if (tileEntityInstance is not IRightClickableTileEntity && tileEntityInstance?.GetTileEntity() is not IUITileEntity) return false;
            if (system && !system.Interactable && tileEntityInstance is ILockUnInteractableRightClickTileEntity) return false;
            return tileEntityInstance is not IConditionalRightClickableTileEntity conditionalRightClickableTileEntity || conditionalRightClickableTileEntity.CanRightClick();
        }
        
        public static ILoadedChunk GetChunk(Vector2 mousePosition)
        {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            if (!closedChunkSystem) {
                return null;
            }
            Vector2Int chunkPosition = Global.GetChunkFromWorld(mousePosition);
            return closedChunkSystem.GetChunk(chunkPosition);
        }

        private void ToolClickUpdate(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem)
        {
            var clickHandler = toolClickHandlerCollection.GetOrAddTool(playerInventory.CurrentToolType, playerInventory.CurrentTool);
            
            RobotArmController gunController = playerRobot.gunController;
            gunController.AngleToPosition(mousePosition);
            
            playerRobot.SetIsUsingTool(true);
            clickHandler.Tick(mousePosition, !closedChunkSystem.Interactable);
            
            playerRobot.FaceMousePosition(mousePosition);
        }
        private void LeftClickUpdate(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem) {
            bool drop = HandleDrop(mousePosition);
            if (drop) {
                return;
            }
            ToolClickUpdate(toolHitPosition, closedChunkSystem);
            
        }
        private void RightClickUpdate(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem)
        {
            ItemObject currentPlayerItem = playerInventory.getSelectedItemSlot()?.itemObject;
            
            if (currentPlayerItem is IPlacableItem && HandlePlace(mousePosition, closedChunkSystem)) return;

            if (!UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame) return;
            if (RightClickPort(mousePosition)) return;
            TryClickTileEntity(mousePosition);
        }

        private ConduitType? GetPortClickType()
        {
            switch (ConduitPortViewMode)
            {
                case PortViewMode.Auto:
                    string id = playerInventory.GetSelectedId();
                    if (id == null) return null;
                    ConduitItem conduitItem = ItemRegistry.GetInstance().GetConduitItem(id);
                    if (ReferenceEquals(conduitItem,null)) {
                        return null;
                    }
                    return conduitItem.GetConduitType();
                case PortViewMode.None:
                case PortViewMode.Matrix:
                    return null;
                case PortViewMode.Item:
                    return ConduitType.Item;
                case PortViewMode.Fluid:
                    return ConduitType.Fluid;
                case PortViewMode.Energy:
                    return ConduitType.Energy;
                case PortViewMode.Signal:
                    return ConduitType.Signal;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool RightClickPort(Vector2 mousePosition) {
            ConduitType? conduitType = GetPortClickType();
            if (conduitType == null) return false;
           
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            if (!closedChunkSystem.Interactable) return true; // Return true in this case so click is cancelled out
            
            if (closedChunkSystem is not ConduitClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }
            
            IConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.GetManager(conduitType.Value);
            if (conduitSystemManager is not PortConduitSystemManagerManager portConduitSystemManager) return false;
            Vector2Int cellPosition = Global.WorldToCell(mousePosition);
            IPortConduit conduit = portConduitSystemManager.GetConduitWithPort(cellPosition);
            if (conduit == null) {
                return false;
            }
            
            StopPlayerHorizontalMovement();
            IOConduitPortUI conduitPortUI = MainCanvasController.TInstance.DisplayUIElement<IOConduitPortUI>(MainSceneUIElement.IOPortViewer);
            IOConduitPort conduitPort = conduit.GetPort() as IOConduitPort;
            conduitPortUI.Display(conduitPort,conduit);
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public void TryClickTileEntity(Vector2 mousePosition) {
            if (highlightPosition.HasValue)
            {
                mousePosition = highlightPosition.Value;
            }
            int layers = TileMapLayer.Base.ToRaycastLayers();
            GameObject tilemapObject = MouseUtils.RaycastObject(mousePosition,layers);
            
            if (ReferenceEquals(tilemapObject,null)) return;
            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            Vector2Int mouseCellPosition = new Vector2Int(Mathf.FloorToInt(mousePosition.x*2), Mathf.FloorToInt(mousePosition.y*2));
            Vector2Int? tilePosition = FindTileAtLocation.Find(mouseCellPosition,tilemap);
            if (tilePosition == null) {
                return;
            }
            Vector2Int nonNullPosition = (Vector2Int) tilePosition;
            Vector2 worldPositionTile = new Vector2(nonNullPosition.x/2f,nonNullPosition.y/2f);
            ILoadedChunk chunk = GetChunk(worldPositionTile);
            if (chunk == null) {
                return;
            }
            Vector2Int partitionPosition = Global.GetPartitionFromWorld(worldPositionTile);
            Vector2Int partitionPositionInChunk = partitionPosition -chunk.GetPosition()*Global.PARTITIONS_PER_CHUNK;
            Vector2Int tilePositionInPartition = nonNullPosition-partitionPosition*Global.CHUNK_PARTITION_SIZE;
            IChunkPartition chunkPartition = chunk.GetPartition(partitionPositionInChunk);
            ITileEntityInstance tileEntityInstance = chunkPartition.GetTileEntity(tilePositionInPartition);
            if (!CanRightClickTileEntity(tileEntityInstance, chunk.GetSystem())) return;
            
            
            
            // In cases where the tile entity has both ui and click behavior, holding left shit lets the player interact with the entity instance
            bool clickInstanceInterface = Keyboard.current.leftShiftKey.isPressed;
            if (tileEntityInstance is IStopPlayerRightClickableTileEntity || tileEntityInstance.GetTileEntity() is IUITileEntity)
            {
                StopPlayerHorizontalMovement();
            }
            if (!clickInstanceInterface && tileEntityInstance.GetTileEntity() is IUITileEntity)
            {
                TileEntityAssetRegistry.Instance.DisplayUI(tileEntityInstance);
                return;
            }
            if (tileEntityInstance is not IRightClickableTileEntity rightClickableTileEntity) return;
            
            rightClickableTileEntity.OnRightClick();
            tileHighlighter.Clear();
        }
        
        private void StopPlayerHorizontalMovement()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb.bodyType == RigidbodyType2D.Static) return;
            var velocity = rb.velocity; // Set X Velocity to 0 to prevent slight camera shake
            velocity.x = 0;
            rb.velocity = velocity;
        } 

        private bool HandlePlace(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem) {
            if (!UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame && timeSinceLastTilePlace < placeCooldown) return false;
            timeSinceLastTilePlace = 0;
            
            if (!closedChunkSystem || !closedChunkSystem.Interactable) {
                return false;
            }
            
            ItemSlot selectedSlot = playerInventory.getSelectedItemSlot();
            if (ItemSlotUtils.IsItemSlotNull(selectedSlot)) return false;
            
            Vector2 placePosition = GetPlacePosition();
            
            bool placed = TilePlaceUtils.PlaceFromWorldPosition(playerScript,selectedSlot,placePosition,closedChunkSystem);
            if (placed) {
                playerScript.PlaceUpdate();
                TileSearchResultCacher.ClearSearchResult();
            }
            playerScript.TileViewers.TilePlacePreviewer.ClearPlacementRecord();
            
            return placed;

            Vector2 GetPlacePosition()
            {
                if (!playerScript.TilePlacementOptions.AutoPlace) return mousePosition;
                return TileSearchResultCacher.GetResult() ?? mousePosition;
            }
        }

        public Vector2 CalculateItemVelocity(Vector3 mouseposition)
        {
            float maxVelocity=5f;
            float distanceMultiplierX = 1/2f;
            float distanceMultiplierY = 2f;
            float xDif = mouseposition.x-playerTransform.position.x;
            float yDif = mouseposition.y-playerTransform.position.y;
            
            if (xDif < 0) {
                xDif = Mathf.Max(xDif*distanceMultiplierX,-maxVelocity);
            } else {
                xDif = Mathf.Min(xDif*distanceMultiplierX,maxVelocity);
            }
            if (yDif < 0) {
                yDif = Mathf.Max(yDif*distanceMultiplierY,-maxVelocity);
            } else {
                yDif = Mathf.Min(yDif*distanceMultiplierY,maxVelocity);
            }
            
            return new Vector2(xDif, yDif);
        }
        
        private bool HandleDrop(Vector2 mousePosition) {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return false;
            }
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            if (grabbedItemProperties.ItemSlot == null || grabbedItemProperties.DragEventActive) {
                return false;
            }
            ILoadedChunk chunk = GetChunk(mousePosition);
            if (chunk == null) {
                return false;
            }
            Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
            if (ItemSlotUtils.IsItemSlotNull(grabbedItemProperties.ItemSlot)) return false;
            ItemEntityFactory.SpawnItemEntityWithVelocity(
                new Vector2(transform.position.x,transform.position.y) + spriteCenter,
                grabbedItemProperties.ItemSlot,
                chunk.GetEntityContainer(),
                CalculateItemVelocity(mousePosition)
            );
            grabbedItemProperties.SetItemSlot(null);
            return true;
        }

        public void GenerateTileSearcher()
        {
            ItemSlot itemSlot = playerInventory.getSelectedItemSlot();
            if (itemSlot?.itemObject is TileItem tileItem)
            {
                var searcher = TilePlacementSearcherFactory.GetSearcher(currentSystem, playerScript, tileItem.tile, tileItem.tileType);
                TileSearchResultCacher.SetSearcher(searcher);
                return;
            }
            TileSearchResultCacher.SetSearcher(null);
        }

        public void SyncTilePlacementCooldown()
        {
            if (WorldManager.GetInstance().WorldLoadType == WorldManager.WorldType.Structure)
            {
                placeCooldown = 0;
                return;
            }
            float tilePlacementUpgrades = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.TilePlacementRate);
            const float MIN_PLACEMENT_RATE = 0.25f;
            const float MAX_PLACEMENT_RATE = 0.0f;
            const int MAX_UPGRADES = 10;
            placeCooldown = Mathf.Lerp(MIN_PLACEMENT_RATE,MAX_PLACEMENT_RATE,tilePlacementUpgrades/MAX_UPGRADES);
        }
        
        

        public void UpdateOnToolChange()
        {
            autoSelectableTool = playerInventory.CurrentTool is IAutoSelectTool;
            
            RobotArmController gunController = playerRobot.gunController;
            IRobotToolInstance currentTool = playerInventory.CurrentTool;
            gunController.PlayAnimationState(currentTool.GetRobotArmAnimation(),currentTool.GetSubState());
            
            if (playerInventory.CurrentTool is IColorableTool colorableTool)
            {
                gunController.SetToolColor(colorableTool.GetColor());
            }
            
            
            ClearToolPreview();
        }

        public void ClearToolPreview()
        {
            tileHighlighter.ResetHistory();
            tileHighlighter.Clear();
            playerScript.TileViewers.tileHighlighter.ResetHistory();
            playerScript.TileViewers.tileHighlighter.Clear();
            previewController.ResetRecord();
        }

        public void Initialize()
        {
            toolClickHandlerCollection = new ToolClickHandlerCollection(playerRobot);
            SetAutoSelect((AutoSelectMode)PlayerPrefs.GetInt(AUTO_SELECT_KEY));
        }

        public void OnDestroy()
        {
            PlayerPrefs.SetInt(AUTO_SELECT_KEY,(int)autoSelectMode);
        }
    }

    
    internal class AutoTileFinder
    {
        private IWorldTileMap hitTileMap;
        private Transform playerTransform;
        private int castLayer;
        #if UNITY_EDITOR
        public bool debug = false;
        #endif

        public AutoTileFinder(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
            castLayer = TileMapLayer.Base.ToRaycastLayers();
        }

        public Vector2 GetTilePosition(Vector2 mousePosition, float range)
        {
            var nullableResult = FindTile(mousePosition,range);
            return nullableResult ?? mousePosition;
        }
        

        public IWorldTileMap GetHitTileMap()
        {
            return hitTileMap;
        }
        private Vector2? FindTile(Vector2 mousePosition, float range)
        {
            Vector2 position = playerTransform.position;
            float defaultAngle = Mathf.Atan2(mousePosition.y - position.y, mousePosition.x - position.x);
            
            
            Vector2 hitDirection = new Vector2(Mathf.Cos(defaultAngle), Mathf.Sin(defaultAngle));;
            
            RaycastHit2D closestHit = Cast(hitDirection);
            RaycastHit2D Cast(Vector2 direction)
            {
                #if UNITY_EDITOR
                if (debug)Debug.DrawLine(position, position + direction * range, Color.red, 0.1f);
                #endif
                return Physics2D.Raycast(position, direction, range, castLayer);
            }

            const float SPREAD = 5 * Mathf.Deg2Rad;
            const int CASTS = 2;
            int r = 1;
            while (r <= CASTS/2)
            {
                void UpdateHit(int dir)
                {
                    float castAngle = defaultAngle + dir * r * SPREAD / CASTS / 2;
                    Vector2 direction = new Vector2(Mathf.Cos(castAngle), Mathf.Sin(castAngle));
                    var hit = Cast(direction);
                    if (!hit.collider) return;
                    if (closestHit.collider && hit.distance >= closestHit.distance) return;
                    hitDirection = direction;
                    closestHit = hit;
                }
                UpdateHit(1);
                UpdateHit(-1);
                r++;
            }
            if (!closestHit) return null;
            IWorldTileMap tilemap = closestHit.collider.GetComponent<IWorldTileMap>();
            if (tilemap == null) return null;
            hitTileMap = tilemap;
            Vector2 result = closestHit.point + hitDirection * 0.01f;
            #if UNITY_EDITOR
            if (debug) Debug.DrawLine(result,playerTransform.position,Color.green,0.1f);
            #endif
            return result;
        }
    }

    internal class ToolPreviewController
    {
        private Vector2 lastMousePosition;
        private Vector2Int lastTilePosition;
        private bool lastPreviewState;
        
        public bool Preview(IRobotToolInstance robotToolInstance, Vector2 mousePosition)
        {
            if (robotToolInstance is not IPreviewableTool previewableTool) return false;
       
            if (mousePosition == lastMousePosition) return lastPreviewState;
            Vector2Int tilePosition = Global.WorldToCell(mousePosition);
            if (lastTilePosition == tilePosition) return lastPreviewState;
            
            lastMousePosition = mousePosition;
            lastTilePosition = tilePosition;
            lastPreviewState = true;
            return previewableTool.Preview(tilePosition);
        }
        

        public void ResetRecord()
        {
            if (lastPreviewState == false) return;
            lastPreviewState = false;
            lastMousePosition = Vector2.negativeInfinity;
            lastTilePosition = Vector2Int.one * int.MaxValue;
        }
    }
}

