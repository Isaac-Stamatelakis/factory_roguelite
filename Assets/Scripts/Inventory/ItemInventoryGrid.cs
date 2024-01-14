using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInventoryGrid : DynamicInventoryGrid
{
    protected override GameObject loadItemAmountNumber(GameObject slot, object data)
    {
        if (data is null || !(data is Matter)) {
            return null;
        }
        Matter matter = (Matter) data;
        if (matter.id == -1) {
            return null;
        }
        GameObject number = base.loadItemAmountNumber(slot, data);
        TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
        textMeshPro.text = matter.amount.ToString();
        
        
        textMeshPro.fontSize = 30;
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        
        rectTransform.localPosition = new Vector3(5f,5f,1);
        rectTransform.sizeDelta = new Vector2(96,96);
        textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
        return number;
    }

    protected override GameObject loadItemImage(GameObject slot, object data)
    {
        if (data is null || !(data is Matter)) {
            return null;
        }
        Matter matter = (Matter) data;
        if (matter.id == -1) {
            return null;
        }
        GameObject imageObject = base.loadItemImage(slot, data);
        imageObject.AddComponent<CanvasRenderer>();
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        Image image = imageObject.AddComponent<Image>();

        if (matter is TypableMatter) {
            TypableMatter typableMatter = (TypableMatter) matter;
            
        } else if (matter is ActionItem) {
            ActionItem actionItem = (ActionItem) matter;
        } else {
            image.sprite = IdDataMap.getInstance().GetSprite(matter.id);
        }
        
        
        rectTransform.sizeDelta = getItemSize(image.sprite);
        return imageObject;
    }

    protected override bool validateData(Matter data)
    {
        return data.state == null || data.state is Solid;
    }
}
