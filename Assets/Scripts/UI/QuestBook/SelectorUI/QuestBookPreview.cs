using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace UI.QuestBook {
    public class QuestBookPreview : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button button;
        private QuestBookLibrary library;
        private QuestBook questBook {get => library.QuestBooks[index];}
        private QuestBookSelectorUI questBookSelectorUI;
        private int index;
        
        public void init(int index, QuestBookSelectorUI questBookSelectorUI, QuestBookLibrary library) {
            this.questBookSelectorUI = questBookSelectorUI;
            this.library = library;
            this.index = index;
            this.image.sprite = questBookSelectorUI.getSprite(questBook.SpriteKey);
            this.title.text = questBook.Title;
            button.onClick.AddListener(navigatePress);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (QuestBookHelper.EditMode) {
                    EditQuestBookUI editQuestBookUI = questBookSelectorUI.AssetManager.cloneElement<EditQuestBookUI>("EDIT");
                    editQuestBookUI.init(questBookSelectorUI,library,index);
                    editQuestBookUI.transform.SetParent(questBookSelectorUI.transform,false);
                }
            }
            
        }

        private void navigatePress() {
            GameObject.Destroy(questBookSelectorUI.gameObject);
            QuestBookUI questBookUI = questBookSelectorUI.AssetManager.cloneElement<QuestBookUI>("QUEST_BOOK");
            questBookUI.transform.SetParent(questBookSelectorUI.transform.parent,false);
            questBookUI.init(questBook,library,questBookSelectorUI.gameObject);
        }
    }
}

