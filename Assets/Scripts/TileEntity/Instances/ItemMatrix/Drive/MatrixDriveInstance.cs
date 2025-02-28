using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Conduits.Ports;
using Conduits.Systems;
using Items.Tags;
using Items.Tags.Matrix;
using Items.Inventory;
using Entities;
using Item.Slot;
using UI;

namespace TileEntity.Instances.Matrix {
    public class MatrixDriveInstance : TileEntityInstance<MatrixDrive>, IMatrixConduitInteractable, ISerializableTileEntity, IRightClickableTileEntity, 
        ILoadableTileEntity, ITickableTileEntity, IBreakActionTileEntity, IInventoryListener
    {
        public MatrixDriveInstance(MatrixDrive tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private List<ItemSlot> storageDrives;
        private MatrixDriverPixelContainer pixelContainer;
        private MatrixConduitSystem matrixConduitSystem;
        private ItemMatrixControllerInstance controller;

        public List<ItemSlot> StorageDrives { get => storageDrives; }

        public ConduitPortLayout getConduitPortLayout()
        {
            return TileEntityObject.Layout;
        }

        public void InventoryUpdate(int n)
        {
            matrixConduitSystem.setDrive(this);
        }

        public void Load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            MatrixDriverPixelContainer pixelContainer = GameObject.Instantiate(TileEntityObject.getAssetManager().getElement<MatrixDriverPixelContainer>("Pixels"));
            pixelContainer.name = "DrivePixels" + GetPositionInChunk();
            pixelContainer.transform.position = GetWorldPosition();
            pixelContainer.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            if (storageDrives == null) {
                storageDrives = new List<ItemSlot>();
                for (int i = 0; i < TileEntityObject.rows*TileEntityObject.columns; i++) {
                    storageDrives.Add(null);
                }
            }
            loadPixels();
        }

        public void loadPixels() {
            for (int i = 0; i < Mathf.Min(pixelContainer.orderedPixels.Count,storageDrives.Count); i++) {
                ItemSlot itemSlot = storageDrives[i];
                SpriteRenderer spriteRenderer = pixelContainer.orderedPixels[i];
                if (controller == null) {
                    spriteRenderer.gameObject.SetActive(true);
                    spriteRenderer.color = Color.black;
                    continue;
                }
                if (itemSlot == null || itemSlot.itemObject == null) {
                    spriteRenderer.gameObject.SetActive(false);
                    continue;
                }
                if (
                    itemSlot.itemObject is not MatrixDriveItem matrixDriveItem || 
                    itemSlot.tags == null || 
                    !itemSlot.tags.Dict.ContainsKey(ItemTag.StorageDrive)
                ) {
                    continue;
                }
                spriteRenderer.gameObject.SetActive(true);
                bool allAssigned = true;
                List<ItemSlot> driveInventory = (List<ItemSlot>)itemSlot.tags.Dict[ItemTag.StorageDrive];
                foreach (ItemSlot item in driveInventory) {
                    if (item == null || item.itemObject == null) {
                        allAssigned = false;
                        break;
                    }
                }
                if (!allAssigned) {
                    spriteRenderer.color = Color.green;
                    continue;
                }
                bool full = true;
                foreach (ItemSlot item in driveInventory) {
                    if (item.amount != matrixDriveItem.MaxAmount) {
                        full = false;
                        break;
                    }
                }
                if (!full) {
                    spriteRenderer.color = new Color(1f,165f/255f,0f); // Orange
                } else {
                    spriteRenderer.color = Color.red;
                }
            }
        }
        public Queue<ItemSlot> getQueueOfDrives() {
            Queue<ItemSlot> queue = new Queue<ItemSlot>();
            foreach (ItemSlot itemSlot in storageDrives) {
                if (
                    itemSlot == null || 
                    itemSlot.itemObject == null || 
                    itemSlot.tags == null || 
                    itemSlot.tags.Dict == null || 
                    !itemSlot.tags.Dict.ContainsKey(ItemTag.StorageDrive) ||
                    itemSlot.itemObject is not MatrixDriveItem matrixDriveItem
                    ) {
                    continue;
                }
                queue.Enqueue(itemSlot);
            }
            return queue;
        }
        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            foreach (ItemSlot itemSlot in storageDrives) {
                ItemEntityFactory.SpawnItemEntity(GetWorldPosition(),itemSlot,loadedChunk.getEntityContainer());
            }
        }

        public void OnRightClick()
        {
            
            MatrixDriveUI ui = MatrixDriveUI.createInstance();
            ui.init(TileEntityObject.rows,TileEntityObject.columns,storageDrives,this);
            MainCanvasController.Instance.DisplayObject(ui.gameObject);
        }

        public string Serialize()
        {
            return ItemSlotFactory.serializeList(storageDrives);
        }

        public void SyncToController(ItemMatrixControllerInstance matrixController)
        {
            this.controller = matrixController;
        }

        public void SyncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            this.matrixConduitSystem = matrixConduitSystem;
            matrixConduitSystem.setDrive(this);
        }

        public void TickUpdate()
        {
            if (pixelContainer == null) {
                return;
            }
            loadPixels();
        }

        public void Unload()
        {
            if (pixelContainer != null) {
                GameObject.Destroy(pixelContainer.gameObject);
            }
        }

        public void Unserialize(string data)
        {
            storageDrives = ItemSlotFactory.Deserialize(data);
        }

        public void RemoveFromSystem()
        {
            if (matrixConduitSystem != null) {
                matrixConduitSystem.removeDrive(this);
            }
        }
    }
}

