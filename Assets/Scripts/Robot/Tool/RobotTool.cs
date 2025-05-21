using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using TileMaps.Layer;
using UnityEngine;

namespace Robot.Tool
{
    public enum MouseButtonKey
    {
        Left = 1,
        Right = 2
    }

    public static class MouseButtonKeyExtension
    {
        public static int ToMouseButton(this MouseButtonKey mouseButtonKey)
        {
            return (int)mouseButtonKey-1;
        }
    }

    public enum MoveDirection
    {
        Left,
        Right
    }

    public interface IAutoSelectTool
    {
        public TileMapLayer GetAutoSelectLayer();
        public Color GetColor(); 
    }
    
    public interface IRobotToolInstance : IPlayerClickHandler
    {
        public Sprite GetSprite();
        public Sprite GetPrimaryModeSprite();
        public void ModeSwitch(MoveDirection moveDirection, bool subMode);
        public string GetName();
        public string GetModeName();
        public RobotToolObject GetToolObject();
        public void Preview(Vector2Int cellPosition);
    }

    public interface IDestructiveTool
    {
        
    }
    
    public interface ISubModeRobotToolInstance {
        public string GetSubModeName();
        public Sprite GetSubModeSprite();
    }
    public abstract class RobotToolInstance<TData, TObject> : IRobotToolInstance 
        where TData : RobotToolData 
        where TObject : RobotToolObject
    {
        protected RobotToolInstance(TData toolData, TObject robotObject, RobotStatLoadOutCollection statLoadOutCollection, PlayerScript playerScript)
        {
            this.toolData = toolData;
            this.robotObject = robotObject;
            this.statLoadOutCollection = statLoadOutCollection;
            this.playerScript = playerScript;
            playerRobot = this.playerScript.GetComponent<PlayerRobot>();
        }
        protected TData toolData;
        protected TObject robotObject;
        protected RobotStatLoadOutCollection statLoadOutCollection;
        protected PlayerScript playerScript;
        protected PlayerRobot playerRobot;
        
        public Sprite GetSprite()
        {
            return robotObject?.ToolIconItem?.GetSprite();
        }
        public abstract Sprite GetPrimaryModeSprite();

        public abstract void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public abstract void TerminateClickHold(MouseButtonKey mouseButtonKey);
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
        public abstract void Preview(Vector2Int cellPosition);
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
