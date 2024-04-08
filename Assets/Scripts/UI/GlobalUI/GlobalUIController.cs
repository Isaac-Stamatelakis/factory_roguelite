using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ConduitModule.Ports;


public class GlobalUIController : MonoBehaviour
{
    private GameObject GUIGameObject = null;
    public bool isActive {get{return this.GUIGameObject != null;}}
    // Start is called before the first frame update
    // Update is called once per frame
    public void Update()
    {
        if (GUIGameObject != null) {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) {
                removeGUI();
            }
        }
    }

    public void removeGUI() {
        if (this.GUIGameObject == null) {
            return;
        }
        GameObject.Destroy(GUIGameObject);
        GUIGameObject = null;
        
    }
    public void setGUI(GameObject gui) {
        removeGUI();
        if (gui == GUIGameObject) {
            GUIGameObject = null;
            return;
        }
        
        this.GUIGameObject = gui;
        GUIGameObject.transform.SetParent(transform,false);
    }

}

