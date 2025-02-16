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
        [SerializeField] public PlayerData playerData;
       
        public void Deserialize() {
            string playerJsonPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player);
            string json = WorldLoadUtils.GetWorldFileJson(WorldFileType.Player);
            playerData = JsonConvert.DeserializeObject<PlayerData>(json);
        }
        
        void OnDestroy() {
            playerData.x = transform.position.x;
            playerData.y = transform.position.y;
            playerData.dim = DimensionManager.Instance.GetPlayerDimension();
            playerData.sInventoryData = PlayerInventoryFactory.Serialize(GetComponent<PlayerInventory>().PlayerInventoryData);
            string playerJsonPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player);
            ItemSlot robotItem = GetComponent<PlayerRobot>().robotItemSlot;
            playerData.playerRobot = ItemSlotFactory.seralizeItemSlot(robotItem);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Player,json);
        }
        
        public string GetPlayerInventoryData() {
            return playerData.sInventoryData;
        }

    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(float x, float y, string playerRobot, string name, string sInventoryData) {
            this.x = x;
            this.y = y;
            this.playerRobot = playerRobot;
            this.name = name;
            this.sInventoryData = sInventoryData;
        }
        public float x;
        public float y;
        public int dim;
        public string playerRobot;
        public string name;
        public string sInventoryData;
        public int stage;
    }
}