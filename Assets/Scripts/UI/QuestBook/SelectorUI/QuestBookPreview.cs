using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UI.QuestBook {
    public class QuestBookPreview : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button button;
        private QuestBookLibraryData library;
        private QuestBookSelectorData questBookSelectorData {get => library.QuestBookDataList[index];}
        private QuestBookSelectorUI questBookSelectorUI;
        private int index;
        private string libraryPath;
        
        public void Initialize(int index, QuestBookSelectorUI questBookSelectorUI, QuestBookLibraryData library, string libraryPath) {
            this.questBookSelectorUI = questBookSelectorUI;
            this.library = library;
            this.index = index;
            this.libraryPath = libraryPath;
            Sprite sprite = questBookSelectorUI.GetSprite(questBookSelectorData.SpritePath);
            if (!ReferenceEquals(sprite, null)) this.image.sprite = sprite;
            
            this.title.text = questBookSelectorData.Title;
            button.onClick.AddListener(NavigatePress);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME) {
                    EditQuestBookUI editQuestBookUI = questBookSelectorUI.AssetManager.cloneElement<EditQuestBookUI>("EDIT");
                    editQuestBookUI.Initialize(questBookSelectorUI,library,index,libraryPath);
                    editQuestBookUI.transform.SetParent(questBookSelectorUI.transform,false);
                }
            }
            
        }

        private void NavigatePress() {
            Destroy(questBookSelectorUI.gameObject);
            QuestBookUI questBookUI = questBookSelectorUI.AssetManager.cloneElement<QuestBookUI>("QUEST_BOOK");
            questBookUI.transform.SetParent(questBookSelectorUI.transform.parent,false);
            string questBookPath = Path.Combine(libraryPath, questBookSelectorData.Id);
            string questBookDataPath = Path.Combine(questBookPath, QuestBookUtils.QUESTBOOK_DATA_PATH);
            string json = File.ReadAllText(questBookDataPath);
            QuestBookData questBookData = JsonConvert.DeserializeObject<QuestBookData>(json);
            questBookUI.Initialize(questBookData,libraryPath,questBookSelectorData.Id);
        }
    }
}

