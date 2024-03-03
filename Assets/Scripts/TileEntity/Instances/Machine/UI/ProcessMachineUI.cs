using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TileEntityModule.Instances.Machines {
    public class ProcessMachineUI : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public Slider energyBar;
        private GameObject slotPrefab;
        private Tier tier;
        private ProcessingMachineInventory machineInventory;
        // Start is called before the first frame update
        public void Update() {
            setEnergyBar();
        }
        public void displayMachine(MachineInventoryLayout layout, ProcessingMachineInventory machineInventory, string machineName, Tier tier) {
            machineInventory.display(layout,transform);
            this.machineInventory = machineInventory;
            this.tier = tier;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        

        
        private void setEnergyBar() {
            //energyBar.value = ((float) machineInventory.Energy)/tier.getEnergyStorage();
        }
    }

    public static class MachineUIFactory {
        private static GameObject solidSlotPrefab = Resources.Load<GameObject>("Prefabs/GUI/ItemInventorySlot");
        public static void initInventory(List<ItemSlot> items, List<Vector2Int> layoutVectors, ItemState itemState, string containerName, Transform transform) {
            if (items == null) {
                return;
            }
            GameObject inventoryContainer = new GameObject();
            NonGridInventory inventoryUI = inventoryContainer.AddComponent<NonGridInventory>();
            
            int index = 0;
            foreach (Vector2Int vector in layoutVectors) {
                GameObject slot = null;
                switch (itemState) {
                    case ItemState.Solid:
                        slot = GameObject.Instantiate(solidSlotPrefab);
                        break;
                    case ItemState.Fluid:
                        slot = GameObject.Instantiate(solidSlotPrefab);
                        break;
                }
                if (slot == null) {
                    Debug.LogError("Tried to init inventory with null slot " + containerName);
                    continue;
                }
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

        public static string formatMachineName(string name) {
            return name.Replace("E~","").Replace("(Clone)","").Replace("_"," ");
        }
    }

}
