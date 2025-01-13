using System;
using System.Collections.Generic;
using Item.ItemObjects.Instances.Tile.Chisel;
using Newtonsoft.Json;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using TileMaps.Layer;
using UnityEngine;


namespace Robot.Tool.Instances
{
    public class Buildinator : RobotToolInstance<BuildinatorData, BuildinatorObject>
    {
        public Buildinator(BuildinatorData toolData, BuildinatorObject robotObject) : base(toolData, robotObject)
        {
         
        }

        public override Sprite GetSprite()
        {
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    return robotObject.ChiselSprite;
                case BuildinatorMode.Rotator:
                    return robotObject.RotatorSprite;
                case BuildinatorMode.Hammer:
                    return robotObject.HammerSprite;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void BeginClickHold(Vector2 mousePosition)
        {
            
            
        }

        public override void TerminateClickHold()
        {
            
            
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    if (!Input.GetMouseButtonDown((int)mouseButtonKey)) return;
                    int direction = mouseButtonKey == MouseButtonKey.Left ? -1 : 1;
                    ChiselItemUtils.TryIterateChiselItem(mousePosition, direction);
                    break;
                case BuildinatorMode.Rotator:
                    break;
                case BuildinatorMode.Hammer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            if (time < 0.125f) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            int direction = moveDirection == MoveDirection.Left ? -1 : 1;
            toolData.Mode = GlobalHelper.ShiftEnum(direction, toolData.Mode);
        }
    }

    public enum BuildinatorMode
    {
        Chisel,
        Rotator,
        Hammer
    }

    public class BuildinatorData : RobotToolData
    {
        public BuildinatorMode Mode;
      
        public BuildinatorData(BuildinatorMode mode)
        {
            Mode = mode;
        }
    }
}
