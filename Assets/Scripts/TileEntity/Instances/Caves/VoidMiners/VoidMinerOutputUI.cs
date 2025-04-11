using System;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Caves.VoidMiners
{
    public class VoidMinerOutputUI : MonoBehaviour
    {
        [SerializeField] private InventoryUI mOutput;
        [SerializeField] private Button mActiveButton;
        private bool outputActive;

        public void FixedUpdate()
        {
            mOutput.RefreshSlots();
        }

        public void Display(List<ItemSlot> outputs, bool isOutputActive, Action<bool> onActiveChange)
        {
            mOutput.DisplayInventory(outputs);
            this.outputActive = isOutputActive;
            mOutput.SetInteractMode(InventoryInteractMode.BlockInput);
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
