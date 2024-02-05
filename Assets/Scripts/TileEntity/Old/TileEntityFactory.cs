using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using System;
using Newtonsoft.Json;


/// <summary>
/// Creates tileEntities from idMap data
/// </summary>
public class TileEntityFactory
{
    public static void createTileEntity(TileItem tileItem, Dictionary<string,object> data, Transform tileEntityContainer, string tileContainerName, UnityEngine.Vector2Int placePosition) {
        GameObject tileEntity = initalizeTileEntityGameObject(tileItem, data, tileEntityContainer,tileContainerName,placePosition);
    }
    
    public static void softLoadTileEntity(TileItem tileItem, Dictionary<string,object> data, Transform tileEntityContainer, string tileContainerName, UnityEngine.Vector2Int placePosition) {
        GameObject tileEntity = initalizeTileEntityGameObject(tileItem,data, tileEntityContainer,tileContainerName,placePosition);
    }

    /// <summary>
    /// loads all tileEntityProperties which are only required when a chunk is full loaded
    /// Must be soft loaded first in order to be full loaded
    /// </summary>
    public static void fullLoadTileEntity(GameObject tileEntity) {
        /*
        if (Global.findChild(tileEntity.transform, "FullLoadedContainer") != null) {
            return;
        }
        GameObject fullLoadedContainer = createFullLoadedContainer(tileEntity);
        SDictionary tileEntityOptions = tileEntity.GetComponent<TileEntityProperties>().TileEntityOptions;

        foreach (string constructor in tileEntityOptions.Keys) {
            activeFullLoaded(constructor, tileEntityOptions.get(constructor), fullLoadedContainer);
        }
        */
    }

    private static GameObject createFullLoadedContainer(GameObject tileEntity) {
        GameObject fullLoadedContainer = new GameObject();
        fullLoadedContainer.transform.SetParent(tileEntity.transform);
        fullLoadedContainer.transform.localPosition = Vector3.zero;
        fullLoadedContainer.name = "FullLoadedContainer";
        return fullLoadedContainer;
    }
    public static void unFullLoad(GameObject tileEntity) {

    }

    private static GameObject initalizeTileEntityGameObject(TileItem tileItem, Dictionary<string,object> data,Transform tileEntityContainer, string tileContainerName, UnityEngine.Vector2Int placePosition) {
        
        
        GameObject tileEntity = null;
        /*
        if (data == null) {
            data = new Dictionary<string, object>();
        }
        
        string prefabPath = IdDataMap.getInstance().getIdTileData(id).tileEntityPrefabPath;
        if (prefabPath == null) {
            return null;
        }
        string folderPath = "Prefabs/TileEntities/" + prefabPath + "/";

        tileEntity = GameObject.Instantiate(Resources.Load<GameObject>(folderPath + "TileEntity"));
        tileEntity.transform.SetParent(tileEntityContainer);
        
        TileEntityProperties tileEntityProperties = tileEntity.GetComponent<TileEntityProperties>();

        tileEntityProperties.Data = data;
        tileEntityProperties.TileContainerName = tileContainerName;

        Vector2 spriteSize = Global.getSpriteSize(id);
        tileEntity.transform.localPosition = new Vector3(placePosition.x/2f+spriteSize.x/2,placePosition.y/2f+spriteSize.y/2,0);
        tileEntityProperties.Position = placePosition;

        tileEntity.name = tileContainerName + "[" + placePosition.x + "," + placePosition.y +"]";
        TileEntityStorageProperties tileEntityStorageProperties = tileEntity.GetComponent<TileEntityStorageProperties>();
        if (tileEntityStorageProperties != null) {
            if (data.ContainsKey("storage")) {
                tileEntityStorageProperties.StorageContainers = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(data["storage"].ToString());
            }
        }
        */
        return tileEntity;
    }

    private static void activateAll(string constructor, object constructionData, GameObject tileEntity, GameObject fullLoadedContainer) {
        activeSoftLoaded(constructor, constructionData, tileEntity);
        activeFullLoaded(constructor, constructionData, fullLoadedContainer);
    }

    /// <summary>
    /// Will active a Tile Entity Property if it is required when soft loaded
    /// </summary>
    private static void activeSoftLoaded(string constructor, object constructionData, GameObject tileEntity) {
        switch (constructor) {
            case "storage":
                initStorage(deseralizeConstructionData(constructionData), tileEntity);
                break;
            case "machine":
                break;
            case "onClick":
                initOnClick(deseralizeConstructionData(constructionData),tileEntity);
                break;
        }
    }
    /// <summary>
    /// Will active a Tile Entity Property if it is required when full loaded (ie decorative)
    /// </summary>
    private static void activeFullLoaded(string constructor, object constructionData, GameObject fullLoadedContainer) {
        switch (constructor) {
            case "light":
                initLight(deseralizeConstructionData(constructionData), fullLoadedContainer);
                break;
        }
    }

    private static Dictionary<string, object> deseralizeConstructionData(object constructionData) {
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(constructionData.ToString());
    }

    private static void initLight(Dictionary<string, object> lightOptions, GameObject tileEntity) {
        Light2D light2D = tileEntity.AddComponent<Light2D>();
        light2D.lightType = Light2D.LightType.Point;
        light2D.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
        if (lightOptions.ContainsKey("color")) {
            int[] colorOptions = JsonConvert.DeserializeObject<int[]>(lightOptions["color"].ToString());
            light2D.color = new Color(colorOptions[0]/255f,colorOptions[1]/255f,colorOptions[2]/255f);
        }
        if (lightOptions.ContainsKey("strength")) {
            light2D.pointLightOuterRadius = JsonConvert.DeserializeObject<int>(lightOptions["strength"].ToString());
        }
    }

    private static void initStorage(Dictionary<string, object> storageOptions, GameObject tileEntity) {
        TileEntityStorageProperties storageProperties = tileEntity.AddComponent<TileEntityStorageProperties>();
        if (storageOptions.ContainsKey("items")) {
            List<List<List<int>>> SeralizedItemInventories = JsonConvert.DeserializeObject<List<List<List<int>>>>(storageOptions["items"].ToString());
            Debug.Log(SeralizedItemInventories);
        }
    }

    private static void initOnClick(Dictionary<string, object> clickOptions, GameObject tileEntity) {
        /*
        if (tileEntity.GetComponent<OnTileEntityClickController>() != null) {
            return;
        }
        OnTileEntityClickController clickController = tileEntity.AddComponent<OnTileEntityClickController>();
        
        
        foreach (string key in clickOptions.Keys) {
            switch (key) {
            case "inventoryGUI":
                clickController.OnClick = new OnClickOpenGui(clickOptions[key].ToString());
                break;
        }
        }
        */
        
    }
    
}
