using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TileEntityStorageProperties : MonoBehaviour, SeralizableTileOption
{
    [SerializeField] protected List<AssemblyInstruction> assemblyInstructions;
    private Dictionary<string, object> storageContainers = new Dictionary<string, object>();
    public List<string> containerNames {get{
        List<string> strings = new List<string>();
        foreach (string key in storageContainers.Keys) {
            strings.Add(key);
        }
        return strings;
        }
    }
    // Start is called before the first frame update
    void Start() {
        foreach (AssemblyInstruction assemblyInstruction in assemblyInstructions) {
            if (assemblyInstruction.type == 0) {
                ItemContainer ItemContainer = new ItemContainer(new List<Matter>());
                for (int n = 0; n < assemblyInstruction.size; n ++) {
                    ItemContainer.add(null);
                }
                storageContainers[assemblyInstruction.name] = ItemContainer;
            } else if (assemblyInstruction.type == 1) {
                FluidContainer fluidMatterContainer = new FluidContainer(new List<Matter>());
                for (int n = 0; n < assemblyInstruction.size; n ++) {
                    fluidMatterContainer.add(null);
                }
                storageContainers[assemblyInstruction.name] = fluidMatterContainer;
            } else if (assemblyInstruction.type == 2) {
                List<int> energies = new List<int>();
                for (int n = 0; n < assemblyInstruction.size; n ++) {
                    energies.Add(0);
                }
                storageContainers[assemblyInstruction.name] = energies;
            }
        }   
    }

    public object getContainer(string key) {
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
        if (storageDictionary.ContainsKey("items")) {
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
