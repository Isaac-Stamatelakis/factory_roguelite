using System.Collections;
using System.Collections.Generic;
using DevTools;
using Item.Slot;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using Items.Inventory;
using UI.QuestBook.Data.Node;

namespace UI.QuestBook {
    public class ItemQuestItemElement : ItemSlotUI, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mItemName;
        private ItemQuestTask itemQuestTask;
        private uint gottenAmount;
        private SerializedItemSlot ItemSlot {get => itemQuestTask.Items[index];}
        private ItemQuestTaskUI taskUI;
        private int index;
        private bool complete = false;
        public bool Complete => complete;
        private QuestBookTaskData questBookTaskData;
        

        public void Initialize(ItemQuestTask itemQuestTask, int index, ItemQuestTaskUI taskUI, QuestBookTaskData questBookTaskData) {
            this.itemQuestTask = itemQuestTask;
            this.taskUI = taskUI;
            this.index = index;
            this.questBookTaskData = questBookTaskData;
            Reload();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (DevToolUtils.OnDevToolScene) {
                    NavigateToEditMode();
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }
        private void NavigateToEditMode() {
            UIAssetManager assetManager = taskUI.QuestBookTaskPageUI.AssetManager;
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = assetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
            SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
            {
                DisplayAmount = true,
                DisplayTags = true,
                EnableArrows = true,
                EnableDelete = true,
                IndexValueChange = Reload,
                ListChangeCallback =  taskUI.Display
            };
            serializedItemSlotEditorUI.InitializeList(itemQuestTask.Items,index,parameters);
            CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject,hideParent:false);
            
        }

        public void Reload()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(ItemSlot);
            
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            mItemName.text = itemSlot.itemObject.name;
            
            gottenAmount = DevToolUtils.OnDevToolScene || questBookTaskData.Complete ? itemSlot.amount : ItemSlotUtils.AmountOf(itemSlot, PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory);
            gottenAmount = GlobalHelper.Clamp(gottenAmount,0,itemSlot.amount);
            complete = gottenAmount >= itemSlot.amount;
            Display(itemSlot);
        }

        public override void SetAmountText()
        {
            mBottomText.color = complete ? Color.green : Color.red;
            mBottomText.text = ItemDisplayUtils.FormatAmountText(gottenAmount,oneInvisible:false) + "/" +
                              ItemDisplayUtils.FormatAmountText(ItemSlot?.amount ?? 0,oneInvisible:false);
        }

        

        
    }
}

