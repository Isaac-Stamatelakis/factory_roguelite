using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegisterLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        // Loads in all items
        ItemObject[] items = Resources.LoadAll<ItemObject>("");
        foreach (ItemObject itemObject in items) {
            print(itemObject.name);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/// Singleton
public class ItemRegister {
    private static Dictionary<string,ItemObject> items;
    private static ItemRegister instance;
    private ItemRegister() {
        ItemObject[] itemObjects = Resources.LoadAll<ItemObject>("");
        foreach (ItemObject itemObject in itemObjects) {
            if (!items.ContainsKey(itemObject.id)) {
                items[itemObject.id] = itemObject;
            } else {
                ItemObject contained = items[itemObject.id];
                Debug.LogError("Duplicate id for objects " + contained.name + " and " + itemObject.name + " with id: " + itemObject.id);
            }
        }

    }

    public static ItemRegister getInstance() {
        if (instance == null) {
            instance = new ItemRegister();
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

    public ItemObject getItemObject(string id) {
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
}
