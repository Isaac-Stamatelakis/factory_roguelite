using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UI;
using Items;
using Items.Inventory;
using UI.QuestBook.Tasks.Rewards;

namespace UI.QuestBook {
    public class RewardListElement : ItemSlotUI, IItemListReloadable, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        private List<SerializedItemSlot> itemSlots;
        private QuestBookRewardUI questBookRewardUI;
        private int index;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left when QuestBookHelper.EditMode:
                {
                    QuestBookTaskPageUI taskPageUI = questBookRewardUI.ParentUI;
                    SerializedItemSlotEditorUI serializedItemSlotEditorUI = taskPageUI.AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    serializedItemSlotEditorUI.Init(itemSlots,index,this,taskPageUI.gameObject);
                    serializedItemSlotEditorUI.transform.SetParent(taskPageUI.transform,false);
                    break;
                }
                case PointerEventData.InputButton.Left:
                {
                    if (questBookRewardUI.SelectedRewards.Contains(index)) {
                        questBookRewardUI.RemoveReward(index);
                    } else {
                        questBookRewardUI.AddReward(index);
                    }

                    break;
                }
                case PointerEventData.InputButton.Right:
                    break;
            }
        }

        public void Initialize(List<SerializedItemSlot> serializedItemSlots, int index, QuestBookRewardUI questBookRewardUI) {
            this.itemSlots = serializedItemSlots;
            this.index = index;
            this.questBookRewardUI = questBookRewardUI;
            reload();
        }

        
        public void reload()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemSlots[index]);
            mNameText.text = itemSlot?.itemObject?.name;
            Display(itemSlot);
        }

        public void reloadAll()
        {
            questBookRewardUI.Display();
        }
    }
}

