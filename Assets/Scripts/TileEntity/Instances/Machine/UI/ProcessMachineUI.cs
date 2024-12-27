using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items.Inventory;
using RecipeModule;
using System;
using Items;

namespace TileEntity.Instances.Machines {
    public class ProcessMachineUI : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public Slider energyBar;
        [SerializeField] public ArrowProgressController arrowProgressController;
        [SerializeField] public Image panel;
        private GameObject slotPrefab;
        private Tier tier;
        public void Update() {
            //setEnergyBar();
        }
        /*
        public void displayMachine(IDisplayableLayout<StandardSolidAndFluidInventory> layout, StandardMachineInventory machineInventory, string machineName, Tier tier, IInventoryListener listener) {
            layout.display(transform,machineInventory,InventoryUIMode.Standard,listener);
            this.machineInventory = machineInventory;
            this.tier = tier;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        public void displayRecipe(IDisplayableLayout<StandardSolidAndFluidInventory> layout, StandardMachineInventory machineInventory, string machineName) {
            this.machineInventory = machineInventory;
            layout.display(transform,machineInventory,InventoryUIMode.Recipe,null);
            title.text = MachineUIFactory.formatMachineName(machineName);
            energyBar.gameObject.SetActive(false);
            panel.enabled = false;
        }
        */
        /*
        private void setEnergyBar() {
            if (machineInventory == null) {
                return;
            }
            energyBar.value = ((float) machineInventory.Energy)/tier.GetEnergyStorage();
        }
        */

        private void setArrow() {
            //arrowProgressController.setArrow()
        }
    }
    /*
    public static class MachineUIFactory {
        public static void initInventory(List<ItemSlot> items, List<Vector2Int> layoutVectors, ItemState itemState, string containerName, Transform transform, InventoryUIMode type, IInventoryListener listener) {
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
            if (listener != null && inventoryUI is AbstractSolidItemInventory solidItemInventory) {
                solidItemInventory.addListener(listener);
            }
            
            int index = 0;
            foreach (Vector2Int vector in layoutVectors) {
                ItemSlotUI slot = ItemSlotUIFactory.newItemSlotUI(null,inventoryContainer.transform,itemState.getSlotColor(),suffix:index.ToString());
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

        public static ProcessMachineUI getProcessMachineRecipeUI(GameObject uiPrefab, InventoryLayout layout, IRecipe recipe, string name) {
            ProcessMachineUI machineUI = getProcessMachineUI(uiPrefab,layout,name);
            StandardMachineInventory machineInventory = (StandardMachineInventory) RecipeInventoryFactory.toStandard(recipe);
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                throw new InvalidOperationException(name + " layout is not standard layout");
            }
            machineUI.displayRecipe(standardLayout, machineInventory, name);
            return machineUI;
        }

        public static ProcessMachineUI getProcessMachineStandardUI(GameObject uiPrefab, InventoryLayout layout, StandardMachineInventory inventory, Tier tier, string name, IInventoryListener listener) {
            ProcessMachineUI machineUI = getProcessMachineUI(uiPrefab,layout,name);
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                throw new InvalidOperationException(name + " layout is not standard layout");
            }
            machineUI.displayMachine(standardLayout, inventory, name, tier,listener);
            return machineUI;
        }

        private static ProcessMachineUI getProcessMachineUI(GameObject uiPrefab, InventoryLayout layout, string name) {
            if (uiPrefab == null) {
                Debug.LogError("GUI GameObject for Processor:" + name + " is null");
                return null;
            }
            GameObject instantiatedUI = GameObject.Instantiate(uiPrefab);
            ProcessMachineUI machineUI = instantiatedUI.GetComponent<ProcessMachineUI>();
            if (machineUI == null) {
                throw new InvalidOperationException(name + " machineUI is null");
            }
            return machineUI;  
        }
    }
    */

}
