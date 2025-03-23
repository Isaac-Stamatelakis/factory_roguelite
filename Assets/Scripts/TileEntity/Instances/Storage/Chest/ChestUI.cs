using System.Collections.Generic;
using Items.Inventory;
using TileEntity.Instances.Machine.UI;
using TMPro;
using UnityEngine;

namespace TileEntity.Instances.Storage.Chest
{
    public class ChestUI : MonoBehaviour, ITileEntityUI, IInventoryUITileEntityUI
    {
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private TextMeshProUGUI mTitle;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not ChestInstance chestInstance) return;
            mTitle.text = tileEntityInstance.GetName();
            mInventoryUI.DisplayInventory(chestInstance.Inventory.Items);
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
