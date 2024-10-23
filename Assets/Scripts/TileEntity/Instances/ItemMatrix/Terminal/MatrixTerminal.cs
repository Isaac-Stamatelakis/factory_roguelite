using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Terminal")]
    public class MatrixTerminal : TileEntity
    {
        public ConduitPortLayout Layout;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixTerminalInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

