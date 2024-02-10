using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WorldDataModule;
using RobotModule;

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
        transform.position = new Vector3(playerData.x,playerData.y,transform.position.z);
        GetComponent<PlayerRobot>().setRobot(ItemRegistry.getInstance().GetRobotItem(playerData.robotID));
        StartCoroutine(setPlayerDynamic());
        tilePlacePreviewController.toggle();
    }

    private IEnumerator setPlayerDynamic() {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
    // Update is called once per frame
    void Update()
    {
        /*
        playerData["enablePlacePreview"] = devMode.placePreview;
        if (devMode.placePreview != tilePlacePreviewController.On) {
            tilePlacePreviewController.toggle();
        }
        */
    }

    void OnDestroy() {
        playerData.x = transform.position.x;
        playerData.y = transform.position.y;
        playerData.inventoryJson = GetComponent<PlayerInventory>().getJson();
        string playerJsonPath =  WorldCreation.getPlayerDataPath(Global.WorldName);
        playerData.robotID = GetComponent<PlayerRobot>().robotItem.id;
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
    public string robotID;
    public string name;
    public string inventoryJson;
}