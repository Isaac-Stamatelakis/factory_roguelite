using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;
using Items.Inventory;
using Items;
using Recipe.Viewer;
using UI.ToolTip;

public interface IItemSlotUIElement {
    public ItemSlot GetItemSlot();
    public void SetItemSlot(ItemSlot itemSlot);
    public ItemSlotUI GetItemSlotUI();
    public uint GetDisplayAmount();
    public ItemObject GetDisplayItemObject();
    public void Reload(ItemSlot inventorySlot,bool force=false) {
        ItemObject displayedItemObject = GetDisplayItemObject();
        if (inventorySlot == null || inventorySlot.itemObject == null) {
            return;
        }
        if (!force && inventorySlot.itemObject.Equals(displayedItemObject) && inventorySlot.amount == GetDisplayAmount()) {
            return;
        }
        SetItemSlot(inventorySlot);
        GetItemSlotUI().Display(inventorySlot);
    }
    public void ShowRecipes() {
        ItemSlot itemSlot = GetItemSlot();
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.DisplayCraftingOfItem(itemSlot);
    }
    public void ShowUses() {
        ItemSlot itemSlot = GetItemSlot();
        if (itemSlot == null || itemSlot.itemObject == null) {
            return;
        }
        RecipeViewerHelper.DisplayUsesOfItem(itemSlot);
    }
}
public class ItemSlotUIClickHandler : MonoBehaviour, IPointerClickHandler, IItemSlotUIElement, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryUI inventoryUI;
    private int index;
    private ItemSlot itemSlot;
    private uint amount;
    private ItemObject itemObject;
    public void init(InventoryUI inventoryUI, int index) {
        this.inventoryUI = inventoryUI;
        this.index = index;
        SetItemSlot(inventoryUI.GetItemSlot(index));
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

    public ItemSlot GetItemSlot()
    {
        return itemSlot;
    }

    public void SetItemSlot(ItemSlot itemSlot)
    {
        this.itemSlot = itemSlot;
        if (itemSlot == null || itemSlot.itemObject == null) {
            this.itemObject = null;
            this.amount = 0;
        } else {
            this.itemObject = itemSlot.itemObject;
            this.amount = itemSlot.amount;
        }
    }

    public uint GetDisplayAmount()
    {
        return amount;
    }

    public ItemObject GetDisplayItemObject()
    {
        return itemObject;
    }

    public ItemSlotUI GetItemSlotUI()
    {
        return gameObject.GetComponent<ItemSlotUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemObject == null) {
            return;
        }
        ToolTipController.Instance.showToolTip(transform.position+new Vector3(40,0),itemObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipController.Instance.hideToolTip();
    }
}
