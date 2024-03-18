using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;

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

    public void showRecipes() {
        ItemSlot itemSlot = inventoryUI.getItemSlot(index);
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.displayCraftingOfItem(itemSlot.itemObject);
    }

    public void showUses() {
        ItemSlot itemSlot = inventoryUI.getItemSlot(index);
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.displayUsesOfItem(itemSlot.itemObject);
    }
}
