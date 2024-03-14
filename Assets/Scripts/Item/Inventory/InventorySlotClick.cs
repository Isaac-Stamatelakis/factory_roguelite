using UnityEngine.UI;
using UnityEngine;
public class InventorySlotClick : MonoBehaviour
{
    
    
    public void onClick() {
        InventoryUI inventoryGrid = transform.parent.GetComponent<InventoryUI>();
        if (inventoryGrid != null) {
            inventoryGrid.clickSlot(int.Parse(gameObject.name.Replace("slot","")));
        }
    }
}
