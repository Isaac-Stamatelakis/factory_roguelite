using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using TileEntity.Instances.CompactMachine.UI.Selector;
using TileEntity.Instances.CompactMachines;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace TileEntity.Instances.CompactMachine.BluePrinter
{
    public class CompactMachineBluePrinterUI : MonoBehaviour, ITileEntityUI<CompactMachineBluePrinterInstance>
    {
        [SerializeField] private InventoryUI mCompactMachineInput;
        [SerializeField] private InventoryUI mCompactMachineOutput;
        [SerializeField] private InventoryUI mCostInventory;
        [SerializeField] private InventoryUI mItemInput;
        [SerializeField] private Button mSelectButton;
        [SerializeField] private Button mCraftButton;

        [SerializeField] private AssetReference mSelectorAssetReference;
        private CompactMachineBluePrinterInstance bluePrinterInstance;

        private CompactMachineHashSelector mSelectorPrefab;
        private CompactMachineCostCalculator costCalculator;
        public void Start()
        {
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(mSelectorAssetReference);
            yield return handle;
            mSelectorPrefab = handle.Result.GetComponent<CompactMachineHashSelector>();
            Addressables.Release(handle);
        }

        public void DisplayTileEntityInstance(CompactMachineBluePrinterInstance tileEntityInstance)
        {
            bluePrinterInstance = tileEntityInstance;
            var inventory = bluePrinterInstance.BluePrintInventory;
            mCompactMachineInput.AddTagRestriction(ItemTag.CompactMachine);
            mCompactMachineInput.DisplayInventory(inventory.CompactMachineInput);
            
            mCompactMachineOutput.SetInteractMode(InventoryInteractMode.BlockInput);
            mCompactMachineOutput.DisplayInventory(inventory.CompactMachineOutput);
            
            mItemInput.DisplayInventory(inventory.ItemInput);
            
            mCostInventory.SetInteractMode(InventoryInteractMode.Recipe);
            
            mCraftButton.onClick.AddListener(TryCraft);
            mSelectButton.onClick.AddListener(DisplaySelector);

            DisplayCost();
        }

        private void TryCraft()
        {
            if (costCalculator == null) return;
            
            if (bluePrinterInstance.BluePrintInventory.CompactMachineInput.Count == 0 || bluePrinterInstance.BluePrintInventory.CompactMachineOutput.Count == 0) return;
            
            ItemSlot inputSlot = bluePrinterInstance.BluePrintInventory.CompactMachineInput[0];
            ItemSlot outputSlot = bluePrinterInstance.BluePrintInventory.CompactMachineOutput[0];
            if (ItemSlotUtils.IsItemSlotNull(inputSlot)) return;
            var metaData = CompactMachineUtils.GetMetaDataFromHash(bluePrinterInstance.BluePrintInventory.CurrentHash);
            
            if (!CanOutput(outputSlot, bluePrinterInstance.BluePrintInventory.CurrentHash, metaData.TileID)) return;
            
            Dictionary<string, uint> inputDict = ItemSlotUtils.ToDict(bluePrinterInstance.BluePrintInventory.ItemInput);
            Dictionary<string, uint> costDict = costCalculator.GetCostDict();
            
            foreach (string key in costDict.Keys)
            {
                if (!inputDict.ContainsKey(key) || inputDict[key] < costDict[key]) return;
            }
            
            
            if (ItemSlotUtils.IsItemSlotNull(outputSlot))
            {
                ItemSlot output = new ItemSlot(inputSlot.itemObject, 1, null);
                ItemSlotUtils.AddTag(output,ItemTag.CompactMachine,bluePrinterInstance.BluePrintInventory.CurrentHash);
                bluePrinterInstance.BluePrintInventory.CompactMachineOutput[0] = output;
            }
            else
            {
                outputSlot.amount++;
            }
            
            inputSlot.amount--;
            mItemInput.RefreshSlots();
            mCompactMachineOutput.RefreshSlots();
            mCompactMachineInput.RefreshSlots();

            Dictionary<string, uint> tempCostDict = new Dictionary<string, uint>();
            foreach (string key in costDict.Keys)
            {
                tempCostDict.Add(key, costDict[key]);
            }
            foreach (ItemSlot itemSlot in bluePrinterInstance.BluePrintInventory.ItemInput)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot) || !tempCostDict.TryGetValue(itemSlot.itemObject.id, out uint cost)) continue;
                if (cost == 0) continue;
                if (itemSlot.amount <= cost)
                {
                    cost -= itemSlot.amount;
                    itemSlot.amount = 0;
                }
                else
                {
                    itemSlot.amount -= cost;
                    cost = 0;
                }
                tempCostDict[itemSlot.itemObject.id] = cost;
            }
        }

        private bool CanOutput(ItemSlot outputSlot, string hash, string inputID)
        {
            if (ItemSlotUtils.IsItemSlotNull(outputSlot)) return true;
            if (outputSlot.itemObject.id != inputID || outputSlot.amount >= Global.MAX_SIZE) return false;
            var tagValue = outputSlot.tags?.Dict?.GetValueOrDefault(ItemTag.CompactMachine) as string;
            return tagValue == hash;
        }

        private void DisplaySelector()
        {
            CompactMachineHashSelector selector = GameObject.Instantiate(mSelectorPrefab);
            selector.Display(OnHashSelect);
            CanvasController.Instance.DisplayObject(selector.gameObject);
        }

        private void OnHashSelect(string hash)
        {
            bluePrinterInstance.BluePrintInventory.CurrentHash = hash;
            DisplayCost();
        }

        private void DisplayCost()
        {
            string hash = bluePrinterInstance.BluePrintInventory.CurrentHash;
            if (hash == null) return;
            var metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
            if (metaData == null) return;
            
            mSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Currently Selected: {hash}";
            costCalculator = new CompactMachineCostCalculator(hash);
            costCalculator.Calculate();

            Dictionary<string, uint> costDict = costCalculator.GetCostDict();

            List<ItemSlot> cost = ItemSlotUtils.FromDict(costDict,Global.MAX_SIZE);
            mCostInventory.DisplayInventory(cost);
        }
    }
}
