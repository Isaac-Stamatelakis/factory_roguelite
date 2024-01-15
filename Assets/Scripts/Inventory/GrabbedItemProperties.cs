using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GrabbedItemProperties : MonoBehaviour
{
    private Dictionary<string,object> grabbedItemData;
    public Dictionary<string,object> GrabbedItemData {get{return grabbedItemData;} set{grabbedItemData = value;}}
    public int Id {get{return getIntFromDict("id");}}
    public int Amount {get{return getIntFromDict("amount");}}
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
        int id = this.Id;
        int amount = this.Amount;
        if (grabbedItemData != null && id > 0) {
            image.enabled = true;

            image.sprite = IdDataMap.getInstance().GetSprite(id);
            GetComponent<RectTransform>().sizeDelta = InventoryGrid.getItemSize(image.sprite);

            GameObject number = new GameObject();
            TextMeshProUGUI textMeshProUGUI = number.AddComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = amount.ToString();
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

    private int getIntFromDict(string key) {
        if (grabbedItemData == null) {
            return -1;
        }
        if (grabbedItemData.ContainsKey(key)) {
            return Convert.ToInt32(grabbedItemData[key]);
        } else {
            return -1;
        }
    }
}
