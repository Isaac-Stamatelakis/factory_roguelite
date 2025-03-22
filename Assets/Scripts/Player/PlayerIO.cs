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
using World.Dimensions.Serialization;

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
        //[SerializeField] public PlayerData playerData;
       
        public PlayerData Deserialize() {
            string path = WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player);
            if (!File.Exists(path))
            {
                return WorldCreation.GetDefaultPlayerData();
            }
            string json = WorldLoadUtils.GetWorldFileJson(WorldFileType.Player);
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json);
            return playerData;
        }

        public static void VerifyIntegrityOfPlayerData(PlayerData playerData)
        {
            const int OUT_BOUNDS_POSITION = 512;
            if (playerData.dimensionData != null)
            {
                if (Mathf.Abs(playerData.dimensionData.Y) > OUT_BOUNDS_POSITION) playerData.dimensionData.X = 0;
                if (Mathf.Abs(playerData.dimensionData.X) > OUT_BOUNDS_POSITION) playerData.dimensionData.Y = 0;
            }
            
            playerData.playerStatistics ??= new PlayerStatisticCollection();
            PlayerStatisticsUtils.VerifyStaticsCollection(playerData.playerStatistics);
        }
        
        void OnDestroy()
        {
            PlayerInventory playerInventory = GetComponent<PlayerInventory>();
            PlayerRobot playerRobot = GetComponent<PlayerRobot>();
            PlayerScript playerScript = GetComponent<PlayerScript>();
            PlayerDimensionData playerDimensionData = new PlayerDimensionData(playerScript.DimensionData,transform.position.x,transform.position.y);
            PlayerData playerData = new PlayerData(
                playerDimensionData,
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
        public PlayerData(PlayerDimensionData dimensionData, string playerRobot, string sInventoryData, string sRobotLoadOut, PlayerStatisticCollection playerStatistics) {
            this.dimensionData = dimensionData;
            this.playerRobot = playerRobot;
            this.sInventoryData = sInventoryData;
            this.sRobotLoadOut = sRobotLoadOut;
            this.playerStatistics = playerStatistics;
        }

        public PlayerDimensionData dimensionData;
        public string playerRobot;
        public string sInventoryData;
        public string sRobotLoadOut;
        public PlayerStatisticCollection playerStatistics;
    }
}