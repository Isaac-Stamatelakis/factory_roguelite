using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using TileEntity;
using TileEntity.Instances.Machines;
using Conduits.Ports;

public class TileEntityConduitGenerator : EditorWindow {
    private TileEntityObject tileEntity;
    private GameObject tilemapPrefab;
    [MenuItem("Tools/TileEntity/SetPorts")]
    public static void ShowWindow()
    {
        TileEntityConduitGenerator window = (TileEntityConduitGenerator)EditorWindow.GetWindow(typeof(TileEntityConduitGenerator));
        window.titleContent = new GUIContent("Tile Entity Conduit Generation");
    }

    void OnGUI()
    {
        tileEntity = EditorGUILayout.ObjectField("TileEntity", tileEntity, typeof(TileEntityObject), true) as TileEntityObject;
        tilemapPrefab = EditorGUILayout.ObjectField("TileMapPrefab", tilemapPrefab, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("Set Conduit Ports"))
        {
            set();
        }
    }

    void set()
    {
        Tilemap mainMap = tilemapPrefab.GetComponent<Tilemap>();
        BoundsInt bounds = mainMap.cellBounds;

        Vector3Int nullVect = new Vector3Int(-9999,-9999);
        Vector3Int center = nullVect;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                TileBase tileBase = mainMap.GetTile(new Vector3Int(x,y,0));
                if (tileBase != null) {
                    if (center != nullVect) {
                        Debug.LogError("More than one tile present in the main conduit port map");
                        return;
                    }
                    center = new Vector3Int(x,y,0);
                }
            }
        }
        Tile tile = (Tile) mainMap.GetTile(center);
        Vector2Int spriteSize = Global.getSpriteSize(tile.sprite);
        IConduitPortTileEntity machinePorts = (IConduitPortTileEntity) tileEntity;
        Vector2Int xArea = getArea(spriteSize.x);
        Vector2Int yArea = getArea(spriteSize.y);
        setPorts(machinePorts,tilemapPrefab.transform,ConduitType.Item,center,xArea,yArea);
        setPorts(machinePorts,tilemapPrefab.transform,ConduitType.Fluid,center,xArea,yArea);
        setPorts(machinePorts,tilemapPrefab.transform,ConduitType.Energy,center,xArea,yArea);
        setPorts(machinePorts,tilemapPrefab.transform,ConduitType.Signal,center,xArea,yArea);

        EditorUtility.SetDirty(tileEntity);
        AssetDatabase.SaveAssets();
    }

    private void setPorts(IConduitPortTileEntity machinePortInterface, Transform parent, ConduitType conduitType, Vector3Int center, Vector2Int xArea, Vector2Int yArea) {
        Transform childTransform = parent.Find(conduitType.ToString());
        if (childTransform == null) {
            Debug.LogError("No tilemap object for " + conduitType.ToString());
            return; 
        }
        Tilemap tilemap = childTransform.GetComponent<Tilemap>();
        
        List<TileEntityPortData> conduitPorts = new List<TileEntityPortData>();
        for (int x = xArea.x; x <= xArea.y; x++) {
            for (int y = yArea.x; y <= yArea.y; y++) {
                TileBase tileBase = tilemap.GetTile(new Vector3Int(x,y,0)+center);
                if (tileBase != null) {
                    StandardTile standardTile = (StandardTile) tileBase;
                    TileEntityPortData conduitPortDataData;
                    switch (standardTile.id) {
                        case "All":
                            conduitPortDataData = new TileEntityPortData(EntityPortType.All,new Vector2Int(x,y));
                            break;
                        case "Input":
                            conduitPortDataData = new TileEntityPortData(EntityPortType.Input,new Vector2Int(x,y));
                            break;
                        case "Output":
                            conduitPortDataData = new TileEntityPortData(EntityPortType.Output,new Vector2Int(x,y));
                            break;
                    }
                }
            }
        }
        Debug.Log(conduitPorts.Count + " " + conduitType.ToString() + " Conduit Ports set for " + tileEntity.name);
    }


    private Vector2Int getArea(int size) {
        return new Vector2Int(Mathf.CeilToInt((float) size/2)-1, Mathf.FloorToInt((float) size/2));
    }

    
}
