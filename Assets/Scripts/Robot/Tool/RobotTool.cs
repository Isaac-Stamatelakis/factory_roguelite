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
    public abstract class RobotTool : IPlayerClickHandler
    {
        public abstract Sprite GetSprite();
        public abstract void BeginClickHold();
        public abstract void TerminateClickHold();
        public abstract void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public abstract bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time);
    }

    public class LaserDrill : RobotTool
    {
        public TileMapLayer Layer;
        public float HitRate;
        public int HitDamage;
        private LineRenderer lineRenderer;

        public LaserDrill(TileMapLayer layer, float hitRate, int hitDamage)
        {
            Layer = layer;
            HitRate = hitRate;
            HitDamage = hitDamage;
        }

        public override Sprite GetSprite()
        {
            RobotDrillObject robotToolObject = PlayerToolRegistry.GetInstance().GetToolObject<RobotDrillObject>(RobotToolType.LaserDrill);
            switch (Layer)
            {
                case TileMapLayer.Base:
                    return robotToolObject.BaseLayerSprite;
                case TileMapLayer.Background:
                    return robotToolObject.BackgroundLayerSprite;
                default:
                    return null;
            }
        }

        public override void BeginClickHold()
        {
            RobotDrillObject robotDrillObject = PlayerToolRegistry.GetInstance().GetToolObject<RobotDrillObject>(RobotToolType.LaserDrill);
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            lineRenderer = GameObject.Instantiate(robotDrillObject.LineRendererPrefab,playerTransform);
        }

        public override void TerminateClickHold()
        {
            GameObject.Destroy(lineRenderer.gameObject);
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return;
            UpdateLineRenderer(mousePosition);
            MouseUtils.HitTileLayer(Layer, mousePosition);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return false;
            bool pass = time >= HitRate;
            if (!pass)
            {
                UpdateLineRenderer(mousePosition);
                return false;
            }
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        private void UpdateLineRenderer(Vector2 mousePosition)
        {
            Vector2 dif =  mousePosition - (Vector2) PlayerManager.Instance.GetPlayer().transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
        }
    }

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

    public static class PlayerToolFactory
    {
        public static RobotTool GetDefault(RobotToolType toolType)
        {
            switch (toolType)
            {
                case RobotToolType.LaserGun:
                    return new LaserGun(1f, 1f, 1f);
                case RobotToolType.LaserDrill:
                    return new LaserDrill(TileMapLayer.Base, 1f, 1);
                case RobotToolType.ConduitSlicers:
                    return new ConduitSlicer(TileMapLayer.Item, ConduitBreakMode.Standard);
                case RobotToolType.Buildinator:
                    return new Buildinator(BuildinatorMode.Hammer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(toolType), toolType, null);
            }
        }

        public static List<RobotTool> GetDefaults()
        {
            List<RobotToolType> defaultTypes = GetDefaultTypes();
            List<RobotTool> defaults = new List<RobotTool>();
            foreach (RobotToolType defaultType in defaultTypes)
            {
                defaults.Add(GetDefault(defaultType));
            }

            return defaults;
        }

        public static List<RobotToolType> GetDefaultTypes()
        {
            return new List<RobotToolType>
            {
                RobotToolType.LaserGun,
                RobotToolType.LaserDrill,
                RobotToolType.ConduitSlicers,
                RobotToolType.Buildinator
            };
        }

        public static RobotToolType GetType(RobotTool robotTool)
        {
            return robotTool switch
            {
                LaserDrill => RobotToolType.LaserDrill,
                LaserGun => RobotToolType.LaserGun,
                ConduitSlicer => RobotToolType.ConduitSlicers,
                Buildinator => RobotToolType.Buildinator,
                _ => throw new ArgumentOutOfRangeException(nameof(robotTool), robotTool, null)
            };
        }

        public static List<RobotTool> Deserialize(List<string> serializedTools, string serializedTypes)
        {
            List<RobotTool> tools = new List<RobotTool>();
            List<RobotToolType> types = DeserializeTypes(serializedTypes);
            for (int i = 0; i < types.Count; i++)
            {
                RobotTool robotTool = DeserializeTool(serializedTools[i], types[i]);
                tools.Add(robotTool);
            }
            return tools;
        }

        public static List<RobotToolType> DeserializeTypes(string serializedTypes)
        {
            if (serializedTypes == null) return GetDefaultTypes();
            try
            {
                List<int> intTypes = JsonConvert.DeserializeObject<List<int>>(serializedTypes);
                List<RobotToolType> types = new List<RobotToolType>();
                foreach (int type in intTypes)
                {
                    types.Add((RobotToolType)type);
                }

                return types;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return GetDefaultTypes();
            }
        }
        public static RobotTool DeserializeTool(string serializedTool, RobotToolType toolType)
        {
            if (serializedTool == null) return GetDefault(toolType);
            try
            {
                switch (toolType)
                {
                    case RobotToolType.LaserGun:
                        return JsonConvert.DeserializeObject<LaserGun>(serializedTool);
                    case RobotToolType.LaserDrill:
                        return JsonConvert.DeserializeObject<LaserDrill>(serializedTool);
                    case RobotToolType.ConduitSlicers:
                        return JsonConvert.DeserializeObject<ConduitSlicer>(serializedTool);
                    case RobotToolType.Buildinator:
                        return JsonConvert.DeserializeObject<Buildinator>(serializedTool);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(toolType), toolType, null);
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return GetDefault(toolType);
            }
        }

        public static List<string> SerializeList(List<RobotTool> tools)
        {
            List<string> serializedTools = new List<string>();
            foreach (RobotTool tool in tools)
            {
                serializedTools.Add(JsonConvert.SerializeObject(tool));
            }

            return serializedTools;
        }
    }
    
}
