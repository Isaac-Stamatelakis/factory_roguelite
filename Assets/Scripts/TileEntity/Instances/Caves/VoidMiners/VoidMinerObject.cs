using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Void Miner", menuName = "Tile Entity/Cave/Miner")]
    public class VoidMinerObject : TileEntityObject, IManagedUITileEntity
    {
        public TileEntityUIManager uIManager;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new VoidMinerInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return uIManager;
        }
    }
}

