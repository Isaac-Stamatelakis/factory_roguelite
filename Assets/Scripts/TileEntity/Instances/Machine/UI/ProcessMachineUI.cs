using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule.Inventory;
using RecipeModule;

namespace TileEntityModule.Instances.Machines {
    public class ProcessMachineUI : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public Slider energyBar;
        [SerializeField] public ArrowProgressController arrowProgressController;
        private GameObject slotPrefab;
        private Tier tier;
        private StandardMachineInventory machineInventory;
        private InventoryUIMode mode;
        public void Update() {
            setEnergyBar();
        }
        public void displayMachine(IDisplayableLayout<StandardSolidAndFluidInventory> layout, StandardMachineInventory machineInventory, string machineName, Tier tier) {
            layout.display(transform,machineInventory,InventoryUIMode.Standard);
            this.machineInventory = machineInventory;
            this.tier = tier;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        public void displayRecipe(IDisplayableLayout<StandardSolidAndFluidInventory> layout, StandardMachineInventory machineInventory, string machineName) {
            this.machineInventory = machineInventory;
            layout.display(transform,machineInventory,InventoryUIMode.Recipe);
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        private void setEnergyBar() {
            if (machineInventory == null) {
                return;
            }
            energyBar.value = ((float) machineInventory.Energy)/tier.getEnergyStorage();
        }

        private void setArrow() {
            //arrowProgressController.setArrow()
        }
    }

    public static class MachineUIFactory {
        public static void initInventory(List<ItemSlot> items, List<Vector2Int> layoutVectors, ItemState itemState, string containerName, Transform transform, InventoryUIMode type) {
            if (items == null) {
                return;
            }
    
            GameObject inventoryContainer = new GameObject();
            ILoadableInventory inventoryUI = null;
            switch (itemState) {
                case ItemState.Solid:
                    switch (type) {
                        case InventoryUIMode.Standard:
                            inventoryUI = inventoryContainer.AddComponent<SolidItemInventory>();
                            break;
                        case InventoryUIMode.Recipe:
                            inventoryUI = inventoryContainer.AddComponent<RecipeInventoryUI>();
                            break;
                    }
                    break;
                case ItemState.Fluid:
                    switch (type) {
                        case InventoryUIMode.Standard:
                            inventoryUI = inventoryContainer.AddComponent<FluidInventoryGrid>();
                            break;
                        case InventoryUIMode.Recipe:
                            inventoryUI = inventoryContainer.AddComponent<RecipeInventoryUI>();
                            break;
                    }
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
                        slot = GameObject.Instantiate(Resources.Load<GameObject>(InventoryHelper.SolidSlotPrefabPath));
                        break;
                    case ItemState.Fluid:
                        slot = GameObject.Instantiate(Resources.Load<GameObject>(InventoryHelper.FluidSlotPrefabPath));
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
