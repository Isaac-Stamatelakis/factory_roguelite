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
using Robot;
using Robot.Tool;
using Robot.Tool.Object;
using Robot.Upgrades;
using Robot.Upgrades.Network;

namespace RobotModule {
    public static class RobotDataFactory
    {
        private static readonly string DEFAULT_ROBOT_ID = "happy_mk1";
        public static string Serialize(RobotItemData robotItemData) {
            SeralizedRobotItemData seralizedRobotItemData = new SeralizedRobotItemData(
                RobotToolFactory.Serialize(robotItemData.ToolData),
                robotItemData.RobotUpgrades,
                robotItemData.Health,
                robotItemData.Energy,
                robotItemData.nanoBotTime
            );
            return JsonConvert.SerializeObject(seralizedRobotItemData);
        }
        public static RobotItemData Deserialize(string data) {
            SeralizedRobotItemData seralizedRobotItemData = JsonConvert.DeserializeObject<SeralizedRobotItemData>(data);
            RobotItemData robotItemData = new RobotItemData(
                RobotToolFactory.Deserialize(seralizedRobotItemData.SerializedToolData),
                seralizedRobotItemData.RobotUpgradeData,
                seralizedRobotItemData.Health,
                seralizedRobotItemData.Energy,
                seralizedRobotItemData.NanoBotTime
                
            );
            robotItemData.RobotUpgrades ??= new List<RobotUpgradeData>();
            return robotItemData;

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
            List<List<RobotUpgradeData>> newUpgradeData = new List<List<RobotUpgradeData>>();
            foreach (RobotToolType robotToolType in defaultTypes)
            {
                defaultData.Add(RobotToolFactory.GetDefault(robotToolType));
                newUpgradeData.Add(new List<RobotUpgradeData>());
            }
            RobotObject robotObject = robotItem.robot;
            
            
            ItemRobotToolData defaultToolData = new ItemRobotToolData(defaultTypes, defaultData, newUpgradeData);
            RobotItemData robotItemData = new RobotItemData(defaultToolData, new List<RobotUpgradeData>(), robotObject.BaseHealth, 0,0);
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
            public ulong Energy;
            public float Health;
            public float NanoBotTime;
            public List<RobotUpgradeData> RobotUpgradeData;
            public SeralizedRobotItemData(string serializedToolData, List<RobotUpgradeData> robotUpgradeData, float health, ulong energy, float nanoBotTime) {
                this.SerializedToolData = serializedToolData;
                this.Energy = energy;
                this.Health = health;
                this.RobotUpgradeData = robotUpgradeData;
                this.NanoBotTime = nanoBotTime;
            }
        }
    }
}

