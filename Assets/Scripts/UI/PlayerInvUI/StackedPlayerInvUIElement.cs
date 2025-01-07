using System;
using Items.Inventory;
using PlayerModule;
using TileEntity.Instances.Machine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInvUI
{
    public class StackedPlayerInvUIElement : MonoBehaviour
    {
        [SerializeField] private InventoryUI playerInventoryUI;
        [SerializeField] private GameObject playerInventoryContainer;

        public void Start()
        {
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            playerInventoryUI.DisplayInventory(playerInventory.Inventory);
        }

        public void DisplayWithPlayerInventory(GameObject uiObject, bool below)
        {
            IInventoryUITileEntityUI inventoryUIAggregator = uiObject.GetComponent<IInventoryUITileEntityUI>();
            if (!ReferenceEquals(inventoryUIAggregator, null))
            {
                playerInventoryUI.SetConnection(inventoryUIAggregator.GetInput());
                inventoryUIAggregator.GetInput().SetConnection(playerInventoryUI);
                inventoryUIAggregator.GetOutput().SetConnection(playerInventoryUI);
            }
            uiObject.transform.SetParent(transform,false);
            if (!below)
            {
                uiObject.transform.SetAsFirstSibling();
            }
        }

        public void SetBackgroundColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        
    }
}
