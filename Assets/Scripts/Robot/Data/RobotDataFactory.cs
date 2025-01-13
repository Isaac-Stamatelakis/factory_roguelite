using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Newtonsoft.Json;
using Items;
using Items.Tags;
using Player.Tool;
using Recipe.Objects;
using Robot.Tool;
using Robot.Tool.Object;

namespace RobotModule {
    public static class RobotDataFactory
    {
        private static readonly string DEFAULT_ROBOT_ID = "happy_mk1";
        public static string Serialize(RobotItemData robotItemData) {
            SeralizedRobotItemData seralizedRobotItemData = new SeralizedRobotItemData(
                RobotToolFactory.Serialize(robotItemData.ToolData)
            );
            return JsonConvert.SerializeObject(seralizedRobotItemData);
        }
        public static RobotItemData Deserialize(string data) {
            SeralizedRobotItemData seralizedRobotItemData = JsonConvert.DeserializeObject<SeralizedRobotItemData>(data);
            return new RobotItemData(
                RobotToolFactory.Deserialize(seralizedRobotItemData.SerializedToolData)
            );
        }
        public static ItemSlot GetDefaultRobot()
        {
            RobotItem robotItem = ItemRegistry.GetInstance().GetRobotItem(DEFAULT_ROBOT_ID);
            if (ReferenceEquals(robotItem,null))
            {
                throw new NullReferenceException("Tried to get default robot which was null");
            }
            
            List<RobotToolType> defaultTypes = new List<RobotToolType> { RobotToolType.LaserDrill, RobotToolType.ConduitSlicers, RobotToolType.Buildinator };
            List<RobotToolData> defaultData = new List<RobotToolData>();
            foreach (RobotToolType robotToolType in defaultTypes)
            {
                defaultData.Add(RobotToolFactory.GetDefault(robotToolType));
            }
            ItemRobotToolData defaultToolData = new ItemRobotToolData(defaultTypes, defaultData);
            RobotItemData robotItemData = new RobotItemData(
                defaultToolData
            );
            Dictionary<ItemTag,object> dict = new Dictionary<ItemTag, object>();
            dict[ItemTag.RobotData] = robotItemData;
            ItemTagCollection itemTagCollection = new ItemTagCollection(
                dict
            );
            return new ItemSlot(
                itemObject: robotItem,
                1,
                itemTagCollection
            );
        }

        public static string GetDefaultRobotData() {
            return ItemSlotFactory.seralizeItemSlot(GetDefaultRobot());
        }

        private class SeralizedRobotItemData
        {
            public string SerializedToolData;
            public SeralizedRobotItemData(string serializedToolData) {
                this.SerializedToolData = serializedToolData;
            }
        }
    }
}

