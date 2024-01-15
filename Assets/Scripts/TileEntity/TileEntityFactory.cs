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
    public static void createTileEntity(Dictionary<string,object> tileEntityOptions, Transform tileEntityContainer, string tileContainerName, Vector2Int placePosition) {
        if (tileEntityOptions == null || tileEntityOptions.Keys.Count == 0) {
            return;
        }
        
        GameObject tileEntity = initalizeTileEntityGameObject(tileEntityOptions, tileEntityContainer,tileContainerName,placePosition);
        /*
        GameObject fullLoadedContainer = createFullLoadedContainer(tileEntity);
        foreach (string constructor in tileEntityOptions.Keys) {
            activateAll(constructor, tileEntityOptions.get(constructor), tileEntity,fullLoadedContainer);
        }
        */
        
    }
    
    public static void softLoadTileEntity(Dictionary<string,object> tileEntityOptions, Transform tileEntityContainer, string tileContainerName, Vector2Int placePosition) {
        if (tileEntityOptions.Count == 0) {
            return;
        }
        
        GameObject tileEntity = initalizeTileEntityGameObject(tileEntityOptions, tileEntityContainer,tileContainerName,placePosition);
        foreach (string constructor in tileEntityOptions.Keys) {
            //activeSoftLoaded(constructor, tileEntityOptions.get(constructor), tileEntity);
        }
    }

    /// <summary>
    /// loads all tileEntityProperties which are only required when a chunk is full loaded
    /// Must be soft loaded first in order to be full loaded
    /// </summary>
    public static void fullLoadTileEntity(GameObject tileEntity) {
        if (Global.findChild(tileEntity.transform, "FullLoadedContainer") != null) {
            return;
        }
        GameObject fullLoadedContainer = createFullLoadedContainer(tileEntity);
        SDictionary tileEntityOptions = tileEntity.GetComponent<TileEntityProperties>().TileEntityOptions;

        foreach (string constructor in tileEntityOptions.Keys) {
            activeFullLoaded(constructor, tileEntityOptions.get(constructor), fullLoadedContainer);
        }
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

    private static GameObject initalizeTileEntityGameObject(Dictionary<string,object> assemblyDirections, Transform tileEntityContainer, string tileContainerName, Vector2Int placePosition) {
        if (assemblyDirections == null) {
            return null;
        }
        GameObject tileEntity = null;
        if (assemblyDirections.ContainsKey("path")) {
            string folderPath = "Prefabs/TileEntities/" + (string) assemblyDirections["path"] + "/";
            tileEntity = GameObject.Instantiate(Resources.Load<GameObject>(folderPath + "TileEntity"));
            tileEntity.transform.SetParent(tileEntityContainer);
            tileEntity.transform.localPosition = new Vector3(placePosition.x/2f,placePosition.y/2f,0);
            TileEntityProperties tileEntityProperties = tileEntity.GetComponent<TileEntityProperties>();
            tileEntityProperties.TileContainerName = tileContainerName;
            tileEntityProperties.TileEntityOptions = new SDictionary();
            tileEntityProperties.Position = placePosition;
            tileEntity.name = tileContainerName + "[" + placePosition.x + "," + placePosition.y +"]";
            TileEntityStorageProperties tileEntityStorageProperties = tileEntity.GetComponent<TileEntityStorageProperties>();
            if (tileEntityStorageProperties != null) {
                tileEntityProperties.TileEntityOptions.addDynamic("storage", null);
            }
        }
        /*
        GameObject tileEntity = new GameObject();
        TileEntityProperties tileEntityProperties = tileEntity.AddComponent<TileEntityProperties>();
        tileEntityProperties.TileEntityOptions = tileEntityOptions;
        tileEntity.transform.SetParent(tileEntityContainer);
        tileEntity.transform.localPosition = new Vector3(placePosition.x/2f,placePosition.y/2f,0);
        tileEntity.name = tileContainerName + "[" + placePosition.x + "," + placePosition.y +"]";
        tileEntityProperties.TileContainerName = tileContainerName;
        tileEntityProperties.Position = placePosition;
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
