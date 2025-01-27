using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Terminal")]
    public class MatrixTerminal : TileEntityObject
    {
        public ConduitPortLayout Layout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixTerminalInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

