using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Core", menuName = "Tile Entity/Item Matrix/Crafting/Core")]
    public class MatrixAutoCraftCore : MatrixAutoCraftingChassis
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixAutoCraftingCoreInstance(this,tilePosition,tileItem,chunk);
        }
    }
}
