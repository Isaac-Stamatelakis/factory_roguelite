using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Chunks;
using Conduits.Systems;
using Items.Tags;
using Items.Tags.Matrix;
using Items.Inventory;
using Entities;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Drive", menuName = "Tile Entity/Item Matrix/Drive")]
    public class MatrixDrive : TileEntity
    {
        public ConduitPortLayout Layout;
        public int rows;
        public int columns;
        [Header("Position for active/inactive pixels\nOrdered BottomLeft to TopRight")]
        public GameObject VisualPrefab;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixDriveInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

