using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class EditConnectionsPageUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup connectionList;
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Button backButton;
        [SerializeField] private Toggle requireAllToggle;
        [SerializeField] private ConnectionElementUI connectionElementUIPrefab;
        private QuestBookNode node;
        private QuestBookPageUI questBookPageUI;
        private HashSet<int> displayed;
        private string currentSearch;
        private Dictionary<int, QuestBookNodeData> idNodeDataDict = new Dictionary<int, QuestBookNodeData>();
        
        public void Initialize(QuestBookNode node, QuestBookPageUI questBookPageUI, string questBookPath) {
            this.node = node;
            this.questBookPageUI = questBookPageUI;
            backButton.onClick.AddListener(() => {
                questBookPageUI.DisplayLines();
                GameObject.Destroy(gameObject);
            });

            requireAllToggle.onValueChanged.AddListener((bool value) => {

            });

            scrollView.onValueChanged.AddListener((Vector2 value) => {
                Vector2 contentSize = scrollView.content.sizeDelta;
                if (value.y <= 100/contentSize.y && displayed.Count < idNodeDataDict.Count) {
                    DisplayNonPreRequisites(1);
                }
            });
            currentSearch = "";
            searchBar.onValueChanged.AddListener((string value) => {
                currentSearch = value.ToLower();
            });
            DisplayConnections();
        }

        public void DisplayConnections() {
            GlobalHelper.deleteAllChildren(connectionList.transform);
            displayed = new HashSet<int>();

            // DisplayNewElement nodes in connections first so they are easier to remove
            foreach (int id in node.GetPrerequisites()) {
                if (nodeMatchSearch(idNodeDataDict[id],currentSearch)) {
                    DisplayConnection(id);
                    displayed.Add(id);
                }
            }
            DisplayNonPreRequisites(10);
        }

        private void DisplayNonPreRequisites(int amount) {
            int i = 0;
            foreach (int id in idNodeDataDict.Keys) {
                if (i >= amount) {
                    break;
                }
                if (displayed.Contains(id)) {
                    continue;
                }
                if (!nodeMatchSearch(idNodeDataDict[id],currentSearch)) {
                    continue;
                }
                DisplayConnection(id);
                displayed.Add(id);
                i++;
            }
        }

        private static bool nodeMatchSearch(QuestBookNodeData node, string search) {
            return node.Content.Title.ToLower().Contains(search) || node.Id.ToString().Contains(search);
        }

        private void DisplayConnection(int id) {
            ConnectionElementUI connectionElementUI = GameObject.Instantiate(connectionElementUIPrefab);
            connectionElementUI.Initialize(node.GetPrerequisites(),idNodeDataDict.GetValueOrDefault(id));
            connectionElementUI.transform.SetParent(connectionList.transform,false);
        }
    }
}

