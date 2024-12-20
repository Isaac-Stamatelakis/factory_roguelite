using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WorldModule;

namespace UI.Title {
    public class WorldSelectUIController : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        [SerializeField]
        public GameObject titleScreenDocument;
        public GameObject worldEditDocument;
        private Label m_Label;
        private List<Button> slotButtons;
        private List<Button> editButtons;
        private Button m_BackButton;
        public void Start()
        {
            m_UIDocument = GetComponent<UIDocument>();
            var rootElement = m_UIDocument.rootVisualElement;

            slotButtons = new List<Button>();
            for (int n = 0; n < 3; n++) {
                int k = n;
                Button button = rootElement.Q<Button>("slot_" + k + "_button");
                if (WorldLoadUtils.defaultWorldExists("world" + n)) {
                    button.text = "Slot " + n;
                } else {
                    button.text = "Empty Slot";
                }
                button.clickable.clicked += () => slotPressed(k);
                slotButtons.Add(button);
            }
            editButtons = new List<Button>();
            for (int n = 0; n < 3; n++) {
                int k = n;
                Button button = rootElement.Q<Button>("slot_" + k + "_edit");
                button.clickable.clicked += () => editPressed(k);
                if (!WorldLoadUtils.defaultWorldExists("world" + n)) {
                    button.visible=false;
                }
                editButtons.Add(button);
            }
            m_BackButton = rootElement.Q<Button>("back_button");
            m_BackButton.clickable.clicked += goBack;
                
        }
        
        private void OnDestroy()
        {
            m_BackButton.clickable.clicked -= goBack;
        }
        
        private void slotPressed(int n) {
            string worldName = "world" + n;
            if (!WorldLoadUtils.defaultWorldExists(worldName)) {
                StartCoroutine(createAndLoadWorld(worldName));
                 //slotButtons[n].name = "Slot " + n;
            } else {
                OpenWorld.LoadWorld(worldName);
            }
        }

        private IEnumerator createAndLoadWorld(string worldName) {
            yield return StartCoroutine(WorldCreation.CreateWorld(worldName));
            OpenWorld.LoadWorld(worldName);
        }

        private void editPressed(int n) {
            GameObject editWorldPage = GameObject.Instantiate(worldEditDocument);
            WorldEditUIController worldEditUIController = editWorldPage.GetComponent<WorldEditUIController>();
            worldEditUIController.WorldName = "world" + n;
            GameObject.Destroy(gameObject);
        }

        private void goBack() {
            GameObject.Instantiate(titleScreenDocument);
            GameObject.Destroy(gameObject);
        }
        private void onQuitClicked() {
            Application.Quit();
        }
        private void onOptionsClicked() {
            Debug.Log("Navigate to options");
        }
    }
}

