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

        private string path;
        private QuestBook questBook;
        private EditQuestBookUI editQuestBookUI;
        private bool Selected {get => questBook.SpritePath == path;}

        public void init(QuestBook questBook, EditQuestBookUI editQuestBookUI, Sprite sprite, string path) {
            this.image.sprite = sprite;
            this.questBook = questBook;
            this.path = path;
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
                    questBook.SpritePath = path;
                    editQuestBookUI.loadSpritePanelColors();
                } 
            }
        }
    }
}

