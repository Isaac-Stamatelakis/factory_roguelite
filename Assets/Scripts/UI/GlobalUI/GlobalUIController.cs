using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Conduits.Ports;


public class GlobalUIController : MonoBehaviour
{
    
    public bool isActive {get => transform.childCount != 0;}
    // Start is called before the first frame update
    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) {
            removeGUI();
        }
    }

    public void removeGUI() {
        GlobalHelper.deleteAllChildren(transform);
    }
    public void setGUI(GameObject gui) {
        GlobalHelper.deleteAllChildren(transform);
        gui.transform.SetParent(transform,false);
    }

    public void addGUI(GameObject gui) {
        gui.transform.SetParent(transform,false);
    }

}

