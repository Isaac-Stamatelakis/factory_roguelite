using System;
using Items.Inventory;
using PlayerModule;
using UnityEngine;

namespace UI.PlayerInvUI
{
    public class StackedPlayerInvUIElement : MonoBehaviour
    {
        [SerializeField] private SolidDynamicInventory playerInventoryUI;

        public void DisplayPlayerInventory()
        {
            
            
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            playerInventoryUI.SetInventory(playerInventory.Inventory);
        
        }
    }
}
