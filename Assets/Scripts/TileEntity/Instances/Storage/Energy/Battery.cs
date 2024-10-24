using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName = "E~New Battery", menuName = "Tile Entity/Storage/Battery")]
    public class Battery : TileEntity
    {
        public int Storage;
        public ConduitPortLayout ConduitPortLayout;
        public TileEntityUIManager UIManager;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new BatteryInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

