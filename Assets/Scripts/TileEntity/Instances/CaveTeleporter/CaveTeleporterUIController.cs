using System.Collections;
using System.Collections.Generic;
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
        public Button buttonPrefab;
        private void ButtonPress(Cave cave) {
            caveSelectController.ShowCave(cave);
        }

        public void DisplayTileEntityInstance(CaveTeleporterInstance tileEntityInstance)
        {
            caveSelectController.ShowDefault();
            mInventoryUI.DisplayInventory(tileEntityInstance.CaveStorageDrives);
            mInventoryUI.AddRestriction(ItemTag.CaveData);
            mInventoryUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mInventoryUI.AddListener(this);
            StartCoroutine(LoadCaves());
        }

        public IEnumerator LoadCaves() {
            var handle = Addressables.LoadAssetsAsync<Cave>("cave",null);
            yield return handle;
            var result = handle.Result;
            foreach (Cave cave in result) {
                Button button = GameObject.Instantiate(buttonPrefab,mButtonList.transform,false);
                button.GetComponentInChildren<TextMeshProUGUI>().text = cave.name;
                button.onClick.AddListener(() => ButtonPress(cave));
            }
            Addressables.Release(handle);
        }

        public void InventoryUpdate(int n)
        {
            
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

