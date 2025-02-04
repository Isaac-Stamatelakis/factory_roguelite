using System.Collections.Generic;
using Items.Inventory;
using TileEntity.Instances.Machine.UI;
using TMPro;
using UnityEngine;

namespace TileEntity.Instances.Storage.Chest
{
    public class ChestUI : MonoBehaviour, ITileEntityUI<ChestInstance>, IInventoryUITileEntityUI
    {
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private TextMeshProUGUI mTitle;
        public void DisplayTileEntityInstance(ChestInstance tileEntityInstance)
        {
            mTitle.text = tileEntityInstance.getName();
            mInventoryUI.DisplayInventory(tileEntityInstance.Inventory.Items);
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
