using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemInventoryGrid : DynamicInventoryGrid
{
    protected override GameObject loadItemAmountNumber(GameObject slot, Dictionary<string,object> data)
    {
        if (data is null) {
            return null;
        }
        int id = Convert.ToInt32(data["id"]);
        int amount = Convert.ToInt32(data["amount"]);
        if (id == -1) {
            return null;
        }
        GameObject number = base.loadItemAmountNumber(slot, data);
        TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
        textMeshPro.text = amount.ToString();
        
        
        textMeshPro.fontSize = 30;
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        
        rectTransform.localPosition = new Vector3(5f,5f,1);
        rectTransform.sizeDelta = new Vector2(96,96);
        textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
        return number;
    }

    protected override GameObject loadItemImage(GameObject slot, Dictionary<string,object> data)
    {
        if (data is null) {
            return null;
        }
        int id = Convert.ToInt32(data["id"]);
        if (id == -1) {
            return null;
        }
        GameObject imageObject = base.loadItemImage(slot, data);
        imageObject.AddComponent<CanvasRenderer>();
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        Image image = imageObject.AddComponent<Image>();

        image.sprite = IdDataMap.getInstance().GetSprite(id);
        
        
        rectTransform.sizeDelta = getItemSize(image.sprite);
        return imageObject;
    }

    /*
    protected override bool validateData(Matter data)
    {
        return data.state == null || data.state is Solid;
    }
    */
}
