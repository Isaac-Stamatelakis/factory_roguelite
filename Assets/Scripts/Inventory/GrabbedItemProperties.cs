using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GrabbedItemProperties : MonoBehaviour
{
    
    public ItemSlot itemSlot;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = Input.mousePosition;
        position.z = 0;
        transform.position = position;
    }

    public void updateSprite() {
        GameObject previousNumber = Global.findChild(transform, "Amount");
        if (previousNumber != null) {
            Destroy(previousNumber);
        }   
        if (itemSlot != null && itemSlot.itemObject != null && itemSlot.itemObject.id != null) {
            image.enabled = true;

            image.sprite = itemSlot.itemObject.getSprite(); 
            GetComponent<RectTransform>().sizeDelta = InventoryGrid.getItemSize(image.sprite);

            GameObject number = new GameObject();
            TextMeshProUGUI textMeshProUGUI = number.AddComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = itemSlot.amount.ToString();
            textMeshProUGUI.alignment = TextAlignmentOptions.BottomRight;
            textMeshProUGUI.fontSize = 30;
            RectTransform rectTransform = number.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50,50);
            number.name = "Amount";
            number.transform.SetParent(transform);
            number.transform.localPosition = new Vector3(-50,-25,-1); 
        } else {
            image.enabled = false;
            image.sprite = null;
        }
    }
}
