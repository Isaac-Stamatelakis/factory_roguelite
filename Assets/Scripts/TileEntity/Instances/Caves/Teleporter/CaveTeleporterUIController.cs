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
    public class CaveTeleporterUIController : MonoBehaviour, ITileEntityUI, IInventoryUpdateListener, IInventoryUITileEntityUI
    {
        public InventoryUI mInventoryUI;
        public CaveSelectController caveSelectController;
        public VerticalLayoutGroup mButtonList;
        public TextMeshProUGUI mNoCaveText;
        public Button buttonPrefab;
        private List<CaveObject> allCaves;
        private CaveTeleporterInstance caveTeleporterInstance;
        public TextMeshProUGUI mStatusText;
        
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
            mInventoryUI.AddCallback(InventoryUpdate);
            GlobalHelper.DeleteAllChildren(mButtonList.transform);
            StartCoroutine(LoadCaves());
            caveSelectController.Initialize(() =>
            {
                caveTeleporterInstance.Delay = CaveTeleporterInstance.TELEPORT_DELAY;
            });
            DisplayStatus();
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

        public void FixedUpdate()
        {
            DisplayStatus();
        }

        private void DisplayStatus()
        {
            bool ready = caveTeleporterInstance.Delay <= 0;
            const string STATUS_PREFIX = "Status: ";
            if (ready)
            {
                mStatusText.text = $"{STATUS_PREFIX}Ready To Teleport";
            }
            else
            {
                mStatusText.text = $"{STATUS_PREFIX}Recharging {caveTeleporterInstance.Delay:F1}";
            }
            caveSelectController.DisplayButtonStatus(ready);
        }


        private void Display()
        {
            if (allCaves == null) return;
            
            GlobalHelper.DeleteAllChildren(mButtonList.transform);
            
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

        public void InventoryUpdate()
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

