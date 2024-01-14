using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrabbedItemProperties : MonoBehaviour
{
    private Matter grabbedItemData;
    public Matter GrabbedItemData {get{return grabbedItemData;} set{grabbedItemData = value;}}
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
        if (grabbedItemData != null && grabbedItemData.id > 0) {
            image.enabled = true;
            if (grabbedItemData.state is Solid) {
                if (grabbedItemData is TypableMatter) {

                } else if (grabbedItemData is ActionItem) {

                } else {
                    image.sprite = IdDataMap.getInstance().GetSprite(grabbedItemData.id);
                    GetComponent<RectTransform>().sizeDelta = InventoryGrid.getItemSize(image.sprite);

                    GameObject number = new GameObject();
                    TextMeshProUGUI textMeshProUGUI = number.AddComponent<TextMeshProUGUI>();
                    textMeshProUGUI.text = grabbedItemData.amount.ToString();
                    textMeshProUGUI.alignment = TextAlignmentOptions.BottomRight;
                    textMeshProUGUI.fontSize = 30;
                    RectTransform rectTransform = number.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(50,50);
                    number.name = "Amount";
                    number.transform.SetParent(transform);
                    number.transform.localPosition = new Vector3(-50,-25,-1);
                } 
            }   
            
               
        } else {
            image.enabled = false;
            image.sprite = null;
        }
    }
}
