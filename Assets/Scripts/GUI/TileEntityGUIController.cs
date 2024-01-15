using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityGUIController : MonoBehaviour
{
    private string loadedPath = null;
    private GameObject GUIGameObject = null;
    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    public void toggleGUI(string path, GameObject tileEntity) {
        if (loadedPath == path) {
            removeGUI();
        } else {
            if (loadedPath != null) {
                removeGUI();
            }
            setGUI(path,tileEntity);
        }
    }

    public void removeGUI() {
        loadedPath = null;
        if (this.GUIGameObject == null) {
            return;
        }
        GameObject.Destroy(GUIGameObject);
        GUIGameObject = null;
        
    }

    public void setGUI(string path,GameObject tileEntity) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/GUI/" + path);
        if (prefab == null) {
            loadedPath = null;
            Debug.LogError("Path at " + path +" did not reference a gameobject");
            return;
        }
        loadedPath = path;
        this.GUIGameObject = GameObject.Instantiate(prefab);
        GUIGameObject.transform.SetParent(transform);
        TileEntityStorageProperties tileEntityStorageProperties = tileEntity.GetComponent<TileEntityStorageProperties>();
        foreach (string storageContainerName in tileEntityStorageProperties.containerNames) {
            GameObject gui = Global.findChild(GUIGameObject.transform,"GUI");
            GameObject container = Global.findChild(gui.transform,storageContainerName);
            
            if (container == null) {
                continue;
            }
            InventoryGrid[] inventoryGrids = container.GetComponents<InventoryGrid>();
            if (inventoryGrids.Length > 0) {
                InventoryGrid inventoryGrid = inventoryGrids[0];
                if (inventoryGrid is ItemInventoryGrid) {
                    inventoryGrid.initalize(tileEntityStorageProperties.getContainer(storageContainerName));
                }

            }

        }
        
    }


}
