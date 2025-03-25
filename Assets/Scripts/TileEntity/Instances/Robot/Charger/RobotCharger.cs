using System;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Robot.Charger
{
    public class RobotCharger : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new RobotChargerInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class RobotChargerInstance : TileEntityInstance<RobotCharger>
    {
        public RobotChargerInstance(RobotCharger tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }
        
    }
}
