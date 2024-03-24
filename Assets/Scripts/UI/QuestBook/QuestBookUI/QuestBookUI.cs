using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {

    public enum QuestBookUIMode {
        View,
        EditConnection
    }
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private Transform lineContainer;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Transform contentMaskContainer;
        [SerializeField] private Button backButton;
        [SerializeField] private GridLayoutGroup chapterGridGroup;
        private string editModePath = "UI/Quest/EditModeElements";
        private QuestEditModeController editModeController;

        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => contentContainer;}
        public Transform ContentMaskContainer {get => contentMaskContainer;}
        public QuestBook QuestBook { get => questBook; set => questBook = value; }
        public QuestBookPage CurrentPage { get => currentPage; set => currentPage = value; }
        public QuestBookLibrary Library { get => library; set => library = value; }
        public QuestBookUIMode Mode { get => mode; set => mode = value; }
        public QuestBookNodeObject CurrentSelected { get => selectedNode; set => selectedNode = value; }

        private QuestBook questBook;
        private GameObject selectorObject;
        private QuestBookPage currentPage;
        private QuestBookLibrary library;
        private float minScale = 0.35f;
        private float maxScale = 3f;
        private float zoomSpeed = 0.3f;
        private QuestBookUIMode mode = QuestBookUIMode.View;
        private QuestBookNodeObject selectedNode;

        // Start is called before the first frame update
        void Start()
        {
            if (QuestBookHelper.EditMode) {
                initEditMode();
            }   
        }

        public void init(QuestBook questBook, QuestBookLibrary library, GameObject selectorObject) {
            this.questBook = questBook;
            this.selectorObject = selectorObject;
            this.library = library;
            this.backButton.onClick.AddListener(backButtonPress);
            loadPageChapters();
            displayPageIndex(0);
        }

        public void loadPageChapters() {
            for (int i = 0; i < chapterGridGroup.transform.childCount; i++) {
                GameObject.Destroy(chapterGridGroup.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < questBook.Pages.Count; i++) {
                GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.QuestBookChapterPrefabPath));
                QuestPageChapterButton chapterButton = instantiated.GetComponent<QuestPageChapterButton>();
                instantiated.transform.SetParent(chapterGridGroup.transform,false);
                chapterButton.init(this,questBook.Pages[i],i);
            }
        }

        private void backButtonPress() {
            selectorObject.SetActive(true);
            GameObject.Destroy(gameObject);
        }
        public void selectNode(QuestBookNodeObject questBookNodeObject) {
            if (CurrentSelected != null) {
                CurrentSelected.setSelect(false);
            }
            CurrentSelected = questBookNodeObject;
            CurrentSelected.setSelect(true);
            
        }

        public void displayPageIndex(int index) {
            if (index < 0 || index >= questBook.Pages.Count) {
                return;
            }
            displayPage(questBook.Pages[index]);
        }

        private void displayPage(QuestBookPage page) {
            for (int i = 0; i < nodeContainer.childCount; i++) {
                GameObject.Destroy(nodeContainer.GetChild(i).gameObject);
            }
            for (int i = 0; i < lineContainer.childCount; i++) {
                GameObject.Destroy(lineContainer.GetChild(i).gameObject);
            }
            
            currentPage = page;
            foreach (QuestBookNode node in page.Nodes) {
                QuestBookUIFactory.generateNode(node,nodeContainer,this);
            }
            displayPrerequisites();
        }

        public void displayPrerequisites() {
            GlobalHelper.deleteAllChildren(lineContainer);
            HashSet<int> pageIds = new HashSet<int>();
            foreach (QuestBookNode node in currentPage.Nodes) {
                pageIds.Add(node.Id);
            }
            Dictionary<int, QuestBookNode> idNodeMap = library.IdNodeMap;
            foreach (QuestBookNode questBookNode in currentPage.Nodes) {
                foreach (int id in questBookNode.Prerequisites) {
                    if (!pageIds.Contains(id)) {
                        continue;
                    }
                    QuestBookNode otherNode = idNodeMap[id];
                    bool discovered = nodeDiscovered(questBookNode,idNodeMap);
                    QuestBookUIFactory.generateLine(questBookNode.Position,otherNode.Position,lineContainer,discovered);
                }
            }
        }

        private bool nodeDiscovered(QuestBookNode questBookNode, Dictionary<int, QuestBookNode> idNodeMap) {
            foreach (int prereqID in questBookNode.Prerequisites) {
                bool preReqComplete = idNodeMap[prereqID].Content.Task.getComplete();
                if (questBookNode.RequireAllPrerequisites && !preReqComplete)  {
                    return false;
                }
                if (!questBookNode.RequireAllPrerequisites && preReqComplete) {
                    return true;
                }
            }
            // If the loop has gotten to this point, there are two cases
            // i) If its RequireAllPrequestites, then all are complete so return RequireAllPrequresites aka true
            // ii) If its not RequireAllPrequesites, then atleast oen is not complete so return not RequireAllPrequreistes aka false
            return questBookNode.RequireAllPrerequisites;
        }

        public void displayCurrentPage() {
            displayPage(currentPage);
        }

        public void FixedUpdate() {
            
            handleRightClick();
        }

        public void Update() {
            handleZoom();
        }

        private void initEditMode() {
            GameObject prefab = Resources.Load<GameObject>(editModePath);
            if (prefab == null) {
                Debug.LogError("QuestBookUI edit mode prefab is null");
                return;
            }
            GameObject instianated = GameObject.Instantiate(prefab);
            editModeController = instianated.GetComponent<QuestEditModeController>();
            editModeController.transform.SetParent(transform,false);
            editModeController.init(this);
            
        }
        private void handleZoom() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = ContentContainer;
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

        private void handleRightClick() {
            if (!Input.GetMouseButton(1)) {
                return;
            }
            int maxPosition = 2000;
            int maxSpeed = 250;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 dif = (mousePosition - ContentMaskContainer.transform.position);
            dif.x = Mathf.Clamp(dif.x,-maxSpeed,maxSpeed);
            dif.y = Mathf.Clamp(dif.y,-maxSpeed,maxSpeed);
            Vector2 newPosition = contentContainer.position - dif * 0.05f;
            float clampedX = Mathf.Clamp(newPosition.x,-maxPosition,maxPosition);
            float clampedY = Mathf.Clamp(newPosition.y,-maxPosition,maxPosition);
            contentContainer.position = new Vector3(clampedX,clampedY);
        }
    }
    
}

