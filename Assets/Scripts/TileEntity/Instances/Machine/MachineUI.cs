using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TileEntityModule.Instances.Machine {
    public class MachineUI : MonoBehaviour
    {
        GameObject slotPrefab;
        // Start is called before the first frame update
        void Awake()
        {
            slotPrefab = Resources.Load<GameObject>("Prefabs/GUI/ItemInventorySlot");
        }
        public void displayMachine(MachineLayout layout, List<ItemSlot> inputs, List<ItemSlot> outputs, List<ItemSlot> other,string machineName) {
            initInventory(inputs,layout.inputs,"Inputs");
            initInventory(outputs,layout.outputs,"Outputs");
            List<Vector2Int> otherLayout = new List<Vector2Int>{
                new Vector2Int(0,-200)
            };
            initInventory(other,otherLayout,"Other");
            Transform titleTransform = transform.Find("Title");
            if (titleTransform != null) {
                TextMeshProUGUI textMeshPro = titleTransform.GetComponent<TextMeshProUGUI>();
                string machineNameNoJunk = machineName.Replace("E~","").Replace("(Clone)","").Replace("_"," ");
                textMeshPro.text=machineNameNoJunk;
            }
        }

        private void initInventory(List<ItemSlot> items, List<Vector2Int> layoutVectors, string containerName) {
            GameObject inventoryContainer = new GameObject();
            NonGridInventory inventoryUI = inventoryContainer.AddComponent<NonGridInventory>();
            
            int index = 0;
            foreach (Vector2Int vector in layoutVectors) {
                GameObject slot = GameObject.Instantiate(slotPrefab);
                slot.name = "slot" + index;
                slot.transform.SetParent(inventoryContainer.transform);
                RectTransform rectTransform = slot.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(64,64);
                rectTransform.transform.position = new Vector3(vector.x,vector.y,0);
                index ++;
            }
            inventoryUI.transform.SetParent(transform);
            inventoryUI.name = containerName;
            inventoryUI.initalize(items);
        }

        
    }

}
