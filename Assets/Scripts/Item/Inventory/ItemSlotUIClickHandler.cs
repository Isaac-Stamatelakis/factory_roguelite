using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;
using ItemModule.Inventory;

public interface IItemSlotUIElement {
    public ItemSlot getItemSlot();
    public void setItemSlot(ItemSlot itemSlot);
    public GameObject getGameObject();
    public int getDisplayAmount();
    public ItemObject getDisplayItemObject();
    public void reload(ItemSlot inventorySlot,bool force=false) {
        ItemObject displayedItemObject = getDisplayItemObject();
        if (inventorySlot == null || inventorySlot.itemObject == null) {
            return;
        }
        if (!force && inventorySlot.itemObject.Equals(displayedItemObject) && inventorySlot.amount == getDisplayAmount()) {
            return;
        }
        setItemSlot(inventorySlot);
        ItemSlotUIFactory.reload(getGameObject(),inventorySlot);
    }
    public void showRecipes() {
        ItemSlot itemSlot = getItemSlot();
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.displayCraftingOfItem(itemSlot.itemObject);
    }
    public void showUses() {
        ItemSlot itemSlot = getItemSlot();
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.displayUsesOfItem(itemSlot.itemObject);
    }
}
public class ItemSlotUIClickHandler : MonoBehaviour, IPointerClickHandler, IItemSlotUIElement
{
    private InventoryUI inventoryUI;
    private int index;
    private ItemSlot itemSlot;
    private int amount;
    private ItemObject itemObject;
    public void init(InventoryUI inventoryUI, int index) {
        this.inventoryUI = inventoryUI;
        this.index = index;
        setItemSlot(inventoryUI.getItemSlot(index));
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

    public ItemSlot getItemSlot()
    {
        return itemSlot;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public void setItemSlot(ItemSlot itemSlot)
    {
        this.itemSlot = itemSlot;
        if (itemSlot == null || itemSlot.itemObject == null) {
            this.itemObject = null;
            this.amount = -1;
        } else {
            this.itemObject = itemSlot.itemObject;
            this.amount = itemSlot.amount;
        }
    }

    public int getDisplayAmount()
    {
        return amount;
    }

    public ItemObject getDisplayItemObject()
    {
        return itemObject;
    }
}
