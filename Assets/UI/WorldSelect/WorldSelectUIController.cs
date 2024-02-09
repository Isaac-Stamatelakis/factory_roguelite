using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WorldDataModule;

namespace UI.Title {
    public class WorldSelectUIController : MonoBehaviour
    {
        [SerializeField]
        public UIDocument m_UIDocument;
        [SerializeField]
        public GameObject titleScreenDocument;
        private Label m_Label;
        private List<Button> slotButtons;
        private Button m_BackButton;
        public void Start()
        {
            var rootElement = m_UIDocument.rootVisualElement;

            slotButtons = new List<Button>();
            for (int n = 0; n < 3; n++) {
                int k = n;
                Button button = rootElement.Q<Button>("slot_" + k + "_button");
                if (WorldCreation.worldExists("world" + n)) {
                    button.text = "Slot " + n;
                } else {
                    button.text = "Empty Slot " + n;
                }
                
                button.clickable.clicked += () => slotPressed(k);
            }
            m_BackButton = rootElement.Q<Button>("back_button");
            m_BackButton.clickable.clicked += goBack;
                
        }
        
        private void OnDestroy()
        {
            /*
            foreach (Button button in slotButtons) {
                button.clickable.clicked
            }
            */
            m_BackButton.clickable.clicked -= goBack;
        }
        
        private void slotPressed(int n) {
            string name = "world" + n;
            if (!WorldCreation.worldExists(name)) {
                WorldCreation.createWorld(name);
                 //slotButtons[n].name = "Slot " + n;
            }
            OpenWorld.loadWorld(name);
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

