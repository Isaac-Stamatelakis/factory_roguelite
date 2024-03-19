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
        private float minScale = 0.5f;
        private float maxScale = 2f;
        private float zoomSpeed = 0.3f;
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
            handleZoom();
        }

        private void spawnNodePlacement() {
            if (spawnedNodeObject == null) {
                return;
            }
            Vector2 mousePosition = Input.mousePosition;
            Vector2 gridPosition = QuestBookHelper.snapGrid(mousePosition);
            spawnedNodeObject.transform.position = gridPosition;
            if (Input.GetMouseButton(0)) {
                spawnedNodeObject = null;
            }
        }

        private void handleZoom() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = questBookUI.ContentContainer;
                Vector3 newScale = containerTransform.localScale + Vector3.one * scrollInput * zoomSpeed;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, minScale, maxScale),
                    Mathf.Clamp(newScale.y, minScale, maxScale),
                    Mathf.Clamp(newScale.z, minScale, maxScale)
                );
                Vector3 scaleChange = (newScale - containerTransform.localScale);
                Vector3 newOffset = scaleChange.x/newScale.x *  (containerTransform.position - mousePosition);
                containerTransform.localScale = newScale;
                containerTransform.position = containerTransform.position + newOffset;
            }
    
        }
    }
}

