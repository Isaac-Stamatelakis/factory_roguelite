using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;
using UnityEngine.EventSystems;
using UI;

namespace UI.QuestBook {
    public class ItemQuestItemElement : MonoBehaviour, IPointerClickHandler, ISerializedItemSlotContainer
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private Image itemImage;
        private QuestBookUI questBookUI;
        private int gottenAmount;
        private ItemQuestTask itemQuestTask;
        private SerializedItemSlot ItemSlot {get => itemQuestTask.Items[index];}
        private int index;

        public void init(ItemQuestTask itemQuestTask, int index, QuestBookUI questBookUI) {
            this.itemQuestTask = itemQuestTask;
            this.questBookUI = questBookUI;
            this.index = index;
            load();
        }

        private void load() {
            SerializedItemSlot itemSlot = ItemSlot;
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
                amount.color = Color.green;
            } else {
                amount.color = Color.red;
            }
            string amountText = gottenAmount + "/" + itemSlot.amount;
            amount.text = amountText;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (questBookUI.EditMode) {
                    navigateToEditMode();
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }
        private void navigateToEditMode() {
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = SerializedItemSlotEditorUI.createNewInstance();
            serializedItemSlotEditorUI.init(ItemSlot,this,gameObject);
            serializedItemSlotEditorUI.transform.SetParent(questBookUI.transform,false);
        }

        public void setSeralizedItemSlot(SerializedItemSlot serializedItemSlot)
        {
            itemQuestTask.Items[index] = serializedItemSlot;
            load();
        }
    }
}

