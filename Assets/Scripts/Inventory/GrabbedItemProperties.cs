using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GrabbedItemProperties : MonoBehaviour
{
    private Matter grabbedItemData;
    public Matter GrabbedItemData {get{return grabbedItemData;} set{grabbedItemData = value;}}
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = -5;
        transform.position = position;
    }

    public void updateSprite() {
        GameObject previousNumber = Global.findChild(transform, "Amount");
        if (previousNumber != null) {
            Destroy(previousNumber);
        }
        if (grabbedItemData != null && grabbedItemData.id > 0) {
            if (grabbedItemData.state is Solid) {
                if (grabbedItemData is TypableMatter) {

                } else if (grabbedItemData is ActionItem) {

                } else {
                    spriteRenderer.sprite = IdDataMap.getInstance().GetSprite(grabbedItemData.id);
                    float scale = InventoryHelper.getItemSize(spriteRenderer.sprite);
                    transform.localScale = new Vector3(scale,scale,1);
                    GameObject number = InventoryHelper.generateNumberText(grabbedItemData.amount, TextAlignmentOptions.BottomLeft);
                    number.name = "Amount";
                    number.transform.SetParent(transform);
                    number.transform.localPosition = new Vector3(0,0,-1);
                    Debug.Log(true);
                } 
            }   
            
               
        } else {
            spriteRenderer.sprite = null;
        }
    }
}
