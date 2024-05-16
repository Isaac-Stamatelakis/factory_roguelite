using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TileEntityModule;
using Items.Inventory;

public static class ProcessorUIFactory
{
    public static T getMachineUI<T>(GameObject uiPrefab, InventoryLayout layout, string name) {
        if (uiPrefab == null) {
            throw new InvalidOperationException(name + " uiPrefab is null");
        }
        GameObject instantiatedUI = GameObject.Instantiate(uiPrefab);
        T machineUI = instantiatedUI.GetComponent<T>();
        if (machineUI == null) {
            throw new InvalidOperationException(name + " machineUI is null");
        }
        return machineUI;  
    }
}
