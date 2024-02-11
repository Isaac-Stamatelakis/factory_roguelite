using UnityEngine.UI;
using UnityEngine;
public class InventorySlotClick : MonoBehaviour
{
    
    
    public void onClick() {
        AInventoryUI inventoryGrid = transform.parent.GetComponent<AInventoryUI>();
        if (inventoryGrid != null) {
            inventoryGrid.swapWithGrabbedItem(int.Parse(gameObject.name.Replace("slot","")));
        }
    }
}
