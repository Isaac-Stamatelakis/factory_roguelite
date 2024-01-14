using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickOpenGui : OnClick
{
    private string path;
    GameObject guiGameObject = null;
    public OnClickOpenGui(string path) {
        this.path = path;
    }
    public override void clicked(GameObject tileEntity)
    {
        if (guiGameObject != null) {
            GameObject.Destroy(guiGameObject);
            guiGameObject = null;
            return;
        }
        TileEntityStorageProperties storageProperties = tileEntity.GetComponent<TileEntityStorageProperties>();
        if (storageProperties == null) {
            Debug.LogError("Tried to load gui without inventory");
            return;
        }
        TileEntityGUIController tileEntityGUIController = GameObject.Find("TileEntityGUIController").GetComponent<TileEntityGUIController>();
        if (tileEntityGUIController == null) {
            Debug.LogError("TileEntityGUIController is null somehow");
            return;
        }
        tileEntityGUIController.toggleGUI(path,tileEntity);
    }
}
