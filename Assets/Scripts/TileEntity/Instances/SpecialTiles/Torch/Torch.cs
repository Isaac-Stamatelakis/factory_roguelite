using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Torch", menuName = "Tile Entity/Torch")]
    public class Torch : TileEntity
    {
        public float intensity = 1;
        public int radius = 10;
        public float falloff = 0.7f;
        public Color color = Color.white;
        public Vector2 positionInTile;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new TorchInstance(this,tilePosition,tileItem,chunk);
        }
    }

}