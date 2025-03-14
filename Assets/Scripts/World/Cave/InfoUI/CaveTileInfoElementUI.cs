using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UnityEngine;

namespace World.Cave.InfoUI
{
    public class CaveTileInfoElementUI : MonoBehaviour
    {
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private TextMeshProUGUI mChanceTextUI;

        public void Display(CaveTileInfoElement caveTileInfoElement)
        {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(caveTileInfoElement.Id);
            ItemSlot itemSlot = new ItemSlot(itemObject,1,null);
            mInventoryUI.DisplayInventory(new List<ItemSlot> { itemSlot },clear:false);
            mInventoryUI.InventoryInteractMode = InventoryInteractMode.Recipe;
        
            mChanceTextUI.text = $"{caveTileInfoElement.Chance:P2}";

        }
    
    }
}
