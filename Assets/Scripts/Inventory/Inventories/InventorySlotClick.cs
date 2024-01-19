using UnityEngine.UI;
using UnityEngine;
public class InventorySlotClick : MonoBehaviour
{
    
    
    public void onClick() {
        InventoryGrid inventoryGrid = transform.parent.GetComponent<InventoryGrid>();
        if (inventoryGrid != null) {
            inventoryGrid.swapWithGrabbedItem(int.Parse(gameObject.name.Replace("slot","")));
        }
    }
}
