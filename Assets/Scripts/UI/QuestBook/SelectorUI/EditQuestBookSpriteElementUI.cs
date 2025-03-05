using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.QuestBook {
    public class EditQuestBookSpriteElementUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private Image panel;

        private QuestBookTitleSpritePath bookTitleSpritePath;
        private QuestBookSelectorData questBookData;
        private EditQuestBookUI editQuestBookUI;
        private bool Selected {get => questBookData.SpritePath == bookTitleSpritePath;}

        public void init(QuestBookSelectorData questBookData, EditQuestBookUI editQuestBookUI, Sprite sprite, QuestBookTitleSpritePath bookTitleSpritePath) {
            this.image.sprite = sprite;
            this.questBookData = questBookData;
            this.bookTitleSpritePath = bookTitleSpritePath;
            this.editQuestBookUI = editQuestBookUI; 
            setPanelColor();
        }

        public void setPanelColor() {
            panel.color = Selected ?  new Color(0f,1f,0f,200f/255f) :new Color(82f/255f,82f/255f,82f/255f,200f/255f);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (!Selected) {
                    questBookData.SpritePath = bookTitleSpritePath;
                    editQuestBookUI.loadSpritePanelColors();
                } 
            }
        }
    }
}

