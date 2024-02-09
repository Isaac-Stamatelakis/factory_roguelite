using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Title {
    public class TitleScreenUIHandler : MonoBehaviour
    {
        [SerializeField]
        public UIDocument m_UIDocument;
        [SerializeField]
        public GameObject worldSelectDocument;
        private Label m_Label;
        private Button m_PlayButton;
        private Button m_QuitButton;
        private Button m_OptionButton;
        public void Start()
        {
            var rootElement = m_UIDocument.rootVisualElement;

            m_PlayButton = rootElement.Q<Button>("play_button");
            m_PlayButton.clickable.clicked += onPlayClicked;

            m_QuitButton = rootElement.Q<Button>("quit_button");
            m_QuitButton.clickable.clicked += onQuitClicked;
            
            m_OptionButton = rootElement.Q<Button>("options_button");
            m_OptionButton.clickable.clicked += onOptionsClicked;
                
        }
        
        private void OnDestroy()
        {
            m_PlayButton.clickable.clicked -= onPlayClicked;
            m_QuitButton.clickable.clicked -= onQuitClicked;
            m_OptionButton.clickable.clicked -= onOptionsClicked;
        }
        
        private void onPlayClicked()
        {
            GameObject.Instantiate(worldSelectDocument);
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

