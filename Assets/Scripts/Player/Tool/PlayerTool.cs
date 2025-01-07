using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TileMaps.Layer;
using UnityEngine;

namespace Player.Tool
{
    public abstract class PlayerTool
    {
        public abstract Sprite GetSprite();
    }

    public class LaserDrill : PlayerTool
    {
        public TileMapLayer Layer;
        public float HitRate;
        public int HitDamage;

        public LaserDrill(TileMapLayer layer, float hitRate, int hitDamage)
        {
            Layer = layer;
            HitRate = hitRate;
            HitDamage = hitDamage;
        }

        public override Sprite GetSprite()
        {
            PlayerToolObject playerToolObject = PlayerToolRegistry.GetInstance().GetToolObject(PlayerToolType.LaserDrill);
            switch (Layer)
            {
                case TileMapLayer.Base:
                    return playerToolObject.Sprites[0];
                case TileMapLayer.Background:
                    return playerToolObject.Sprites[1];
                default:
                    return null;
            }
        }
    }

    public class LaserGun : PlayerTool
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
    }

    public enum ConduitBreakMode
    {
        Standard,
        Disconnect
    }
    public class ConduitSlicer : PlayerTool
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
    }
    public enum BuildinatorMode
    {
        Chisel,
        Rotator,
        Hammer
    }

    public class Buildinator : PlayerTool
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
    }

    public static class PlayerToolFactory
    {
        public static PlayerTool GetDefault(PlayerToolType toolType)
        {
            switch (toolType)
            {
                case PlayerToolType.LaserGun:
                    return new LaserGun(1f, 1f, 1f);
                case PlayerToolType.LaserDrill:
                    return new LaserDrill(TileMapLayer.Base, 1f, 1);
                case PlayerToolType.ConduitSlicers:
                    return new ConduitSlicer(TileMapLayer.Item, ConduitBreakMode.Standard);
                case PlayerToolType.Buildinator:
                    return new Buildinator(BuildinatorMode.Hammer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(toolType), toolType, null);
            }
        }

        public static List<PlayerTool> GetDefaults()
        {
            List<PlayerToolType> defaultTypes = GetDefaultTypes();
            List<PlayerTool> defaults = new List<PlayerTool>();
            foreach (PlayerToolType defaultType in defaultTypes)
            {
                defaults.Add(GetDefault(defaultType));
            }

            return defaults;
        }

        public static List<PlayerToolType> GetDefaultTypes()
        {
            return new List<PlayerToolType>
            {
                PlayerToolType.LaserGun,
                PlayerToolType.LaserDrill,
                PlayerToolType.ConduitSlicers,
                PlayerToolType.Buildinator
            };
        }

        public static PlayerToolType GetType(PlayerTool playerTool)
        {
            return playerTool switch
            {
                LaserDrill => PlayerToolType.LaserDrill,
                LaserGun => PlayerToolType.LaserGun,
                ConduitSlicer => PlayerToolType.ConduitSlicers,
                Buildinator => PlayerToolType.Buildinator,
                _ => throw new ArgumentOutOfRangeException(nameof(playerTool), playerTool, null)
            };
        }

        public static List<PlayerTool> Deserialize(List<string> serializedTools, string serializedTypes)
        {
            List<PlayerTool> tools = new List<PlayerTool>();
            List<PlayerToolType> types = DeserializeTypes(serializedTypes);
            for (int i = 0; i < types.Count; i++)
            {
                PlayerTool playerTool = DeserializeTool(serializedTools[i], types[i]);
                tools.Add(playerTool);
            }
            return tools;
        }

        public static List<PlayerToolType> DeserializeTypes(string serializedTypes)
        {
            if (serializedTypes == null) return GetDefaultTypes();
            try
            {
                List<int> intTypes = JsonConvert.DeserializeObject<List<int>>(serializedTypes);
                List<PlayerToolType> types = new List<PlayerToolType>();
                foreach (int type in intTypes)
                {
                    types.Add((PlayerToolType)type);
                }

                return types;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return GetDefaultTypes();
            }
        }
        public static PlayerTool DeserializeTool(string serializedTool, PlayerToolType toolType)
        {
            if (serializedTool == null) return GetDefault(toolType);
            try
            {
                switch (toolType)
                {
                    case PlayerToolType.LaserGun:
                        return JsonConvert.DeserializeObject<LaserGun>(serializedTool);
                    case PlayerToolType.LaserDrill:
                        return JsonConvert.DeserializeObject<LaserDrill>(serializedTool);
                    case PlayerToolType.ConduitSlicers:
                        return JsonConvert.DeserializeObject<ConduitSlicer>(serializedTool);
                    case PlayerToolType.Buildinator:
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

        public static List<string> SerializeList(List<PlayerTool> tools)
        {
            List<string> serializedTools = new List<string>();
            foreach (PlayerTool tool in tools)
            {
                serializedTools.Add(JsonConvert.SerializeObject(tool));
            }

            return serializedTools;
        }
    }
    
}
