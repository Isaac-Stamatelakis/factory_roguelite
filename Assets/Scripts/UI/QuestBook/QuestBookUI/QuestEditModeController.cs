using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class QuestEditModeController : MonoBehaviour
    {
        [SerializeField] private Button addNode;
        [SerializeField] private Button addChapter;
        [SerializeField] private Button toggleConnectionMode;
        [SerializeField] private Image toggleConnectionPanel;
        private QuestBookUI questBookUI;
        private GameObject spawnedNodeObject;
        
        // Start is called before the first frame update

        public void init(QuestBookUI questBookUI) {
            this.questBookUI = questBookUI;
            addNode.onClick.AddListener(addButtonClick);
            addChapter.onClick.AddListener(addChapterClick);
            toggleConnectionMode.onClick.AddListener(() => {
                questBookUI.Mode = questBookUI.Mode == QuestBookUIMode.EditConnection ? QuestBookUIMode.View : QuestBookUIMode.EditConnection;
                toggleConnectionPanel.color = questBookUI.Mode == QuestBookUIMode.EditConnection ? new Color(1f,0f,0f,200f/255f) : new Color(0,1f,0f,200f/255f);
            });
        }

        public void OnDestroy() {
            addNode.onClick.RemoveAllListeners();
            addChapter.onClick.RemoveAllListeners();
        }
        

        private void addChapterClick() {
            questBookUI.QuestBook.Pages.Add(new QuestBookPage("New Page", new List<QuestBookNode>()));
            questBookUI.loadPageChapters();
        }
        private void addButtonClick() {
            spawnedNodeObject = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.NodeObjectPrefabPath));
            spawnedNodeObject.transform.SetParent(questBookUI.NodeContainer,false);
        }

        public void Update() {
            spawnNodePlacement();
        }

        private void spawnNodePlacement() {
            if (spawnedNodeObject == null) {
                return;
            }
            Vector2 mousePosition = Input.mousePosition;
            Vector2 gridPosition = QuestBookHelper.snapGrid(mousePosition,questBookUI.ContentContainer.position,questBookUI.ContentContainer.localScale.x);
            spawnedNodeObject.transform.position = gridPosition;
            if (Input.GetMouseButton(0)) {
                QuestBookNode node = new QuestBookNode(
                    spawnedNodeObject.transform.localPosition,
                    null,
                    new QuestBookNodeContent(
                        new CheckMarkQuestTask(),
                        "Empty Description",
                        "New Task",
                        new List<SerializedItemSlot>(),
                        9999
                    ),
                    new HashSet<int>(),
                    questBookUI.Library.getSmallestNewID(),
                    true
                );
                questBookUI.CurrentPage.Nodes.Add(node);
                questBookUI.Library.addNode(node);
                questBookUI.displayCurrentPage();
                spawnedNodeObject = null;
                
            }
        }

        
    }
}

