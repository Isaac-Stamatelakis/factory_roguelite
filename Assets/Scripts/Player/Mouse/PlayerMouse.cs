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
using Item.Slot;
using Player;
using Player.Mouse;
using Player.Tool;
using PlayerModule.IO;
using PlayerModule.KeyPress;
using Robot.Tool;
using Tiles.Highlight;
using UI;
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
        [SerializeField] private TileHighlighter tileHighlighter;
        [SerializeField] private LineRenderer t1;
        [SerializeField] private LineRenderer t2;
        
        void Start()
        {
            mainCamera = Camera.main;
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerScript = GetComponent<PlayerScript>();
            playerTransform = transform;
            eventSystem = EventSystem.current;
        }
        
        void Update()
        {   
            InventoryControlUpdate();
            
            bool leftClick = Input.GetMouseButton(0);
            bool rightClick = Input.GetMouseButton(1);
            bool scroll = Input.mouseScrollDelta.y != 0;
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (PlayerKeyPressUtils.BlockKeyInput)
            {
                if (eventSystem.IsPointerOverGameObject())
                {

                    if (scroll)
                    {
                        MouseScrollUIUpdate(mousePosition);
                    }
                }
                return;
            }

            if (eventSystem.IsPointerOverGameObject()) return;
            
            
            if (!leftClick)
            {
                toolClickHandlerCollection.Terminate(MouseButtonKey.Left);
            }
            
            if (!rightClick)
            {
                toolClickHandlerCollection.Terminate(MouseButtonKey.Right);
            }
            
            
            if (!leftClick && !rightClick) return;
            
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            if (!closedChunkSystem) {
                return;
            }
            
            if (leftClick) {
                LeftClickUpdate(mousePosition);
            }
            if (rightClick) {
                RightClickUpdate(mousePosition);
            }
            
        }

        public void FixedUpdate()
        {
            if (PlayerKeyPressUtils.BlockKeyInput) return;
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            if (!closedChunkSystem) {
                return;
            }
            List<IWorldTileMap> tilemaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Object),
                closedChunkSystem.GetTileMap(TileMapType.Block),
            };
            
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var result = MousePositionTileMapSearcher.FindTileNearestMousePosition(mousePosition, tilemaps, 3);
            if (result != null)
            {
                bool highlight = TryHighlight(result.Value);
                if (highlight) return;
            }
            tileHighlighter.Hide();
        }

        private bool TryHighlight((Vector2, IWorldTileMap) result)
        {
            (Vector2 position, IWorldTileMap tilemap) = result;
            if (tilemap is not WorldTileGridMap worldTileGridMap) return false;
                
            Vector3Int cellPosition = tilemap.GetTilemap().WorldToCell(position);
            ITileEntityInstance tileEntityInstance = worldTileGridMap.getTileEntityAtPosition((Vector2Int)cellPosition);

            if (tileEntityInstance is not (ILeftClickableTileEntity or IRightClickableTileEntity)) return false;
            if (tileEntityInstance is IConditionalRightClickableTileEntity conditionalRightClickableTileEntity)
            {
                if (!conditionalRightClickableTileEntity.CanRightClick())
                {
                    return false;
                }
            }
            tileHighlighter.Highlight(position, tilemap.GetTilemap());
            return true;
        }

        private void MouseScrollUpdate(Vector2 mousePosition)
        {
            if (eventSystem.IsPointerOverGameObject())
            {
                MouseScrollUIUpdate(mousePosition);
            }
            // More after?
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
            Vector2Int chunkPosition = Global.getChunkFromWorld(mousePosition);
            return closedChunkSystem.getChunk(chunkPosition);
        }

        private void LeftClickUpdate(Vector2 mousePosition) {
            bool drop = HandleDrop(mousePosition);
            if (drop) {
                return;
            }

            var leftClickHandler = toolClickHandlerCollection.GetOrAddTool(playerInventory.CurrentToolType, MouseButtonKey.Left, playerInventory.CurrentTool);
            leftClickHandler.Tick(mousePosition);

        }
        private void RightClickUpdate(Vector2 mousePosition)
        {
            InventoryDisplayMode inventoryDisplayMode = playerInventory.Mode;
            switch (inventoryDisplayMode)
            {
                case InventoryDisplayMode.Inventory:
                    if (HandlePlace(mousePosition, DimensionManager.Instance.GetPlayerSystem())) return;
                    break;
                case InventoryDisplayMode.Tools:
                    IRobotToolInstance currentTool = playerInventory.CurrentTool;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (Input.GetMouseButtonDown(1)) {
                if (RightClickPort(mousePosition)) return;
                if (TryClickTileEntity(mousePosition)) return;
            }
            
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
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }

            IConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.GetManager(conduitType.Value);
            if (conduitSystemManager is not PortConduitSystemManager portConduitSystemManager) return false;
            Vector2Int cellPosition = Global.getCellPositionFromWorld(mousePosition);
            IPortConduit conduit = portConduitSystemManager.GetConduitWithPort(cellPosition);
            if (conduit == null) {
                return false;
            }
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
        public static bool TryClickTileEntity(Vector2 mousePosition) {
            int layers = TileMapLayer.Base.toRaycastLayers();
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
            Vector2Int partitionPosition = Global.getPartitionFromWorld(worldPositionTile);
            Vector2Int partitionPositionInChunk = partitionPosition -chunk.GetPosition()*Global.PARTITIONS_PER_CHUNK;
            Vector2Int tilePositionInPartition = nonNullPosition-partitionPosition*Global.CHUNK_PARTITION_SIZE;
            IChunkPartition chunkPartition = chunk.GetPartition(partitionPositionInChunk);
            if (chunkPartition.ClickTileEntity(tilePositionInPartition)) {
                return true;
            }
            return false;
        }

        private bool HandlePlace(Vector2 mousePosition, ClosedChunkSystem closedChunkSystem) {
            if (ReferenceEquals(closedChunkSystem,null)) {
                return false;
            }

            ItemSlot selectedSlot = playerInventory.getSelectedItemSlot();
            if (ItemSlotUtils.IsItemSlotNull(selectedSlot)) return false;
            
            bool placed = PlaceTile.PlaceFromWorldPosition(playerScript,selectedSlot,mousePosition,closedChunkSystem);
            if (placed && !DevMode.Instance.noPlaceCost) {
                playerInventory.deiterateInventoryAmount();
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
            if (grabbedItemProperties.ItemSlot == null) {
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
                chunk.getEntityContainer(),
                CalculateItemVelocity(mousePosition)
            );
            grabbedItemProperties.SetItemSlot(null);
            return true;
        }
        
        

        private void InventoryControlUpdate()
        {
            if (PlayerKeyPressUtils.BlockKeyInput) return;
            if (Input.mouseScrollDelta.y != 0) {
                float y = Input.mouseScrollDelta.y;
                if (y < 0) {
                    playerInventory.IterateSelectedTile(1);
                } else {
                    playerInventory.IterateSelectedTile(-1);
                }
            }
        }
    }
}

