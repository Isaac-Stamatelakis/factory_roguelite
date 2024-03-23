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
            this.image.sprite = Resources.Load<Sprite>(questBook.SpritePath);
            this.title.text = questBook.Title;
            button.onClick.AddListener(navigatePress);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (QuestBookHelper.EditMode) {
                    EditQuestBookUI editQuestBookUI = EditQuestBookUI.newInstance();
                    editQuestBookUI.init(questBookSelectorUI,library,index);
                    editQuestBookUI.transform.SetParent(questBookSelectorUI.transform,false);
                }
            }
            
        }

        private void navigatePress() {
            questBookSelectorUI.gameObject.SetActive(false);
            GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.QuestBookPrefabPath));
            QuestBookUI questBookUI = instantiated.GetComponent<QuestBookUI>();
            questBookUI.transform.SetParent(questBookSelectorUI.transform.parent,false);
            questBookUI.init(questBook,library,questBookSelectorUI.gameObject);
        }
    }
}

