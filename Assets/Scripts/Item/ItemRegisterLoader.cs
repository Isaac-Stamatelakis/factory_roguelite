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
    private Dictionary<string,ItemObject> items;
    private ItemRegister instance;
    private ItemRegister() {
        ItemObject[] items = Resources.LoadAll<ItemObject>("");
        foreach (ItemObject itemObject in items) {
            Debug.Log(itemObject.name);
        }
    }

    public ItemRegister getInstance() {
        if (instance == null) {
            instance = new ItemRegister();
        }
        return instance;
    }
}
