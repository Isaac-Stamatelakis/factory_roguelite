using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Conduits.Systems;
using UnityEngine.Tilemaps;
using Chunks;
using Items.Tags;
using Items.Tags.Matrix;
using Items;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Controller")]
    public class ItemMatrixController : TileEntity
    {
        public ConduitPortLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ItemMatrixControllerInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

