using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using TileMaps.Layer;
using UnityEngine;

namespace Robot.Tool
{
    public enum MouseButtonKey
    {
        Left = 0,
        Right = 1
    }

    public enum MoveDirection
    {
        Left,
        Right
    }
    
    public interface IRobotToolInstance : IPlayerClickHandler
    {
        public Sprite GetSprite();
        public Sprite GetPrimaryModeSprite();
        public void ModeSwitch(MoveDirection moveDirection, bool subMode);
        public string GetName();
        public string GetModeName();
        public RobotToolObject GetToolObject();

    }
    
    public interface ISubModeRobotToolInstance {
        public string GetSubModeName();
        public Sprite GetSubModeSprite();
    }
    public abstract class RobotToolInstance<TData, TObject> : IRobotToolInstance 
        where TData : RobotToolData 
        where TObject : RobotToolObject
    {
        protected RobotToolInstance(TData toolData, TObject robotObject)
        {
            this.toolData = toolData;
            this.robotObject = robotObject;
        }
        protected TData toolData;
        protected TObject robotObject;

        public Sprite GetSprite()
        {
            return robotObject?.ToolIconItem?.getSprite();
        }
        public abstract Sprite GetPrimaryModeSprite();

        public abstract void BeginClickHold(Vector2 mousePosition);
        public abstract void TerminateClickHold();
        public abstract void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public abstract bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time);
        public abstract void ModeSwitch(MoveDirection moveDirection, bool subMode);
        public string GetName()
        {
            return robotObject?.name;
        }

        public abstract string GetModeName();
        public RobotToolObject GetToolObject()
        {
            return robotObject;
        }
    }

    public abstract class RobotToolData
    {
        
    }

    
    /*
    public class LaserGun : RobotTool
    {
        public float FireRate;
        public float Damage;
        public float EnergyCost;

        public LaserGun(float fireRate, float damage, float energyCost)
        {
            FireRate = fireRate;
            Damage = damage;
            EnergyCost = energyCost;
        }

        public override Sprite GetSprite()
        {
            return null;
        }

        public override void BeginClickHold()
        {
            throw new NotImplementedException();
        }

        public override void TerminateClickHold()
        {
            throw new NotImplementedException();
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            throw new NotImplementedException();
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            throw new NotImplementedException();
        }
    }

    public enum ConduitBreakMode
    {
        Standard,
        Disconnect
    }
    public class ConduitSlicer : RobotTool
    {
        public TileMapLayer Layer;
        public ConduitBreakMode BreakMode;

        public ConduitSlicer(TileMapLayer layer, ConduitBreakMode breakMode)
        {
            Layer = layer;
            BreakMode = breakMode;
        }

        public override Sprite GetSprite()
        {
            return null;
        }

        public override void BeginClickHold()
        {
            throw new NotImplementedException();
        }

        public override void TerminateClickHold()
        {
            throw new NotImplementedException();
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            throw new NotImplementedException();
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            throw new NotImplementedException();
        }
    }
    public enum BuildinatorMode
    {
        Chisel,
        Rotator,
        Hammer
    }

    public class Buildinator : RobotTool
    {
        public BuildinatorMode Mode;

        public Buildinator(BuildinatorMode mode)
        {
            Mode = mode;
        }

        public override Sprite GetSprite()
        {
            return null;
        }

        public override void BeginClickHold()
        {
            throw new NotImplementedException();
        }

        public override void TerminateClickHold()
        {
            throw new NotImplementedException();
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            throw new NotImplementedException();
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            throw new NotImplementedException();
        }
    }

    
    */
    
}
