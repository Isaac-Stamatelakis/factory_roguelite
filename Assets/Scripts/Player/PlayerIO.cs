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

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
        [SerializeField] public PlayerData playerData;
        TilePlacePreviewController tilePlacePreviewController;
        DevMode devMode;
        // Start is called before the first frame update
        void Awake()
        {
            devMode = GetComponent<DevMode>();
            tilePlacePreviewController = GameObject.Find("PlacePreviewController").GetComponent<TilePlacePreviewController>();
        }

        public void initRead() {
            string playerJsonPath = WorldLoadUtils.getPlayerDataPath();
            string json = File.ReadAllText(playerJsonPath);
            playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(json);
            //transform.position = new Vector3(playerData.x,playerData.y,transform.position.z);
            ItemSlot playerRobotItem = ItemSlotFactory.deseralizeItemSlotFromString(playerData.playerRobot);
            GetComponent<PlayerRobot>().setRobot(playerRobotItem);
            tilePlacePreviewController.Toggle();
        }

    
        public Vector2 getPlayerPosition() {
            return new Vector2(playerData.x,playerData.y);
        }

        void OnDestroy() {
            playerData.x = transform.position.x;
            playerData.y = transform.position.y;
            playerData.dim = DimensionManager.Instance.getPlayerDimension(transform);
            playerData.inventoryJson = GetComponent<PlayerInventory>().getJson();
            string playerJsonPath =  WorldLoadUtils.getPlayerDataPath();
            ItemSlot robotItem = GetComponent<PlayerRobot>().robotItemSlot;
            if (robotItem == null) {
                playerData.playerRobot = null;
            } else {
                playerData.playerRobot = ItemSlotFactory.seralizeItemSlot(robotItem);
            }
            File.WriteAllText(playerJsonPath,Newtonsoft.Json.JsonConvert.SerializeObject(playerData));
        }
        
        public string getPlayerInventoryData() {
            return playerData.inventoryJson;
        }

    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(float x, float y, string playerRobot, string name, string inventoryJson) {
            this.x = x;
            this.y = y;
            this.playerRobot = playerRobot;
            this.name = name;
            this.inventoryJson = inventoryJson;
        }
        public float x;
        public float y;
        public int dim;
        public string playerRobot;
        public string name;
        public string inventoryJson;
        public int stage;
    }
}