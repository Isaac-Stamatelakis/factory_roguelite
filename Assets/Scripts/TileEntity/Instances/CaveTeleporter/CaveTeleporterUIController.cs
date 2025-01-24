using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using PlayerModule;
using TileEntity.Instances.Machine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldModule.Caves;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances {
    public class CaveTeleporterUIController : MonoBehaviour, ITileEntityUI<CaveTeleporterInstance>, IInventoryListener, IInventoryUITileEntityUI
    {
        public InventoryUI mInventoryUI;
        public CaveSelectController caveSelectController;
        public VerticalLayoutGroup mButtonList;
        public TextMeshProUGUI mNoCaveText;
        public Button buttonPrefab;
        private List<Cave> allCaves;
        private CaveTeleporterInstance caveTeleporterInstance;
        
        private void ButtonPress(Cave cave) {
            caveSelectController.ShowCave(cave);
        }

        public void DisplayTileEntityInstance(CaveTeleporterInstance tileEntityInstance)
        {
            caveTeleporterInstance = tileEntityInstance;
            caveSelectController.DisplayEmpty();
            mInventoryUI.DisplayInventory(tileEntityInstance.CaveStorageDrives);
            mInventoryUI.AddRestriction(ItemTag.CaveData);
            mInventoryUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mInventoryUI.AddListener(this);
            GlobalHelper.deleteAllChildren(mButtonList.transform);
            StartCoroutine(LoadCaves());
        }

        public IEnumerator LoadCaves() {
            var handle = Addressables.LoadAssetsAsync<Cave>("cave",null);
            yield return handle;
            var result = handle.Result;
            allCaves = new List<Cave>();
            foreach (Cave cave in result)
            {
                allCaves.Add(cave);
            }

            Addressables.Release(handle);
            Display();
        }


        private void Display()
        {
            if (allCaves == null) return;
            
            GlobalHelper.deleteAllChildren(mButtonList.transform);
            
            foreach (Cave cave in allCaves) {
                if (!CaveDataInTeleporter(cave)) continue;
                Button button = GameObject.Instantiate(buttonPrefab,mButtonList.transform,false);
                button.GetComponentInChildren<TextMeshProUGUI>().text = cave.name;
                button.onClick.AddListener(() => ButtonPress(cave));
            }

            bool noCaves = mButtonList.transform.childCount == 0;
            mNoCaveText.gameObject.SetActive(noCaves);

            if (!CaveDataInTeleporter(caveSelectController.CurrentCave) || noCaves)
            {
                caveSelectController.DisplayEmpty();
            }
        }

        private bool CaveDataInTeleporter(Cave cave)
        {
            if (ReferenceEquals(cave,null)) return false;
            foreach (ItemSlot itemSlot in caveTeleporterInstance.CaveStorageDrives)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                if (itemSlot.tags?.Dict == null || !itemSlot.tags.Dict.TryGetValue(ItemTag.CaveData, out var value)) continue;
                if ((string)value == cave.Id) return true;
            }

            return false;
        }

        public void InventoryUpdate(int n)
        {
            Display();
        }

        public InventoryUI GetInput()
        {
            return mInventoryUI;
        }

        public InventoryUI GetOutput()
        {
            return mInventoryUI;
        }
    }
}

