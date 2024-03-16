using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ItemSlotUIClickHandler : MonoBehaviour, IPointerClickHandler
{
    private InventoryUI inventoryUI;
    private int index;
    public void init(InventoryUI inventoryUI, int index) {
        this.inventoryUI = inventoryUI;
        this.index = index;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) {
            inventoryUI.leftClick(index);
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            inventoryUI.rightClick(index);
        } else if (eventData.button == PointerEventData.InputButton.Middle) {
            inventoryUI.middleClick(index);
        }
    }

}
