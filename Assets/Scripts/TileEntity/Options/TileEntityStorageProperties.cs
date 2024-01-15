using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TileEntityStorageProperties : MonoBehaviour, SeralizableTileOption
{
    [SerializeField] protected List<AssemblyInstruction> assemblyInstructions;
    private Dictionary<string, List<Dictionary<string, object>>> storageContainers = new Dictionary<string, List<Dictionary<string, object>>>();
    public List<string> containerNames {get{
        List<string> strings = new List<string>();
        foreach (string key in storageContainers.Keys) {
            strings.Add(key);
        }
        return strings;
        }
    }
    
    void Start() {
        foreach (AssemblyInstruction assemblyInstruction in assemblyInstructions) {
            List<Dictionary<string,object>> container = new List<Dictionary<string, object>>();
            for (int n = 0; n < assemblyInstruction.size; n ++) { 
                container.Add(null);
            }
            storageContainers[assemblyInstruction.name] = container;
        }   
    }

    public List<Dictionary<string,object>> getContainer(string key) {
        if (storageContainers.ContainsKey(key)) {
            return storageContainers[key];
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void seralize() {
        /*
        Dictionary<string,object> seralizedDict = new Dictionary<string, object>();
        if (storageContainers.ContainsKey("items")) {
            List<List<List<int>>> seralizedInventories = new List<List<List<int>>>();
            foreach (ItemInventory itemInventory in (List<ItemInventory>) storageDictionary["items"]) {
                List<List<int>> seralizedItems = new List<List<int>>();
                List<int> inventorySize = new List<int>();
                inventorySize.Add(itemInventory.Size.x);
                inventorySize.Add(itemInventory.Size.y);
                seralizedItems.Add(inventorySize);
                foreach (ItemInventoryData itemInventoryData in itemInventory.Inventory) {
                    List<int> seralizedItem = new List<int>();
                    if (itemInventoryData == null) {
                        seralizedItem.Add(-1);
                        seralizedItem.Add(-1);
                    } else {
                        seralizedItem.Add(itemInventoryData.id);
                        seralizedItem.Add(itemInventoryData.amount);
                    }   
                    seralizedItems.Add(seralizedItem);
                    
                }
                seralizedInventories.Add(seralizedItems);
            }
            seralizedDict["items"] = seralizedInventories;
        }
        TileEntityProperties tileEntityProperties = GetComponent<TileEntityProperties>();
        tileEntityProperties.TileEntityOptions.set("storage", seralizedDict);
        */
        TileEntityProperties tileEntityProperties = GetComponent<TileEntityProperties>();
        tileEntityProperties.TileEntityOptions.set("storage", storageContainers);
    }
    [System.Serializable]
    protected class AssemblyInstruction {
        public uint size;
        [Tooltip("0:item\n1:fluid\n2:energy")]
        public uint type;
        [Tooltip("Will be linked to inventorygrid")]
        public string name;
    }    
}
