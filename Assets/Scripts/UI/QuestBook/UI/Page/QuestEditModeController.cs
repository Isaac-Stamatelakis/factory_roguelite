using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class QuestEditModeController : MonoBehaviour
    {
        [SerializeField] private Button addButton;
        private QuestBookUI questBookUI;
        private GameObject spawnedNodeObject;
        
        // Start is called before the first frame update

        public void init(QuestBookUI questBookUI) {
            this.questBookUI = questBookUI;
            addButton.onClick.AddListener(addButtonClick);
        }

        public void OnDestroy() {
            addButton.onClick.RemoveAllListeners();
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
                questBookUI.CurrentPage.Nodes.Add(new QuestBookNode(spawnedNodeObject.transform.localPosition));
                spawnedNodeObject = null;
                
            }
        }

        
    }
}

