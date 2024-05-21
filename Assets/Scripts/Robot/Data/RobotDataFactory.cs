using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items;
using Items.Tags;

namespace RobotModule {
    public static class RobotDataFactory
    {
        public static string seralize(RobotItemData robotItemData) {
            SeralizedRobotItemData seralizedRobotItemData = new SeralizedRobotItemData(
                tools: ItemSlotFactory.serializeList(robotItemData.Equipment),
                accessories: ItemSlotFactory.serializeList(robotItemData.Accessories),
                robotItemData.Name
            );
            return JsonConvert.SerializeObject(seralizedRobotItemData);
        }
        public static RobotItemData deseralize(string data) {
            SeralizedRobotItemData seralizedRobotItemData = JsonConvert.DeserializeObject<SeralizedRobotItemData>(data);
            return new RobotItemData(
                equipment: ItemSlotFactory.deserialize(seralizedRobotItemData.tools),
                accessories: ItemSlotFactory.deserialize(seralizedRobotItemData.accessories),
                seralizedRobotItemData.name
            );
        }
        public static ItemSlot getDefaultRobot(bool creative) {
            string id = creative ? "happy_inf" : "happy_mk1";
            ItemObject robotItem = ItemRegistry.getInstance().GetRobotItem(id);
            if (robotItem == null) {
                Debug.LogWarning("Tried to get default robot which was null");
            }
            RobotItemData robotItemData = new RobotItemData(
                new List<ItemSlot>{

                },
                new List<ItemSlot>{
                    
                },
                "Bob"
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

        public static string getDefaultRobotString(bool creative) {
            return ItemSlotFactory.seralizeItemSlot(getDefaultRobot(creative));
        }

        private class SeralizedRobotItemData
        {
            public string tools;
            public string accessories;
            public string name;
            public SeralizedRobotItemData(string tools, string accessories, string name) {
                this.tools = tools;
                this.accessories = accessories;
                this.name = name;
            }
        }
    }
}

