using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using GUIModule;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Drive", menuName = "Tile Entity/Item Matrix/Drive")]
    public class MatrixDrive : TileEntity, IMatrixConduitInteractable, ISerializableTileEntity, IRightClickableTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        private List<ItemSlot> storageDrives;
        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void onRightClick()
        {
            if (storageDrives == null) {
                storageDrives = new List<ItemSlot>();
                for (int i = 0; i < rows*columns; i++) {
                    storageDrives.Add(null);
                }
            }
            MatrixDriveUI ui = MatrixDriveUI.createInstance();
            ui.init(rows,columns,storageDrives);
            GlobalUIContainer.getInstance().getUiController().setGUI(ui.gameObject);
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(storageDrives);
        }

        public void unserialize(string data)
        {
            storageDrives = ItemSlotFactory.deserialize(data);
        }
    }
}

