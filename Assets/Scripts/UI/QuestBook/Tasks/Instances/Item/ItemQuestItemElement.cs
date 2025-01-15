using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using Items.Inventory;

namespace UI.QuestBook {
    public class ItemQuestItemElement : ItemSlotUI, IItemListReloadable, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mItemName;
        private QuestBookPageUI questBookUI;
        private ItemQuestTask itemQuestTask;
        private uint gottenAmount;
        private SerializedItemSlot ItemSlot {get => itemQuestTask.Items[index];}
        private ItemQuestTaskUI taskUI;
        private int index;
        

        public void Initialize(ItemQuestTask itemQuestTask, int index, ItemQuestTaskUI taskUI, QuestBookPageUI questBookUI) {
            this.itemQuestTask = itemQuestTask;
            this.questBookUI = questBookUI;
            this.taskUI = taskUI;
            this.index = index;
            reload();
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
            UIAssetManager assetManager = taskUI.QuestBookTaskPageUI.AssetManager;
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = assetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
            serializedItemSlotEditorUI.Init(itemQuestTask.Items,index,this,gameObject);
            serializedItemSlotEditorUI.transform.SetParent(taskUI.QuestBookTaskPageUI.transform,false);
        }

        public void reload()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(ItemSlot);
            
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            mItemName.text = itemSlot.itemObject.name;
            
            if (!ReferenceEquals(PlayerManager.Instance, null))
            {
                gottenAmount = ItemSlotUtils.AmountOf(itemSlot, PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory);
            }
            
            gottenAmount = GlobalHelper.Clamp(gottenAmount,0,itemSlot.amount);
            Display(itemSlot);
        }

        public override void SetAmountText()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(ItemSlot);
            AmountText.color = gottenAmount >= itemSlot.amount ? Color.green : Color.red;
            AmountText.text = ItemDisplayUtils.FormatAmountText(gottenAmount,oneInvisible:false) + "/" +
                              ItemDisplayUtils.FormatAmountText(itemSlot.amount,oneInvisible:false);
        }

        public void reloadAll()
        {
            taskUI.Display();
        }

        
    }
}

