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
using Robot.Upgrades;
using UI.Statistics;

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
        //[SerializeField] public PlayerData playerData;
       
        public PlayerData Deserialize() {
            string json = WorldLoadUtils.GetWorldFileJson(WorldFileType.Player);
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json);
            VerifyIntegrityOfPlayerData(playerData);
            return playerData;
        }

        private void VerifyIntegrityOfPlayerData(PlayerData playerData)
        {
            const int OUT_BOUNDS_POSITION = 512;
            if (Mathf.Abs(playerData.x) > OUT_BOUNDS_POSITION) playerData.x = 0;
            if (Mathf.Abs(playerData.y) > OUT_BOUNDS_POSITION) playerData.y = 0;
            playerData.playerStatistics ??= new PlayerStatisticCollection();
            PlayerStatisticsUtils.VerifyStaticsCollection(playerData.playerStatistics);
        }
        
        void OnDestroy()
        {
            PlayerInventory playerInventory = GetComponent<PlayerInventory>();
            PlayerRobot playerRobot = GetComponent<PlayerRobot>();
            PlayerScript playerScript = GetComponent<PlayerScript>();
            PlayerData playerData = new PlayerData(
                transform.position.x, 
                transform.position.y, 
                ItemSlotFactory.seralizeItemSlot(playerRobot.robotItemSlot),
                PlayerInventoryFactory.Serialize(playerInventory.PlayerInventoryData),
                JsonConvert.SerializeObject(playerRobot.RobotUpgradeLoadOut),
                playerScript.PlayerStatisticCollection
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Player,json);
        }
    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(float x, float y, string playerRobot, string sInventoryData, string sRobotLoadOut, PlayerStatisticCollection playerStatistics) {
            this.x = x;
            this.y = y;
            this.playerRobot = playerRobot;
            this.sInventoryData = sInventoryData;
            this.sRobotLoadOut = sRobotLoadOut;
            this.playerStatistics = playerStatistics;
        }
        public float x;
        public float y;
        public string playerRobot;
        public string sInventoryData;
        public string sRobotLoadOut;
        public PlayerStatisticCollection playerStatistics;
    }
}