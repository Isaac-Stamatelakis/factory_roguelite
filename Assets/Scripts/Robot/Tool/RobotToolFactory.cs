using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player.Tool;
using Player.Tool.Object;
using Robot.Tool.Instances;
using Robot.Tool.Object;
using TileMaps.Layer;
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
                if (robotToolObject is RobotDrillObject robotDrillObject)
                {
                    dict[RobotToolType.LaserDrill] = robotDrillObject;
                }
            }

            return dict;
        } 
        public static IRobotToolInstance GetInstance(RobotToolType type, RobotToolObject toolObject, RobotToolData robotToolData)
        {
            switch (type)
            {
                case RobotToolType.LaserDrill:
                    return new LaserDrill(robotToolData as LaserDrillData, toolObject as RobotDrillObject);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static RobotToolData GetDefault(RobotToolType toolType)
        {
            switch (toolType)
            {
                case RobotToolType.LaserGun:
                    return null;
                //return new LaserGun(1f, 1f, 1f);
                case RobotToolType.LaserDrill:
                    return new LaserDrillData(TileMapLayer.Base, 1f, 1);
                case RobotToolType.ConduitSlicers:
                    return null;
                //return new ConduitSlicer(TileMapLayer.Item, ConduitBreakMode.Standard);
                case RobotToolType.Buildinator:
                    return null;
                //return new Buildinator(BuildinatorMode.Hammer);
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
            SerializedItemRobotToolData sData = new SerializedItemRobotToolData(toolData, itemRobotToolData.Types);
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
            return new ItemRobotToolData( robotToolData.Types, robotTools);
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

        private class SerializedItemRobotToolData
        {
            public List<string> ToolData;
            public List<RobotToolType> Types;

            public SerializedItemRobotToolData(List<string> toolData, List<RobotToolType> types)
            {
                ToolData = toolData;
                this.Types = types;
            }
        }
    }

    public class ItemRobotToolData
    {
        public List<RobotToolType> Types;
        public List<RobotToolData> Tools;

        public ItemRobotToolData(List<RobotToolType> types, List<RobotToolData> tools)
        {
            Types = types;
            Tools = tools;
        }
    }
}