using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
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
        private string currentHash;
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
            
        }

        private void TryCraft()
        {
            
        }

        private void DisplaySelector()
        {
            CompactMachineHashSelector selector = GameObject.Instantiate(mSelectorPrefab);
            selector.Display(OnHashSelect);
            CanvasController.Instance.DisplayObject(selector.gameObject);
        }

        private void OnHashSelect(string hash)
        {
            currentHash = hash;
            mSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Currently Selected: {hash}";
            CompactMachineCostCalculator costCalculator = new CompactMachineCostCalculator();

            List<ItemSlot> cost = costCalculator.GetCost(hash);
            mCostInventory.DisplayInventory(cost);
        }
    }
}
