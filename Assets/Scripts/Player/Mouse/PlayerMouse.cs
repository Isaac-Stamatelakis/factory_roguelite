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
using Dimensions;
using Items;
using TileEntity;
using Entities;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Player.Mouse;
using Player.Tool;
using PlayerModule.IO;
using PlayerModule.KeyPress;
using Robot.Tool;
using UI;
using UI.ToolTip;

namespace PlayerModule.Mouse {

    public interface IPlayerClickHandler
    {
        public void BeginClickHold();
        public void TerminateClickHold();
        public void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time);
    }
    public class HoldClickHandler
    {
        private float counter = 0f;
        private readonly IPlayerClickHandler clickHandler;
        private readonly int mouseIndex;
        private MouseButtonKey mouseButtonKey;
        public HoldClickHandler(IPlayerClickHandler clickHandler, MouseButtonKey mouseButtonKey)
        {
            this.clickHandler = clickHandler;
            this.mouseIndex = (int)mouseButtonKey;
            clickHandler.BeginClickHold();
        }

        public void Tick(Vector2 mousePosition)
        {
            if (Input.GetMouseButtonDown(mouseIndex)) {
                clickHandler.ClickUpdate(mousePosition,mouseButtonKey);
                return;
            }
            
            if (DevMode.Instance.noBreakCooldown)
            {
                clickHandler.HoldClickUpdate(mousePosition, mouseButtonKey, int.MaxValue);
                return;
            }
            counter += Time.deltaTime;
            if (clickHandler.HoldClickUpdate(mousePosition,mouseButtonKey, counter))
            {
                counter = 0f;
            }
        }

        public void Terminate()
        {
            clickHandler.TerminateClickHold();
        }
    }
    /// <summary>
    /// Handles all player mouse interactions
    /// </summary>
    public class PlayerMouse : MonoBehaviour
    {
        private HoldClickHandler leftClickHandler;
        private HoldClickHandler rightClickHandler;
        private PlayerInventory playerInventory;
        private Transform playerTransform;
        private Camera mainCamera;
        private EventSystem eventSystem;
        
        void Start()
        {
            mainCamera = Camera.main;
            playerInventory = GetComponent<PlayerInventory>();
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
            
            if (!leftClick && leftClickHandler != null)
            {
                leftClickHandler.Terminate();
                leftClickHandler = null;
            }
            
            if (!rightClick && rightClickHandler != null)
            {
                rightClickHandler = null;
            }
            
            if (eventSystem.IsPointerOverGameObject())
            {
                
                if (scroll)
                {
                    MouseScrollUIUpdate(mousePosition);
                }
                return;
            }
            ToolTipController.Instance.HideToolTip();
            if (!leftClick && !rightClick) return;
            
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (!closedChunkSystem) {
                return;
            }
            Vector2 systemOffset = new Vector2(closedChunkSystem.DimPositionOffset.x/2f,closedChunkSystem.DimPositionOffset.y/2f);
            
            if (leftClick) {
                LeftClickUpdate(mousePosition,systemOffset);
            }
            if (rightClick) {
                RightClickUpdate(mousePosition,systemOffset);
            }
            
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
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (!closedChunkSystem) {
                return null;
            }
            Vector2Int chunkPosition = Global.getChunkFromWorld(mousePosition);
            return closedChunkSystem.getChunk(chunkPosition);
        }

        private void LeftClickUpdate(Vector2 mousePosition, Vector2 offset) {
            if (DevMode.Instance.spawnItem) {
                ILoadedChunk chunk = GetChunk(mousePosition+offset);
                if (chunk != null) {
                        ItemEntityHelper.spawnItemEntity(
                        mousePosition+offset,
                        ItemSlotFactory.CreateNewItemSlot(
                            ItemRegistry.GetInstance().GetItemObject(DevMode.Instance.spawnItemID),
                            1
                        ),
                        chunk.getEntityContainer()
                    );
                }
                return;
            }
            bool drop = HandleDrop(mousePosition,offset);
            if (drop) {
                return;
            }
            if (leftClickHandler == null)
            {
                RobotTool currentTool = playerInventory.CurrentTool;
                leftClickHandler = new HoldClickHandler(currentTool,MouseButtonKey.Left);
            }
            leftClickHandler.Tick(mousePosition);

        }
        private void RightClickUpdate(Vector2 mousePosition,Vector2 offset)
        {
            
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(playerInventory.getSelectedId());
            if (Input.GetMouseButtonDown(1)) {
                bool somethingClicked = false;
                if (itemObject is ConduitItem) {
                    somethingClicked = RightClickPort(mousePosition+offset);
                } else {
                    somethingClicked = TryClickTileEntity(mousePosition,offset);
                }
                if (somethingClicked) return;
            }
            InventoryDisplayMode inventoryDisplayMode = playerInventory.Mode;
            switch (inventoryDisplayMode)
            {
                case InventoryDisplayMode.Inventory:
                    handlePlace(mousePosition,offset,DimensionManager.Instance.getPlayerSystem(playerTransform));
                    break;
                case InventoryDisplayMode.Tools:
                    RobotTool currentTool = playerInventory.CurrentTool;
                    /*
                    switch (expression)
                    {
                        
                    }
                    */
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool RightClickPort(Vector2 mousePosition) {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(playerTransform);
            if (closedChunkSystem is not ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                return false;
            }
            
            string id = playerInventory.getSelectedId();
            ConduitItem conduitItem = ItemRegistry.GetInstance().GetConduitItem(id);
            if (ReferenceEquals(conduitItem,null)) {
                return false;
            }
            ConduitType conduitType = conduitItem.GetConduitType();
            IConduitSystemManager conduitSystemManager = conduitTileClosedChunkSystem.GetManager(conduitType);
            switch (conduitSystemManager)
            {
                case null:
                    Debug.LogError("Attempted to click port of null conduit system manager");
                    return false;
                case PortConduitSystemManager portConduitSystemManager:
                {
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
                default:
                    return false;
            }
        }
        /*
        private void breakMouseHover(Vector2 mousePosition) {
            if (Input.GetMouseButtonDown(0)) {
                isHoldingMouse = true;
            }

            if (Input.GetMouseButton(0) && isHoldingMouse) {
                if (DevMode.Instance.noBreakCooldown) {
                    HitTileLayer(DevMode.Instance.breakType, mousePosition);
                    return;
                }
                timeSinceLastUpdate += Time.deltaTime;
                if (timeSinceLastUpdate >= updateInterval) {
                    timeSinceLastUpdate = 0f;
                    HitTileLayer(DevMode.Instance.breakType, mousePosition);
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                isHoldingMouse = false;
            }
        }
        */

        
        

        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static bool TryClickTileEntity(Vector2 mousePosition,Vector2 offset) {
            int layers = TileMapLayer.Base.toRaycastLayers();
            GameObject tilemapObject = MouseUtils.RaycastObject(mousePosition,layers);
            if (ReferenceEquals(tilemapObject,null)) return false;
            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            Vector2Int mouseCellPosition = new Vector2Int(Mathf.FloorToInt((mousePosition.x+offset.x)*2), Mathf.FloorToInt((mousePosition.y+offset.y)*2));
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
            Vector2Int partitionPositionInChunk = partitionPosition -chunk.getPosition()*Global.PartitionsPerChunk;
            Vector2Int tilePositionInPartition = nonNullPosition-partitionPosition*Global.ChunkPartitionSize;
            IChunkPartition chunkPartition = chunk.getPartition(partitionPositionInChunk);
            if (chunkPartition.clickTileEntity(tilePositionInPartition)) {
                return true;
            }
            return false;
        }

        private bool handlePlace(Vector2 mousePosition, Vector2 offset, ClosedChunkSystem closedChunkSystem) {
            if (ReferenceEquals(closedChunkSystem,null)) {
                return false;
            }
            string id;
            if (DevMode.Instance.placeSelectedID) {
                id = DevMode.Instance.placeID;
            } else {
                id = playerInventory.getSelectedId();
            }
            if (id == null) {
                return false;
            }
        
            bool placed = false;
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            placed = PlaceTile.PlaceFromWorldPosition(itemObject,mousePosition,closedChunkSystem);
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
        
        private bool HandleDrop(Vector2 mousePosition,Vector2 offset) {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return false;
            }
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            if (grabbedItemProperties.ItemSlot == null) {
                return false;
            }
            ILoadedChunk chunk = GetChunk(mousePosition+offset);
            if (chunk == null) {
                return false;
            }
            Vector2 spriteCenter = GetComponent<SpriteRenderer>().sprite.bounds.center.normalized;
            if (ItemSlotUtils.IsItemSlotNull(grabbedItemProperties.ItemSlot)) return false;
            ItemEntityHelper.spawnItemEntityWithVelocity(
                new Vector2(transform.position.x,transform.position.y) + spriteCenter,
                grabbedItemProperties.ItemSlot,
                chunk.getEntityContainer(),
                CalculateItemVelocity(mousePosition)
            );
            grabbedItemProperties.SetItemSlot(null);
            return true;
        }
        
        

        private void InventoryControlUpdate() {
            if (Input.mouseScrollDelta.y != 0) {
                float y = Input.mouseScrollDelta.y;
                if (y < 0) {
                    playerInventory.iterateSelectedTile(1);
                } else {
                    playerInventory.iterateSelectedTile(-1);
                }
            }
        }
    }
}

