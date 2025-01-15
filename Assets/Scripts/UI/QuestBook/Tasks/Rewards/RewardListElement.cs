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

namespace UI.QuestBook {
    public class RewardListElement : ItemSlotUI, IItemListReloadable, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        private List<SerializedItemSlot> itemSlots;
        private QuestBookTaskPageUI questBookTaskPageUI;
        private int index;
        private bool Selected {get => questBookTaskPageUI.SelectedRewards.Contains(index);}
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left when QuestBookHelper.EditMode:
                {
                    SerializedItemSlotEditorUI serializedItemSlotEditorUI = questBookTaskPageUI.AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    serializedItemSlotEditorUI.Init(itemSlots,index,this,questBookTaskPageUI.gameObject);
                    serializedItemSlotEditorUI.transform.SetParent(questBookTaskPageUI.transform,false);
                    break;
                }
                case PointerEventData.InputButton.Left:
                {
                    if (questBookTaskPageUI.RewardsSelectable) {
                        if (Selected) {
                            questBookTaskPageUI.RemoveReward(index);
                        } else {
                            questBookTaskPageUI.AddReward(index);
                        }
                    }

                    break;
                }
                case PointerEventData.InputButton.Right:
                    break;
            }
        }

        public void Initialize(List<SerializedItemSlot> serializedItemSlots, int index, QuestBookTaskPageUI questBookTaskPageUI) {
            this.itemSlots = serializedItemSlots;
            this.index = index;
            this.questBookTaskPageUI = questBookTaskPageUI;
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
            questBookTaskPageUI.DisplayRewards();
        }
    }
}

