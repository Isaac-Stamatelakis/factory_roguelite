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
    public class RewardListElement : MonoBehaviour, IPointerClickHandler,  IItemListReloadable
    {
        [SerializeField] private ItemSlotUI itemSlotUI;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemAmount;
        [SerializeField] private Image radioButtonImage;
        [SerializeField] private Transform itemContainer;
        private SerializedItemSlot ItemSlot {get => itemSlots[index];}
        private List<SerializedItemSlot> itemSlots;
        private QuestBookTaskPageUI questBookTaskPageUI;
        private int index;
        private bool Selected {get => questBookTaskPageUI.SelectedRewards.Contains(index);}
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (QuestBookHelper.EditMode) {
                    SerializedItemSlotEditorUI serializedItemSlotEditorUI = questBookTaskPageUI.AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    serializedItemSlotEditorUI.Init(itemSlots,index,this,questBookTaskPageUI.gameObject);
                    serializedItemSlotEditorUI.transform.SetParent(questBookTaskPageUI.transform,false);
                } else {
                    if (questBookTaskPageUI.RewardsSelectable) {
                        if (Selected) {
                            questBookTaskPageUI.removeReward(index);
                        } else {
                            questBookTaskPageUI.addReward(index);
                        }
                    }
                    
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }

        public void Init(List<SerializedItemSlot> serializedItemSlots, int index, QuestBookTaskPageUI questBookTaskPageUI) {
            this.itemSlots = serializedItemSlots;
            this.index = index;
            this.questBookTaskPageUI = questBookTaskPageUI;
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemSlots[index]);
            Display();
        }

        public void Display() {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemSlots[index]);
            itemAmount.text = itemSlot.amount.ToString();
            itemName.text = itemSlot.itemObject.name;
            itemSlotUI.Display(itemSlot);
            DisplayRadioButton();  
        }

        private void DisplayRadioButton() {
            if (!questBookTaskPageUI.RewardsSelectable) {
                radioButtonImage.gameObject.SetActive(false);
            }
            radioButtonImage.color = Selected ? Color.green : Color.white;
        }

        public void reload()
        {
            Display();
        }

        public void reloadAll()
        {
            questBookTaskPageUI.displayRewards();
        }
    }
}

