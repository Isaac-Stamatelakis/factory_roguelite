using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items.Tags;
using TMPro;

namespace Items {
    public static class ItemSlotUIFactory
    {
        

        public static ItemSlotUI newItemSlotUI(ItemSlot itemSlot, Transform parent, Color? color, bool enableText = true,  string suffix = null) {
            GameObject slot = new GameObject();
            ItemSlotUI itemSlotUI = slot.AddComponent<ItemSlotUI>();
            slot.transform.SetParent(parent);
            itemSlotUI.init(color,enableText);
            if (suffix == null) {
                slot.name = ItemDisplayUtils.SlotName;
            } else {
                slot.name = ItemDisplayUtils.SlotName + suffix;
            }
            itemSlotUI.display(itemSlot);
            return itemSlotUI;
        }

        public static List<ItemSlotUI> getSlotsForInventory(List<ItemSlot> inventories, Transform parent, Color? color, bool enableText = true) {
            List<ItemSlotUI> slots = new List<ItemSlotUI>();
            for (int i = 0; i < inventories.Count; i++) {
                ItemSlotUI slot = newItemSlotUI(inventories[i],parent, color,enableText:enableText,suffix: i.ToString());
                slots.Add(slot);
                slot.transform.SetParent(parent);
            }
            return slots;
        }


        

        
        

        
        

       
        

        
        /*
    
        public static void reload(GameObject slot, ItemSlot itemSlot) {
            if (slot == null) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;   
            }
            Transform imageTransform = slot.transform.Find(itemImageName);
            if (imageTransform == null) {
                getItemImage(itemSlot,slot.transform);
            } else {
                Sprite sprite = itemSlot.itemObject.getSprite();
                Image itemImage = imageTransform.GetComponent<Image>();
                if (itemImage.sprite != sprite) {
                    itemImage.sprite = sprite;
                    imageTransform.GetComponent<RectTransform>().sizeDelta = getItemSize(sprite);
                }
            }
            Transform numberTransform = slot.transform.Find(itemAmountName);
            if (numberTransform == null) {
                getNumber(itemSlot,slot.transform);
            } else {
                numberTransform.GetComponent<TextMeshProUGUI>().text = formatAmountText(itemSlot.amount);
            }

        }

        public static void unload(Transform slotTransform) {
            Transform imageTransform = slotTransform.Find(itemImageName);
            if (imageTransform != null) {
                GameObject.Destroy(imageTransform.gameObject);
            }
            Transform amountTransform = slotTransform.Find(ItemAmountName);
            if (amountTransform != null) {
                GameObject.Destroy(amountTransform.gameObject);
            }
            Transform frontTagTransform = slotTransform.Find(ItemTagNameFront);
            if (frontTagTransform != null) {
                GameObject.Destroy(frontTagTransform.gameObject);
            }
            Transform behindTagTransform = slotTransform.Find(ItemTagNameBehind);
            if (behindTagTransform != null) {
                GameObject.Destroy(behindTagTransform.gameObject);
            }
        }

        public static void replaceAmountTextWithString(Transform slotTransform, string text) {
            Transform amountTransform = slotTransform.Find(ItemAmountName);
            if (amountTransform != null) {
                amountTransform.GetComponent<TextMeshProUGUI>().text = text;
            }
        }
        */
    }
    
}

