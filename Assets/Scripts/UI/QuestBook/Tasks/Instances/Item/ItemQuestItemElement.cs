using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;
using UnityEngine.EventSystems;
using UI;
using ItemModule.Inventory;

namespace UI.QuestBook {
    public class ItemQuestItemElement : MonoBehaviour, IPointerClickHandler, IItemListReloadable
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private Image itemImage;
        private QuestBookUI questBookUI;
        private int gottenAmount;
        private ItemQuestTask itemQuestTask;
        private SerializedItemSlot ItemSlot {get => itemQuestTask.Items[index];}
        private ItemQuestTaskUI taskUI;
        private int index;
        

        public void init(ItemQuestTask itemQuestTask, int index, ItemQuestTaskUI taskUI, QuestBookUI questBookUI) {
            this.itemQuestTask = itemQuestTask;
            this.questBookUI = questBookUI;
            this.taskUI = taskUI;
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
            if (this == null) {
                return;
            }
            itemImage.sprite = itemObject.getSprite();
            if (itemImage.sprite != null) {
                itemImage.transform.localScale = ItemSlotUIFactory.getItemScale(itemImage.sprite);
            }
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

