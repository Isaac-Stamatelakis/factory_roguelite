using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class EditConnectionsPageUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup connectionList;
        [SerializeField] private TMP_InputField searchBar;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Button backButton;
        [SerializeField] private Toggle requireAllToggle;
        private QuestBookNode node;
        private QuestBookPageUI questBookPageUI;
        private HashSet<int> displayed;
        private string currentSearch;
        
        public void init(QuestBookNode node, QuestBookPageUI questBookPageUI) {
            this.node = node;
            this.questBookPageUI = questBookPageUI;
            backButton.onClick.AddListener(() => {
                questBookPageUI.displayLines();
                GameObject.Destroy(gameObject);
            });

            requireAllToggle.onValueChanged.AddListener((bool value) => {

            });

            scrollView.onValueChanged.AddListener((Vector2 value) => {
                Vector2 contentSize = scrollView.content.sizeDelta;
                if (value.y <= 100/contentSize.y && displayed.Count < questBookPageUI.Library.IdNodeMap.Count) {
                    displayNonPreRequisites(1);
                }
            });
            currentSearch = "";
            searchBar.onValueChanged.AddListener((string value) => {
                currentSearch = value.ToLower();
            });
            displayConnections();
        }

        public void displayConnections() {
            GlobalHelper.deleteAllChildren(connectionList.transform);
            displayed = new HashSet<int>();
            Dictionary<int, QuestBookNode> idNodeMap = questBookPageUI.Library.IdNodeMap;

            // Display nodes in connections first so they are easier to remove
            foreach (int id in node.Prerequisites) {
                if (nodeMatchSearch(idNodeMap[id],currentSearch)) {
                    displayConnection(id);
                    displayed.Add(id);
                }
            }
            displayNonPreRequisites(10);
        }

        private void displayNonPreRequisites(int amount) {
            int i = 0;
            Dictionary<int, QuestBookNode> idNodeMap = questBookPageUI.Library.IdNodeMap;
            foreach (int id in idNodeMap.Keys) {
                if (i >= amount) {
                    break;
                }
                if (displayed.Contains(id)) {
                    continue;
                }
                if (!nodeMatchSearch(idNodeMap[id],currentSearch)) {
                    continue;
                }
                displayConnection(id);
                displayed.Add(id);
                i++;
            }
        }

        private bool nodeMatchSearch(QuestBookNode node, string search) {
            return node.Content.Title.ToLower().Contains(search) || node.Id.ToString().Contains(search);
        }

        private void displayConnection(int id) {
            ConnectionElementUI connectionElementUI = ConnectionElementUI.newInstance();
            connectionElementUI.init(node.Prerequisites,questBookPageUI.Library.getNode(id));
            connectionElementUI.transform.SetParent(connectionList.transform,false);
        }

        public static EditConnectionsPageUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Quest/Connections/EditConnections").GetComponent<EditConnectionsPageUI>();
        }

        
    }
}

