using System.Collections;
using System.Collections.Generic;
using DevTools;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UI;
using Items;
using Items.Inventory;
using UI.QuestBook.Tasks.Rewards;
using UnityEditor.Rendering;

namespace UI.QuestBook {
    public class RewardListElement : ItemSlotUI, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        private List<SerializedItemSlot> itemSlots;
        private QuestBookRewardUI questBookRewardUI;
        private int index;
        [SerializeField] private Color OnSelectColor;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left when DevToolUtils.OnDevToolScene:
                {
                    QuestBookTaskPageUI taskPageUI = questBookRewardUI.ParentUI;
                    SerializedItemSlotEditorUI serializedItemSlotEditorUI = taskPageUI.AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
                    {
                        DisplayAmount = true,
                        DisplayTags = true,
                        EnableArrows = true,
                        EnableDelete = true,
                        IndexValueChange = reload,
                        ListChangeCallback = reloadAll
                    };
                    serializedItemSlotEditorUI.InitializeList(itemSlots,index,parameters);
                    CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject,hideParent:false);
                    break;
                }
                case PointerEventData.InputButton.Left when questBookRewardUI.ParentUI.Content.ItemRewards.LimitOne:
                {
                    questBookRewardUI.SelectReward(index);

                    break;
                }
                case PointerEventData.InputButton.Right:
                    break;
            }
        }

        public void ToggleHighlight()
        {
            Image image = GetComponent<Image>();
            Color temp = image.color;
            GetComponent<Image>().color = OnSelectColor;
            OnSelectColor = temp;
        }

        public void Initialize(List<SerializedItemSlot> serializedItemSlots, int index, QuestBookRewardUI questBookRewardUI) {
            this.itemSlots = serializedItemSlots;
            this.index = index;
            this.questBookRewardUI = questBookRewardUI;
            reload();
        }

        public void Hide()
        {
            mNameText.text = null;
            // Hides image without messing with the sizing in layout group
            var color = GetComponent<Image>().color;
            color.a = 0;
            GetComponent<Image>().color = color;
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

