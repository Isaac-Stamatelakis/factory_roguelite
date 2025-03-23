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
    public class CaveTeleporterUIController : MonoBehaviour, ITileEntityUI, IInventoryListener, IInventoryUITileEntityUI
    {
        public InventoryUI mInventoryUI;
        public CaveSelectController caveSelectController;
        public VerticalLayoutGroup mButtonList;
        public TextMeshProUGUI mNoCaveText;
        public Button buttonPrefab;
        private List<CaveObject> allCaves;
        private CaveTeleporterInstance caveTeleporterInstance;
        
        private void ButtonPress(CaveObject caveObject) {
            caveSelectController.ShowCave(caveObject);
        }

        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not CaveTeleporterInstance caveTeleporter) return;
            caveTeleporterInstance = caveTeleporter;
            caveSelectController.DisplayEmpty();
            mInventoryUI.DisplayInventory(caveTeleporterInstance.CaveStorageDrives);
            mInventoryUI.AddTagRestriction(ItemTag.CaveData);
            mInventoryUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mInventoryUI.AddListener(this);
            GlobalHelper.deleteAllChildren(mButtonList.transform);
            StartCoroutine(LoadCaves());
        }

        public IEnumerator LoadCaves() {
            var handle = Addressables.LoadAssetsAsync<CaveObject>("cave",null);
            yield return handle;
            var result = handle.Result;
            allCaves = new List<CaveObject>();
            foreach (CaveObject cave in result)
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
            
            foreach (CaveObject cave in allCaves) {
                if (!CaveDataInTeleporter(cave)) continue;
                Button button = GameObject.Instantiate(buttonPrefab,mButtonList.transform,false);
                button.GetComponentInChildren<TextMeshProUGUI>().text = cave.name;
                button.onClick.AddListener(() => ButtonPress(cave));
            }

            bool noCaves = mButtonList.transform.childCount == 0;
            mNoCaveText.gameObject.SetActive(noCaves);

            if (!CaveDataInTeleporter(caveSelectController.CurrentCaveObject) || noCaves)
            {
                caveSelectController.DisplayEmpty();
            }
        }

        private bool CaveDataInTeleporter(CaveObject caveObject)
        {
            if (ReferenceEquals(caveObject,null)) return false;
            foreach (ItemSlot itemSlot in caveTeleporterInstance.CaveStorageDrives)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                if (itemSlot.tags?.Dict == null || !itemSlot.tags.Dict.TryGetValue(ItemTag.CaveData, out var value)) continue;
                if ((string)value == caveObject.GetId()) return true;
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

        public List<InventoryUI> GetAllInventoryUIs()
        {
            return new List<InventoryUI> { mInventoryUI };
        }
    }
}

