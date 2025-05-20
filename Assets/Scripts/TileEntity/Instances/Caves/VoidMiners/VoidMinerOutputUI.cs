using System;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TileEntity.Instances.Caves.VoidMiners
{
    public class VoidMinerOutputUI : MonoBehaviour
    {
        [FormerlySerializedAs("mOutput")] [SerializeField] private InventoryUI mInventoryUI;
        public InventoryUI InventoryUI => mInventoryUI;
        [SerializeField] private Button mActiveButton;
        private bool outputActive;

        public void FixedUpdate()
        {
            mInventoryUI.RefreshSlots();
        }

        public void Display(List<ItemSlot> outputs, bool isOutputActive, Action<bool> onActiveChange)
        {
            mInventoryUI.DisplayInventory(outputs);
            this.outputActive = isOutputActive;
            mInventoryUI.SetInteractMode(InventoryInteractMode.BlockInput);
            SetButtonColor();
            mActiveButton.onClick.AddListener(() =>
            {
                outputActive = !outputActive;
                SetButtonColor();
                onActiveChange(outputActive);
            });
        }

        private void SetButtonColor()
        {
            mActiveButton.GetComponent<Image>().color = outputActive ? Color.green : Color.gray;
        }
    }
}
