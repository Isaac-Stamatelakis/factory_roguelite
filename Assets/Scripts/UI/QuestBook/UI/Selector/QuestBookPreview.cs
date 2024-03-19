using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class QuestBookPreview : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button button;

        private QuestBook questBook;
        private GameObject selectorGameObject;
        
        public void init(QuestBook questBook, GameObject selectorGameObject) {
            this.questBook = questBook;
            this.selectorGameObject = selectorGameObject;
            this.image.sprite = Resources.Load<Sprite>(questBook.SpritePath);
            this.title.text = questBook.Title;
            button.onClick.AddListener(navigatePress);
        }

        private void navigatePress() {
            selectorGameObject.SetActive(false);
            GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.QuestBookPrefabPath));
            QuestBookUI questBookUI = instantiated.GetComponent<QuestBookUI>();
            questBookUI.transform.SetParent(selectorGameObject.transform.parent,false);
            questBookUI.init(questBook,selectorGameObject);
        }
    }
}

