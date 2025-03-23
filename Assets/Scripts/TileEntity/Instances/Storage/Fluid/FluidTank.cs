using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;


namespace TileEntity.Instances.Storage {
    [CreateAssetMenu(fileName ="New Fluid Tank",menuName="Tile Entity/Storage/Fluid/Standard")]
    public class FluidTank : TileEntityObject
    {
        public Tier Tier;
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new FluidTankInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

