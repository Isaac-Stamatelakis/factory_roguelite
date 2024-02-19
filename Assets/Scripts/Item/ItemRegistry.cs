using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using RobotModule;
using ItemModule;
using TileEntityModule;
using ConduitModule.Ports;

/// Singleton
public class ItemRegistry {
    private static Dictionary<string,ItemObject> items;
    private static ItemRegistry instance;
    private ItemRegistry() {
        items = new Dictionary<string, ItemObject>();
        ItemObject[] itemObjects = Resources.LoadAll<ItemObject>("");
        foreach (ItemObject itemObject in itemObjects) {
            addToDict(itemObject);
        }
        Debug.Log("Item registry loaded  " + items.Count + " items");
        RecipeRegistry.getInstance();
    }

    private bool addToDict(ItemObject itemObject) {
        if (!items.ContainsKey(itemObject.id)) {
            items[itemObject.id] = itemObject;
            return true;
        } else {
            ItemObject contained = items[itemObject.id];
            Debug.LogError("Duplicate id for objects " + contained.name + " and " + itemObject.name + " with id: " + itemObject.id);
            return false;
        }
    }
    public static ItemRegistry getInstance() {
        if (instance == null) {
            instance = new ItemRegistry();
        }
        return instance;
    }
    ///
    /// Returns tileItem if id maps to tile item, null otherwise
    ///
    public TileItem getTileItem(string id) {
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is TileItem) {
            return (TileItem) itemObject;
        } else {
            return null;
        }
    }

    public RobotItem GetRobotItem(string id) {
        if (id == null) {
            return null;
        }
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is RobotItem) {
            return (RobotItem) itemObject;
        }
        return null;
    }

    public TransmutableItemObject getTransmutableItemObject(string id) {
        if (id == null) {
            return null;
        }
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is TransmutableItemObject) {
            return (TransmutableItemObject) itemObject;
        }
        return null;
    }
    public ItemObject getItemObject(string id) {
        if (id == null) {
            return null;
        }
        if (!items.ContainsKey(id)) {
            return null;
        }
        return items[id];   
    }
    ///
    /// Returns ConduitItem if id maps to ConduitItem, null otherwise
    ///
    public ConduitItem GetConduitItem(string id) {
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is ConduitItem) {
            return (ConduitItem) itemObject;
        } else {
            return null;
        }
    }

    public List<ItemObject> query(string serach) {
        List<ItemObject> queried = new List<ItemObject>();
        foreach (ItemObject itemObject in items.Values) {
            if (itemObject.name.ToLower().Contains(serach.ToLower())) {
                queried.Add(itemObject);
            }
        }
        return queried;
    }
}
