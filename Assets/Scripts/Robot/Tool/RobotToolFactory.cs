using System;
using System.Collections.Generic;
using Item.Slot;
using Newtonsoft.Json;
using Player;
using Player.Tool;
using Player.Tool.Object;
using Robot.Tool.Instances;
using Robot.Tool.Object;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using TileMaps.Layer;
using UnityEditor.UIElements;
using UnityEngine;

namespace Robot.Tool
{
    public static class RobotToolFactory
    {
        public static Dictionary<RobotToolType, RobotToolObject> GetDictFromCollection(
            RobotToolObjectCollection collection)
        {
            var dict = new Dictionary<RobotToolType, RobotToolObject>();
            if (ReferenceEquals(collection,null)) throw new NullReferenceException("Tried to get robot tool objects from a null collection");
            foreach (RobotToolObject robotToolObject in collection.Tools)
            {
                switch (robotToolObject)
                {
                    case RobotDrillObject:
                        dict[RobotToolType.LaserDrill] = robotToolObject;
                        break;
                    case RobotConduitCutterObject:
                        dict[RobotToolType.ConduitSlicers] = robotToolObject;
                        break;
                    case BuildinatorObject:
                        dict[RobotToolType.Buildinator] = robotToolObject;
                        break;
                    case RobotLaserGunObject:
                        dict[RobotToolType.LaserGun] = robotToolObject;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            return dict;
        } 
        
        public static IRobotToolInstance GetInstance(RobotToolType type, RobotToolObject toolObject, RobotToolData robotToolData, RobotStatLoadOutCollection loadOut, PlayerScript playerScript)
        {
            switch (type)
            {
                case RobotToolType.LaserDrill:
                    return new LaserDrill(robotToolData as LaserDrillData, toolObject as RobotDrillObject, loadOut, playerScript);
                case RobotToolType.ConduitSlicers:
                    return new ConduitCutters(robotToolData as ConduitCuttersData, toolObject as RobotConduitCutterObject, loadOut, playerScript);
                case RobotToolType.Buildinator:
                    return new Buildinator(robotToolData as BuildinatorData, toolObject as BuildinatorObject, loadOut, playerScript);
                case RobotToolType.LaserGun:
                    return new LaserGun(robotToolData as LaserGunData, toolObject as RobotLaserGunObject, loadOut, playerScript);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static RobotToolData GetDefault(RobotToolType toolType)
        {
            switch (toolType)
            {
                case RobotToolType.LaserDrill:
                    return new LaserDrillData(TileMapLayer.Base, 0.125f, 1);
                case RobotToolType.ConduitSlicers:
                    return new ConduitCuttersData();
                case RobotToolType.Buildinator:
                    return new BuildinatorData(BuildinatorMode.Rotator);
                case RobotToolType.LaserGun:
                    return new LaserGunData();
                default:
                    throw new ArgumentOutOfRangeException(nameof(toolType), toolType, null);
            }
        }

        public static List<RobotToolData> GetDefaults()
        {
            List<RobotToolType> defaultTypes = GetDefaultTypes();
            List<RobotToolData> defaults = new List<RobotToolData>();
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
        
        public static string Serialize(ItemRobotToolData itemRobotToolData)
        {
            List<string> toolData = SerializeList(itemRobotToolData.Tools);
            SerializedItemRobotToolData sData = new SerializedItemRobotToolData(toolData, itemRobotToolData.Types, itemRobotToolData.Upgrades);
            return JsonConvert.SerializeObject(sData);
        }

        public static ItemRobotToolData Deserialize(string serializedToolData)
        {
            SerializedItemRobotToolData robotToolData = JsonConvert.DeserializeObject<SerializedItemRobotToolData>(serializedToolData);
            List<RobotToolData> robotTools = new List<RobotToolData>();
            for (int i = 0; i < robotToolData.ToolData.Count; i++)
            {
                robotTools.Add(Deserialize(robotToolData.ToolData[i], robotToolData.Types[i]));
            }
            return new ItemRobotToolData( robotToolData.Types, robotTools, robotToolData.Upgrades);
        }

        public static RobotToolData Deserialize(string toolData, RobotToolType type)
        {
            if (toolData == null) return GetDefault(type);
            try
            {
                switch (type)
                {
                    case RobotToolType.LaserDrill:
                        return JsonConvert.DeserializeObject<LaserDrillData>(toolData);
                    case RobotToolType.ConduitSlicers:
                        return JsonConvert.DeserializeObject<ConduitCuttersData>(toolData);
                    case RobotToolType.Buildinator:
                        return JsonConvert.DeserializeObject<BuildinatorData>(toolData);
                    case RobotToolType.LaserGun:
                        return JsonConvert.DeserializeObject<LaserGunData>(toolData);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return GetDefault(type);
            }
            
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
        public static RobotToolData DeserializeTool(string serializedTool, RobotToolType toolType)
        {
            if (serializedTool == null) return GetDefault(toolType);
            try
            {
                switch (toolType)
                {
                    case RobotToolType.LaserDrill:
                        return JsonConvert.DeserializeObject<LaserDrillData>(serializedTool);
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

        public static List<string> SerializeList(List<RobotToolData> tools)
        {
            List<string> serializedTools = new List<string>();
            foreach (RobotToolData tool in tools)
            {
                string sToolData = JsonConvert.SerializeObject(tool);
                serializedTools.Add(sToolData);
            }

            return serializedTools;
        }

        public static List<ItemSlot> ToolInstancesToItems(List<IRobotToolInstance> toolInstances)
        {
            List<ItemSlot> items = new List<ItemSlot>();
            foreach (IRobotToolInstance tool in toolInstances)
            {
                items.Add(new ItemSlot(tool.GetToolObject().ToolIconItem,1,null));
            }

            return items;
        }

        private class SerializedItemRobotToolData
        {
            public List<string> ToolData;
            public List<RobotToolType> Types;
            public List<List<RobotUpgradeData>> Upgrades;

            public SerializedItemRobotToolData(List<string> toolData, List<RobotToolType> types, List<List<RobotUpgradeData>> upgrades)
            {
                ToolData = toolData;
                this.Types = types;
                this.Upgrades = upgrades;
            }
        }
        
        
    }

    public class ItemRobotToolData
    {
        public List<RobotToolType> Types;
        public List<RobotToolData> Tools;
        public List<List<RobotUpgradeData>> Upgrades;

        public ItemRobotToolData(List<RobotToolType> types, List<RobotToolData> tools, List<List<RobotUpgradeData>> upgrades)
        {
            Types = types;
            Tools = tools;
            Upgrades = upgrades;
        }
    }
}