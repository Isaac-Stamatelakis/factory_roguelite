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
using Player.Robot;
using Player.Tool;
using PlayerModule.IO;
using PlayerModule.KeyPress;
using Robot.Tool;
using Robot.Upgrades;
using TileEntity.AssetManagement;
using Tiles.Highlight;
using Tiles.Indicators;
using UI;
using UI.Indicators;
using UI.Indicators.General;
using UI.ToolTip;
using MoveDirection = Robot.Tool.MoveDirection;

namespace PlayerModule.Mouse {
    /// <summary>
    /// Handles all player mouse interactions
    /// </summary>
    public class PlayerMouse : MonoBehaviour
    {
        private PlayerInventory playerInventory;
        public PortViewMode ConduitPortViewMode;
        private PlayerRobot playerRobot;
        private Transform playerTransform;
        private PlayerScript playerScript;
        private Camera mainCamera;
        private EventSystem eventSystem;
        private ToolClickHandlerCollection toolClickHandlerCollection = new ToolClickHandlerCollection();
        private ToolPreviewController previewController = new ToolPreviewController();
        private AutoTileFinder autoTileFinder;
        private TileHighlighter tileHighlighter;
        private TileBreakHighlighter tileBreakHighlighter;
        private bool autoSelectableTool;
        private bool enableAutoSelect = true;
        public bool AutoSelectEnabled => enableAutoSelect;
        public const string AUTO_SELECT_PREF_KEY = "_mouse_auto_select";
        private List<IWorldTileMap> systemTileMaps = new();
        private ClosedChunkSystem currentSystem;
        private float range;
        private Vector2 toolHitPosition;
        private CanvasController canvasController;
        void Start()
        {
            mainCamera = Camera.main;
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerScript = GetComponent<PlayerScript>();
            playerTransform = transform;
            eventSystem = EventSystem.current;
            autoTileFinder = new AutoTileFinder(transform);
            tileHighlighter = playerScript.TileViewers.TileHighlighter;
            tileBreakHighlighter = playerScript.TileViewers.MainBreakHighlighter;
            enableAutoSelect = PlayerPrefs.GetInt(AUTO_SELECT_PREF_KEY) != 0;
            canvasController = CanvasController.Instance;
        }

        public bool ToggleAutoSelect()
        {
            enableAutoSelect = !enableAutoSelect;
            PlayerPrefs.SetInt(AUTO_SELECT_PREF_KEY, enableAutoSelect ? 1 : 0);
            if (!enableAutoSelect) tileBreakHighlighter.Clear();
            return enableAutoSelect;
        }

        public void SyncToClosedChunkSystem(ClosedChunkSystem closedChunkSystem)
        {
            systemTileMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Object),
            };
            currentSystem = closedChunkSystem;
        }
        
        void Update()
        {
            InventoryControlUpdate();
            
            bool leftClick = Input.GetMouseButton(0);
            bool rightClick = Input.GetMouseButton(1);
            bool scroll = Input.mouseScrollDelta.y != 0;
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (canvasController.BlockKeyInput)
            {
                if (eventSystem.IsPointerOverGameObject())
                {
                    if (scroll)
                    {
                        MouseScrollUIUpdate(mousePosition);
                    }
                }
                ToolTipController.Instance.HideToolTip(ToolTipType.World);
                return;
            }
            
            toolHitPosition = autoSelectableTool && enableAutoSelect ? AutoSelectTile(mousePosition) : mousePosition;

            if (!currentSystem)  return;
            
            if (!canvasController.IsActive) PreviewHighlight(mousePosition);
            
            if (eventSystem.IsPointerOverGameObject()) return;
            
            if (!leftClick)
            {
                toolClickHandlerCollection.Terminate(MouseButtonKey.Left);
            }
            
            if (!rightClick)
            {
                toolClickHandlerCollection.Terminate(MouseButtonKey.Right);
            }

            if (!leftClick && !rightClick)
            {
                playerRobot.SetIsUsingTool(false);
                return;
            }
            
            if (!DevMode.Instance.NoReachLimit && Vector2.Distance(transform.position, mousePosition) > range) return;
            
            if (leftClick) {
                LeftClickUpdate(mousePosition,toolHitPosition,currentSystem);
            }
            if (rightClick) {
                RightClickUpdate(mousePosition,toolHitPosition,currentSystem);
            }
        }

        private Vector2 AutoSelectTile(Vector2 mousePosition)
        {
            IAutoSelectTool autoSelectTool = (IAutoSelectTool)playerInventory.CurrentTool;
            TileMapLayer layer = autoSelectTool.GetAutoSelectLayer();
            if (layer != TileMapLayer.Base) return mousePosition;
            toolHitPosition = autoTileFinder.GetTilePosition(mousePosition,range);
            IWorldTileMap hitMap = autoTileFinder.GetHitTileMap();
            if (hitMap == null || !hitMap.GetTilemap())
            {
                tileBreakHighlighter.Clear();
                return toolHitPosition;
            }
            
            tileBreakHighlighter.SetOutlineColor(autoSelectTool.GetColor());
            OutlineTileMapCellData outlineTileMapCellData;
            Vector2Int cellPosition;
            if (hitMap is IOutlineTileGridMap outlineTileGridMap)
            {
                cellPosition = Global.GetCellPositionFromWorld(toolHitPosition);
                Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
                outlineTileMapCellData = outlineTileGridMap.GetOutlineCellData(vector3Int);
            }
            else
            {
                cellPosition = hitMap.GetHitTilePosition(toolHitPosition);
                outlineTileMapCellData = hitMap.FormatMainTileMapOutlineData(new Vector3Int(cellPosition.x, cellPosition.y, 0));
            }
                   
            tileBreakHighlighter.Display(cellPosition,outlineTileMapCellData);
            return toolHitPosition;
        }
        
        public void SetRange(float range)
        {
            this.range = range;
        }

        private void PreviewHighlight(Vector2 mousePosition)
        {
            previewController.Preview(playerInventory.CurrentTool,toolHitPosition);
            foreach (IWorldTileMap worldTileMap in systemTileMaps)
            {
                var result = MousePositionTileMapSearcher.GetNearestTileMapPosition(mousePosition, worldTileMap.GetTilemap(), 3);
                if (!result.HasValue)
                {
                    ToolTipController.Instance.HideToolTip(ToolTipType.World);
                    continue;
                }
                bool highlight = TryHighlight(currentSystem, (result.Value,worldTileMap));
                if (highlight) return;
            }
            tileHighlighter.Hide();
        }
        

        private bool TryHighlight(ClosedChunkSystem system, (Vector2, IWorldTileMap) result)
        {
            (Vector2 position, IWorldTileMap tilemap) = result;
            if (tilemap is not WorldTileMap worldTileGridMap) return false;
                
            Vector3Int cellPosition = tilemap.GetTilemap().WorldToCell(position);
            ITileEntityInstance tileEntityInstance = worldTileGridMap.GetTileEntityAtPosition((Vector2Int)cellPosition);
            if (Input.GetKey(KeyCode.LeftShift) || tileEntityInstance is not IWorldToolTipTileEntity textPreviewTileEntity || !DisplayTextPreviewToolTip(textPreviewTileEntity))
            {
                ToolTipController.Instance.HideToolTip(ToolTipType.World);
            }
            if (!CanRightClickTileEntity(tileEntityInstance, system)) return false;
            tileHighlighter.Highlight(position, tilemap.GetTilemap());
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
        private void MouseScrollUIUpdate(Vector2 mousePosition)
        {
            ItemSlotUIClickHandler clickHandler = PlayerKeyPress.GetPointerOverComponent<ItemSlotUIClickHandler>();
            clickHandler?.MiddleMouseScroll();
        }

        public static ILoadedChunk GetChunk(Vector2 mousePosition)
        {
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            if (!closedChunkSystem) {
                return null;
            }
            Vector2Int chunkPosition = Global.GetChunkFromWorld(mousePosition);
            return closedChunkSystem.GetChunk(chunkPosition);
        }

        private void ToolClickUpdate(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem, MouseButtonKey mouseButtonKey)
        {
            var clickHandler = toolClickHandlerCollection.GetOrAddTool(playerInventory.CurrentToolType,mouseButtonKey, playerInventory.CurrentTool);
            if (clickHandler == null)
            {
                return;
            }
            clickHandler.Tick(mousePosition, !closedChunkSystem.Interactable);
            playerRobot.FaceMousePosition(mousePosition);
            playerRobot.SetIsUsingTool(true);
            PlayerRobotLaserGunController gunController = playerRobot.gunController;
            gunController.AngleToPosition(mousePosition);
            gunController.OnClick(mouseButtonKey);
        }
        private void LeftClickUpdate(Vector2 mousePosition, Vector2 toolHitPosition, ClosedChunkSystem closedChunkSystem) {
            bool drop = HandleDrop(mousePosition);
            if (drop) {
                return;
            }
            ToolClickUpdate(toolHitPosition, closedChunkSystem, MouseButtonKey.Left);
            
        }
        private void RightClickUpdate(Vector2 mousePosition, Vector2 toolHitPosition, ClosedChunkSystem closedChunkSystem)
        {
            ItemObject currentPlayerItem = playerInventory.getSelectedItemSlot()?.itemObject;
            bool placable = currentPlayerItem is IPlacableItem;
            
            if (placable)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    ToolClickUpdate(toolHitPosition, closedChunkSystem, MouseButtonKey.Right);
                    return;
                }
                if (HandlePlace(mousePosition, DimensionManager.Instance.GetPlayerSystem())) return;
            }
            
            if (Input.GetMouseButtonDown(1)) {
                if (RightClickPort(mousePosition)) return;
                if (TryClickTileEntity(mousePosition)) return;
            }
            
            if (placable) return;
            ToolClickUpdate(toolHitPosition, closedChunkSystem, MouseButtonKey.Right);
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
            
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }
            
            IConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.GetManager(conduitType.Value);
            if (conduitSystemManager is not PortConduitSystemManagerManager portConduitSystemManager) return false;
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(mousePosition);
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
        public bool TryClickTileEntity(Vector2 mousePosition) {
            int layers = TileMapLayer.Base.ToRaycastLayers();
            GameObject tilemapObject = MouseUtils.RaycastObject(mousePosition,layers);
            if (ReferenceEquals(tilemapObject,null)) return false;
            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            Vector2Int mouseCellPosition = new Vector2Int(Mathf.FloorToInt(mousePosition.x*2), Mathf.FloorToInt(mousePosition.y*2));
            Vector2Int? tilePosition = FindTileAtLocation.find(mouseCellPosition,tilemap);
            if (tilePosition == null) {
                return false;
            }
            Vector2Int nonNullPosition = (Vector2Int) tilePosition;
            Vector2 worldPositionTile = new Vector2(nonNullPosition.x/2f,nonNullPosition.y/2f);
            ILoadedChunk chunk = GetChunk(worldPositionTile);
            if (chunk == null) {
                return false;
            }
            Vector2Int partitionPosition = Global.GetPartitionFromWorld(worldPositionTile);
            Vector2Int partitionPositionInChunk = partitionPosition -chunk.GetPosition()*Global.PARTITIONS_PER_CHUNK;
            Vector2Int tilePositionInPartition = nonNullPosition-partitionPosition*Global.CHUNK_PARTITION_SIZE;
            IChunkPartition chunkPartition = chunk.GetPartition(partitionPositionInChunk);
            ITileEntityInstance tileEntityInstance = chunkPartition.GetTileEntity(tilePositionInPartition);
            if (!CanRightClickTileEntity(tileEntityInstance, chunk.GetSystem())) return false;
            
            IRightClickableTileEntity rightClickableTileEntity = tileEntityInstance as IRightClickableTileEntity;
            
            // In cases where the tile entity has both ui and click behavior, holding left shit lets the player interact with the entity instance
            bool clickInstanceInterface = rightClickableTileEntity != null && Input.GetKey(KeyCode.LeftShift);
            if (rightClickableTileEntity is IStopPlayerRightClickableTileEntity || tileEntityInstance.GetTileEntity() is IUITileEntity)
            {
                StopPlayerHorizontalMovement();
            }
            if (!clickInstanceInterface && tileEntityInstance.GetTileEntity() is IUITileEntity)
            {
                
                TileEntityAssetRegistry.Instance.DisplayUI(tileEntityInstance);
                
                return true;
            }
            

            rightClickableTileEntity?.OnRightClick();
            return rightClickableTileEntity != null;

            
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
            if (!closedChunkSystem || !closedChunkSystem.Interactable) {
                return false;
            }

            ItemSlot selectedSlot = playerInventory.getSelectedItemSlot();
            if (ItemSlotUtils.IsItemSlotNull(selectedSlot)) return false;
            
            bool placed = TilePlaceUtils.PlaceFromWorldPosition(playerScript,selectedSlot,mousePosition,closedChunkSystem);
            if (placed) {
                playerScript.PlaceUpdate();
            }
            return placed;
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
        
        

        private void InventoryControlUpdate()
        {
            if (canvasController.BlockKeyInput) return;
            if (Input.mouseScrollDelta.y != 0) {
                float y = Input.mouseScrollDelta.y;
                if (y < 0) {
                    playerInventory.IterateSelectedTile(1);
                } else {
                    playerInventory.IterateSelectedTile(-1);
                }
            }
        }

        public void UpdateOnToolChange()
        {
            autoSelectableTool = playerInventory.CurrentTool is IAutoSelectTool;
            IndicatorManager indicatorManager = playerScript.PlayerUIContainer.IndicatorManager;
            if (autoSelectableTool)
            {
                indicatorManager.AddViewBundle(IndicatorDisplayBundle.AutoSelect);
            }
            else
            {
                indicatorManager.RemoveBundle(IndicatorDisplayBundle.AutoSelect);
            }
            ClearToolPreview();
        }

        public void ClearToolPreview()
        {
            tileBreakHighlighter.ResetHistory();
            tileBreakHighlighter.Clear();
            playerScript.TileViewers.TileBreakHighlighter.ResetHistory();
            playerScript.TileViewers.TileBreakHighlighter.Clear();
            previewController.ResetRecord();
        }
    }

    internal class MouseTileHighlighter
    {
        
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
        
        public void Preview(IRobotToolInstance robotToolInstance, Vector2 mousePosition)
        {
            if (robotToolInstance == null) return;
       
            if (mousePosition == lastMousePosition) return;
            Vector2Int tilePosition = Global.GetCellPositionFromWorld(mousePosition);
            if (lastTilePosition == tilePosition) return;
            
            lastMousePosition = mousePosition;
            lastTilePosition = tilePosition;
            robotToolInstance.Preview(tilePosition);
        }
        

        public void ResetRecord()
        {
            lastMousePosition = Vector2.negativeInfinity;
            lastTilePosition = Vector2Int.one * int.MaxValue;
        }
    }
}

