using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryHelper
{
    private static float itemScaleMultipler = 1.4f;
    public static float getItemSize(Sprite sprite) {
        Vector2 vector = new Vector2();
        if (sprite.texture.width == 16) {
            vector.x = 0.5f;
        } else {
            vector.x = 16f/(sprite.texture.width);
        }
        if (sprite.texture.height == 16) {
            vector.y = 0.5f;
        } else {
            vector.y = 16f/(sprite.texture.height);
        }
        return Mathf.Min(vector.x,vector.y) * Global.InventoryScale * itemScaleMultipler;
        //vector *= Global.InventoryScale * itemScaleMultipler;
        //return vector;
    }

    public static GameObject generateNumberText(int amount, TextAlignmentOptions alignmentOptions) {
        GameObject number = new GameObject();
        TextMeshPro textMeshPro = number.AddComponent<TMPro.TextMeshPro>();
        textMeshPro.text = amount.ToString();
        textMeshPro.fontSize = 4;
        textMeshPro.rectTransform.sizeDelta = new Vector2(Global.InventoryScale,Global.InventoryScale);
        textMeshPro.alignment = alignmentOptions;
        return number;
    
    }
}
