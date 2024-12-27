using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Chassis", menuName = "Tile Entity/Item Matrix/Crafting/Chassis")]
    public class MatrixAutoCraftingChassis : TileEntityObject
    {
        public ConduitPortLayout Layout;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixAutoCraftingChassisInstance<MatrixAutoCraftingChassis>(this,tilePosition,tileItem,chunk);
        }
    }
}

