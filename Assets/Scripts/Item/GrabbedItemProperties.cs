using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ItemModule;

public class GrabbedItemProperties : MonoBehaviour
{
    public ItemSlot itemSlot;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        Vector3 position = Input.mousePosition;
        position.z = 0;
        transform.position = position;
    }

    public void updateSprite() {
        unload();
        if (itemSlot != null && itemSlot.itemObject != null && itemSlot.itemObject.id != null) {
            GameObject tag = ItemSlotUIFactory.getTagObject(itemSlot,transform);
            GameObject imageObject = ItemSlotUIFactory.getItemImage(itemSlot,transform);
            GameObject numberObject = ItemSlotUIFactory.getNumber(itemSlot,transform);
            return;
        }
    }
    
    public bool setItemSlotFromInventory(List<ItemSlot> inventory, int n) {
        if (itemSlot != null && itemSlot.itemObject != null) {
            return false;
        }
        ItemSlot inventorySlot = inventory[n];
        ItemSlot newSlot = ItemSlotFactory.createNewItemSlot(inventorySlot.itemObject,1);
        inventorySlot.amount--;
        if (inventorySlot.amount == 0) {
            inventory[n] = null;
        }
        this.itemSlot = newSlot;
        updateSprite();
        return true;
    }

    public void addItemSlotFromInventory(List<ItemSlot> inventory, int n) {
        ItemSlot inventorySlot = inventory[n];
        if (!ItemSlotHelper.areEqual(itemSlot,inventorySlot)) {
            return;
        }
        if (itemSlot.amount >= Global.MaxSize) {
            return;
        }
        inventorySlot.amount--;
        if (inventorySlot.amount == 0) {
            inventory[n] = null;
        }
        this.itemSlot.amount += 1;
        updateSprite();
    }

    private void unload() {
        Transform previousNumber = transform.Find("amount");
        if (previousNumber != null) {
            Destroy(previousNumber.gameObject);
        }   
        Transform previousTag = transform.Find("tags");
        if (previousTag != null) {
            Destroy(previousTag.gameObject);
        }
        Transform previousImage = transform.Find("item");
        if (previousImage != null) {
            Destroy(previousImage.gameObject);
        }
    }
}
