using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Items.Inventory {
    public class PlayerInventoryGrid : SolidDynamicInventory
    {
        private int selectedSlot = -1;
        
        public virtual void selectSlot(int n) {
            if (n == selectedSlot) {
                return;
            }
            
            if (selectedSlot >= 0) {
                slots[selectedSlot].GetComponent<Image>().color = ItemDisplayUtils.SolidItemPanelColor;
            }
            
            selectedSlot = n;
            slots[selectedSlot].GetComponent<Image>().color = new Color(255/255f,215/255f,0,100/255f);
        }
    }
}

