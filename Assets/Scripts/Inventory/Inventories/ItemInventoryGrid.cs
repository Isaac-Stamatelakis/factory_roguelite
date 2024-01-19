using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemInventoryGrid : DynamicInventoryGrid
{
    protected override GameObject loadItemAmountNumber(GameObject slot, ItemSlot itemSlot)
    {
        if (itemSlot is null) {
            return null;
        }
        if (itemSlot.itemObject.id == null) {
            return null;
        }
        GameObject number = base.loadItemAmountNumber(slot, itemSlot);
        TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
        textMeshPro.text = itemSlot.amount.ToString();
        
        
        textMeshPro.fontSize = 30;
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        
        rectTransform.localPosition = new Vector3(5f,5f,1);
        rectTransform.sizeDelta = new Vector2(96,96);
        textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
        return number;
    }

    protected override GameObject loadItemImage(GameObject slot, ItemSlot itemSlot)
    {
        if (itemSlot is null) {
            return null;
        }
        if (itemSlot.itemObject.id == null) {
            return null;
        }
        GameObject imageObject = base.loadItemImage(slot, itemSlot);
        imageObject.AddComponent<CanvasRenderer>();
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        Image image = imageObject.AddComponent<Image>();
        image.sprite = itemSlot.itemObject.sprite;
        rectTransform.sizeDelta = getItemSize(image.sprite);
        return imageObject;
    }
}
