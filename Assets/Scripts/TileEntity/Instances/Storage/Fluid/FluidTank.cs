using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;


namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Fluid Tank",menuName="Tile Entity/Storage/Fluid/Standard")]
    public class FluidTank : TileEntity
    {
        public Tier Tier;
        public ConduitPortLayout ConduitLayout;
        public TileEntityUIManager UIManager;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new FluidTankInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

