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
        [SerializeField] public ArrowProgressController arrowProgressController;
        private GameObject slotPrefab;
        private Tier tier;
        private StandardMachineInventory machineInventory;
        // Start is called before the first frame update
        public void Update() {
            setEnergyBar();
        }
        public void displayMachine(MachineInventoryLayout layout, StandardMachineInventory machineInventory, string machineName, Tier tier) {
            machineInventory.display(layout,transform);
            this.machineInventory = machineInventory;
            this.tier = tier;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        

        
        private void setEnergyBar() {
            energyBar.value = ((float) machineInventory.Energy)/tier.getEnergyStorage();
        }

        private void setArrow() {
            //arrowProgressController.setArrow()
        }
    }

    public static class MachineUIFactory {
        private static GameObject solidSlotPrefab = Resources.Load<GameObject>("Prefabs/GUI/ItemInventorySlot");
        private static GameObject fluidSlotPrefab = Resources.Load<GameObject>("Prefabs/GUI/FluidInventorySlot");
        public static void initInventory(List<ItemSlot> items, List<Vector2Int> layoutVectors, ItemState itemState, string containerName, Transform transform) {
            if (items == null) {
                return;
            }
    
            GameObject inventoryContainer = new GameObject();
            ILoadableInventory inventoryUI = null;
            switch (itemState) {
                case ItemState.Solid:
                    inventoryUI = inventoryContainer.AddComponent<SolidItemInventory>();
                    break;
                case ItemState.Fluid:
                    inventoryUI = inventoryContainer.AddComponent<FluidInventoryGrid>();
                    break;
            }
            if (inventoryUI == null) {
                Debug.LogError("Could not init inventory for state " + itemState + " as uiinventory is null");
                return;
            }
            
            int index = 0;
            foreach (Vector2Int vector in layoutVectors) {
                GameObject slot = null;
                switch (itemState) {
                    case ItemState.Solid:
                        slot = GameObject.Instantiate(solidSlotPrefab);
                        break;
                    case ItemState.Fluid:
                        slot = GameObject.Instantiate(fluidSlotPrefab);
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
            inventoryContainer.transform.SetParent(transform);
            inventoryContainer.name = containerName;
            inventoryUI.initalize(items);
        }

        public static string formatMachineName(string name) {
            return name.Replace("E~","").Replace("(Clone)","").Replace("_"," ");
        }
    }

}
