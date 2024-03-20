using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;

namespace UI.QuestBook {
    public class ItemQuestItemElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private Image itemImage;

        public void init(SerializedItemSlot itemSlot, int gottenAmount) {
            string id = itemSlot.id;
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(id);
            if (itemObject == null) {
                GameObject.Destroy(gameObject);
                return;
            }
            itemImage.sprite = itemObject.getSprite();
            itemName.text = itemObject.name;
            gottenAmount = Mathf.Clamp(gottenAmount,0,itemSlot.amount);
            if (gottenAmount == itemSlot.amount) {
                itemName.color = Color.green;
            } else {
                itemImage.color = Color.red;
            }
            string amountText = gottenAmount + "/" + itemSlot.amount;
            itemName.text = amountText;
        }
    }
}

