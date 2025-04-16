using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Conduit.Placement.LoadOut;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WorldModule;
using RobotModule;
using Items;
using TileMaps.Previewer;
using TileEntity.Instances;
using Dimensions;
using Item.GrabbedItem;
using Item.Slot;
using Player;
using Robot.Upgrades;
using UI.Statistics;
using World.Dimensions.Serialization;

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
       
        public PlayerData Deserialize() {
            string path = WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player);
            if (!File.Exists(path))
            {
                return WorldCreation.GetDefaultPlayerData();
            }
            string json = WorldLoadUtils.GetWorldFileJson(WorldFileType.Player);
            PlayerData playerData;
            try
            {
                playerData = JsonConvert.DeserializeObject<PlayerData>(json);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError($"Resetting Player Inventory: {e.Message}");
                playerData = WorldCreation.GetDefaultPlayerData();
            }
            
            return playerData;
        }

        public static void VerifyIntegrityOfPlayerData(PlayerData playerData)
        {
            playerData.playerStatistics ??= new PlayerStatisticCollection();
            PlayerStatisticsUtils.VerifyStaticsCollection(playerData.playerStatistics);
            playerData.miscPlayerData ??= WorldCreation.GetDefaultMiscPlayerData();
        }

        public void Serialize()
        {
            PlayerInventory playerInventory = GetComponent<PlayerInventory>();
            PlayerRobot playerRobot = GetComponent<PlayerRobot>();
            PlayerScript playerScript = GetComponent<PlayerScript>();
            PlayerDimensionData playerDimensionData = new PlayerDimensionData(playerScript.DimensionData,transform.position.x,transform.position.y);

            MiscPlayerData miscPlayerData = new MiscPlayerData
            {
                GrabbedItemData = ItemSlotFactory.seralizeItemSlot(GrabbedItemProperties.Instance.ItemSlot),
                ConduitPortPlacementLoadOuts = playerScript.ConduitPlacementOptions.ConduitPlacementLoadOuts
            };
            PlayerData playerData = new PlayerData(
                playerDimensionData,
                ItemSlotFactory.seralizeItemSlot(playerRobot.robotItemSlot),
                PlayerInventoryFactory.Serialize(playerInventory.PlayerInventoryData),
                JsonConvert.SerializeObject(playerRobot.RobotUpgradeLoadOut),
                playerScript.PlayerStatisticCollection,
                miscPlayerData
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Player,json);
        }
        void OnDestroy()
        {
            Serialize();
        }
    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(PlayerDimensionData dimensionData, string playerRobot, string sInventoryData, string sRobotLoadOut, PlayerStatisticCollection playerStatistics, MiscPlayerData miscPlayerData) {
            this.dimensionData = dimensionData;
            this.playerRobot = playerRobot;
            this.sInventoryData = sInventoryData;
            this.sRobotLoadOut = sRobotLoadOut;
            this.playerStatistics = playerStatistics;
            this.miscPlayerData = miscPlayerData;
        }

        public PlayerDimensionData dimensionData;
        public string playerRobot;
        public string sInventoryData;
        public string sRobotLoadOut;
        public PlayerStatisticCollection playerStatistics;
        public MiscPlayerData miscPlayerData;
    }

    public class MiscPlayerData
    {
        public string GrabbedItemData;
        public Dictionary<LoadOutConduitType, IOConduitPortData> ConduitPortPlacementLoadOuts;
    }
}