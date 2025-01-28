using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Recipe Encoder")]
    public class MatrixRecipeEncoder : TileEntityObject
    {
        public ConduitPortLayout Layout;
        public TileEntityUIManager UIManager;
        public int RecipeOutputCount;
        public int RecipeInputCount;
        public int BlankRecipeCount;
        public int EncodedRecipeCount;

        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixRecipeEncoderInstance(this,tilePosition,tileItem,chunk);
        }
    }

}
