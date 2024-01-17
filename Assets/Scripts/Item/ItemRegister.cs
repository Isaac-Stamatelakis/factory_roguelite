using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegister : MonoBehaviour
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
