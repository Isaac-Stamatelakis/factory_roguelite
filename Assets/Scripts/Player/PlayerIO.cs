using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WorldModule;
using RobotModule;
using ItemModule;
using TileMapModule.Previewer;
using TileEntityModule.Instances;
using DimensionModule;

namespace PlayerModule.IO {

    public class PlayerIO : MonoBehaviour
    {
        [SerializeField]
        public PlayerData playerData;
        TilePlacePreviewController tilePlacePreviewController;
        DevMode devMode;
        // Start is called before the first frame update
        void Awake()
        {
            devMode = GetComponent<DevMode>();
            tilePlacePreviewController = GameObject.Find("PlacePreviewController").GetComponent<TilePlacePreviewController>();
        }

        public void initRead() {
            string playerJsonPath =  WorldCreation.getPlayerDataPath(Global.WorldName);
            string json = File.ReadAllText(playerJsonPath);
            playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(json);
            //transform.position = new Vector3(playerData.x,playerData.y,transform.position.z);
            GetComponent<PlayerRobot>().setRobot(ItemRegistry.getInstance().GetRobotItem(playerData.robotID));
            tilePlacePreviewController.toggle();
        }

    
        public Vector2 getPlayerPosition() {
            return new Vector2(playerData.x,playerData.y);
        }

        void OnDestroy() {
            playerData.x = transform.position.x;
            playerData.y = transform.position.y;
            playerData.dim = DimensionManagerContainer.getManager().Dim;
            playerData.inventoryJson = GetComponent<PlayerInventory>().getJson();
            string playerJsonPath =  WorldCreation.getPlayerDataPath(Global.WorldName);
            RobotItem robotItem = GetComponent<PlayerRobot>().robotItem;
            if (robotItem == null) {
                playerData.robotID = null;
            } else {
                playerData.robotID = robotItem.id;
            }
            File.WriteAllText(playerJsonPath,Newtonsoft.Json.JsonConvert.SerializeObject(playerData));
        }
        
        public string getPlayerInventoryData() {
            return playerData.inventoryJson;
        }

    }

    [System.Serializable]
    public class PlayerData {
        public PlayerData(float x, float y, string robotID, string name, string inventoryJson) {
            this.x = x;
            this.y = y;
            this.robotID = robotID;
            this.name = name;
            this.inventoryJson = inventoryJson;
        }
        public float x;
        public float y;
        public int dim;
        public string robotID;
        public string name;
        public string inventoryJson;
        public int stage;
    }
}