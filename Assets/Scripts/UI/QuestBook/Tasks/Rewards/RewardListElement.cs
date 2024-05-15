using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemAmount;
        [SerializeField] private Image radioButtonImage;
        private SerializedItemSlot ItemSlot {get => itemSlots[index];}
        private List<SerializedItemSlot> itemSlots;
        private QuestBookTaskPageUI questBookTaskPageUI;
        private int index;
        private bool Selected {get => questBookTaskPageUI.SelectedRewards.Contains(index);}
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (QuestBookHelper.EditMode) {
                    SerializedItemSlotEditorUI serializedItemSlotEditorUI = SerializedItemSlotEditorUI.createNewInstance();
                    serializedItemSlotEditorUI.init(itemSlots,index,this,questBookTaskPageUI.gameObject);
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

        public void init(List<SerializedItemSlot> serializedItemSlots, int index, QuestBookTaskPageUI questBookTaskPageUI) {
            this.itemSlots = serializedItemSlots;
            this.index = index;
            this.questBookTaskPageUI = questBookTaskPageUI;
            display();
        }

        public void display() {
            SerializedItemSlot serializedItemSlot = ItemSlot;
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(serializedItemSlot.id);
            if (itemObject != null) {
                itemImage.sprite = itemObject.getSprite();
                if (itemImage.sprite != null) {
                    itemImage.transform.localScale = ItemSlotUIFactory.getItemScale(itemImage.sprite);
                }
                itemName.text = itemObject.name;
            }
            itemAmount.text = serializedItemSlot.amount.ToString();
            displayRadioButton();  
        }

        private void displayRadioButton() {
            if (!questBookTaskPageUI.RewardsSelectable) {
                radioButtonImage.gameObject.SetActive(false);
            }
            radioButtonImage.color = Selected ? Color.green : Color.white;
        }

        public void reload()
        {
            display();
        }

        public void reloadAll()
        {
            questBookTaskPageUI.displayRewards();
        }

        public static RewardListElement newInstance() {
            GameObject instantiated = GlobalHelper.instantiateFromResourcePath("UI/Quest/RewardItemElement");
            RewardListElement rewardListElement = instantiated.GetComponent<RewardListElement>();
            return rewardListElement;
        }
    }
}

