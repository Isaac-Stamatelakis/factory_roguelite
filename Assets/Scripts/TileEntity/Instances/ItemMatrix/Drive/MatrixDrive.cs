using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using GUIModule;
using ChunkModule;
using ConduitModule.Systems;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Drive", menuName = "Tile Entity/Item Matrix/Drive")]
    public class MatrixDrive : TileEntity, 
        IMatrixConduitInteractable, ISerializableTileEntity, IRightClickableTileEntity, 
        ILoadableTileEntity, ITickableTileEntity, IBreakActionTileEntity, IInventoryListener
    {
        [SerializeField] private ConduitPortLayout layout;
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        [Header("Position for active/inactive pixels\nOrdered BottomLeft to TopRight")]
        [SerializeField] private GameObject visualPrefab;
        private List<ItemSlot> storageDrives;
        private MatrixDriverPixelContainer pixelContainer;
        private MatrixConduitSystem matrixConduitSystem;

        public List<ItemSlot> StorageDrives { get => storageDrives; }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void inventoryUpdate()
        {
            matrixConduitSystem.setDrive(this);
        }

        public void load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            GameObject container = GlobalHelper.instantiateFromResourcePath("Prefabs/TileEntities/Matrix/DrivePixels");
            container.name = "DrivePixels" + getPositionInChunk();
            container.transform.position = getWorldPosition();
            container.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            pixelContainer = container.GetComponent<MatrixDriverPixelContainer>();
            if (storageDrives == null) {
                storageDrives = new List<ItemSlot>();
                for (int i = 0; i < rows*columns; i++) {
                    storageDrives.Add(null);
                }
            }
            loadPixels();
        }

        public void loadPixels() {
            for (int i = 0; i < Mathf.Min(pixelContainer.orderedPixels.Count,storageDrives.Count); i++) {
                ItemSlot itemSlot = storageDrives[i];
                SpriteRenderer spriteRenderer = pixelContainer.orderedPixels[i];
                if (itemSlot == null || itemSlot.itemObject == null) {
                    spriteRenderer.gameObject.SetActive(false);
                    continue;
                }
                spriteRenderer.gameObject.SetActive(true);
            }
        }

        public void onBreak()
        {
            matrixConduitSystem.removeDrive(this);
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            foreach (ItemSlot itemSlot in storageDrives) {
                ItemEntityHelper.spawnItemEntity(getWorldPosition(),itemSlot,loadedChunk.getEntityContainer());
            }
        }

        public void onRightClick()
        {
            
            MatrixDriveUI ui = MatrixDriveUI.createInstance();
            ui.init(rows,columns,storageDrives,this);
            GlobalUIContainer.getInstance().getUiController().setGUI(ui.gameObject);
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(storageDrives);
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            this.matrixConduitSystem = matrixConduitSystem;
            matrixConduitSystem.setDrive(this);
        }

        public void tickUpdate()
        {
            if (pixelContainer == null) {
                return;
            }
            loadPixels();
        }

        public void unload()
        {
            if (pixelContainer != null) {
                GameObject.Destroy(pixelContainer.gameObject);
            }
        }

        public void unserialize(string data)
        {
            storageDrives = ItemSlotFactory.deserialize(data);
        }
    }
}

