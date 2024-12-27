using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using Items.Tags;
using Chunks;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Interface")]
    public class MatrixInterface : TileEntityObject
    {
        public ConduitPortLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixInterfaceInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

