using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WorldModule;
using System.IO;

namespace UI.Title {
    public class WorldEditUIController : MonoBehaviour
    {
        private UIDocument m_UIDocument;
        [SerializeField]
        public GameObject worldSelectObject;
        private Button m_BackButton;
        private Button m_deleteButton;
        private string worldName;

        public string WorldName { get => worldName; set => worldName = value; }

        public void Start()
        {
            m_UIDocument = GetComponent<UIDocument>();
            var rootElement = m_UIDocument.rootVisualElement;
            
            m_BackButton = rootElement.Q<Button>("back_button");
            m_BackButton.clickable.clicked += goBack;

            m_deleteButton = rootElement.Q<Button>("delete_button");
            m_deleteButton.clickable.clicked += delete;
                
        }
        
        private void OnDestroy()
        {
            m_BackButton.clickable.clicked -= goBack;
            m_deleteButton.clickable.clicked -= delete;
        }


        private void delete() {
            string path = WorldLoadUtils.getDefaultWorldPath(worldName);
            Directory.Delete(path, true);
            Debug.Log(worldName + " Deleted");
            goBack();
        }
        private void goBack() {
            GameObject.Instantiate(worldSelectObject);
            GameObject.Destroy(gameObject);
        }
    }
}

