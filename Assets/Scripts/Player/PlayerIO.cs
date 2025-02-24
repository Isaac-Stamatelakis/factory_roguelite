using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WorldModule;
using RobotModule;
using Items;
using TileMaps.Previewer;
using TileEntity.Instances;
using Dimensions;
using Item.Slot;
using Player;

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
        //[SerializeField] public PlayerData playerData;
       
        public PlayerData Deserialize() {
            string playerJsonPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player);
            string json = WorldLoadUtils.GetWorldFileJson(WorldFileType.Player);
            return JsonConvert.DeserializeObject<PlayerData>(json);
        }
        
        void OnDestroy()
        {
            PlayerInventory playerInventory = GetComponent<PlayerInventory>();
            PlayerRobot playerRobot = GetComponent<PlayerRobot>();
            PlayerData playerData = new PlayerData(
                transform.position.x, 
                transform.position.y, 
                ItemSlotFactory.seralizeItemSlot(playerRobot.robotItemSlot),
                PlayerInventoryFactory.Serialize(playerInventory.PlayerInventoryData),
                null
            );
            
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Player,json);
        }
    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(float x, float y, string playerRobot, string sInventoryData, string sRobotLoadOut) {
            this.x = x;
            this.y = y;
            this.playerRobot = playerRobot;
            this.sInventoryData = sInventoryData;
        }
        public float x;
        public float y;
        public string playerRobot;
        public string sInventoryData;
        public string sRobotLoadOut;
    }
}