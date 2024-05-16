using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using Items.Inventory;

namespace UI.QuestBook {
    public class ItemQuestItemElement : MonoBehaviour, IPointerClickHandler, IItemListReloadable
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private Transform itemUIContainer;
        private QuestBookPageUI questBookUI;
        private int gottenAmount;
        private ItemQuestTask itemQuestTask;
        private SerializedItemSlot ItemSlot {get => itemQuestTask.Items[index];}
        private ItemQuestTaskUI taskUI;
        private int index;
        

        public void init(ItemQuestTask itemQuestTask, int index, ItemQuestTaskUI taskUI, QuestBookPageUI questBookUI) {
            this.itemQuestTask = itemQuestTask;
            this.questBookUI = questBookUI;
            this.taskUI = taskUI;
            this.index = index;
            load();
        }

        private void load() {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(ItemSlot);
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            if (this == null) {
                return;
            }
            ItemSlotUI itemSlotUI = ItemSlotUIFactory.newItemSlotUI(itemSlot,itemUIContainer,null,false);
            itemName.text = itemSlot.itemObject.name;
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
                if (QuestBookHelper.EditMode) {
                    navigateToEditMode();
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }
        private void navigateToEditMode() {
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = SerializedItemSlotEditorUI.createNewInstance();
            serializedItemSlotEditorUI.init(itemQuestTask.Items,index,this,gameObject);
            serializedItemSlotEditorUI.transform.SetParent(questBookUI.transform,false);
        }

        public void reload()
        {
            load();
        }

        public void reloadAll()
        {
            taskUI.display();
        }

        
    }
}

