using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerIO : MonoBehaviour
{
    private Dictionary<string, object> playerData = new Dictionary<string, object>();
    TilePlacePreviewController tilePlacePreviewController;
    DevMode devMode;
    // Start is called before the first frame update
    void Start()
    {
        
        devMode = GetComponent<DevMode>();
        tilePlacePreviewController = GameObject.Find("PlacePreviewController").GetComponent<TilePlacePreviewController>();
    }

    public void initRead() {
        playerData = new Dictionary<string, object>();
        readPlayerData();

        transform.position = (Vector3) playerData["location"];
    }
    // Update is called once per frame
    void Update()
    {

        playerData["enablePlacePreview"] = devMode.placePreview;
        if (devMode.placePreview != tilePlacePreviewController.On) {
            tilePlacePreviewController.toggle();
        }
    }

    void Destroy() {
        playerData["location"] = transform.position;
        string filePath = Application.dataPath + "/Resources/worlds/" + Global.WorldName + "/PlayerData.json";
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(playerData, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, jsonString);
    }
    private void readPlayerData() {
        string filePath = Application.dataPath + "/Resources/worlds/" + Global.WorldName + "/PlayerData.json";
        string json = File.ReadAllText(filePath);
        Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
        playerData["name"] = (string) jObject["name"];
        string locationJson = jObject["location"].ToString();
        Newtonsoft.Json.Linq.JObject locationJObject = Newtonsoft.Json.Linq.JObject.Parse(locationJson);
        playerData["location"] = new Vector3(
            (float) locationJObject["x"],
            (float) locationJObject["y"],
            -5
        );
        playerData["hp"] = (float) jObject["hp"];
        ItemContainer inventory = new ItemContainer(MatterImporter.jsonToMatter(jObject["inventory"].ToString()));
        playerData["inventory"] = inventory;
        playerData["enablePlacePreview"] = devMode.placePreview;
        tilePlacePreviewController.setActive((bool) playerData["enablePlacePreview"]);
        Debug.Log(inventory.Count);
    }

    public ItemContainer getPlayerInventory() {
        return (ItemContainer) playerData["inventory"];
    }

}