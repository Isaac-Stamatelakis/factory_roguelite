using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using WorldModule;

#if UNITY_EDITOR
    
namespace DevTools.Structures {
    public class StructureSelectorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI date;
        [SerializeField] private Button editButton;
        [SerializeField] private Button downloadButton;
        [SerializeField] private EditStructurePopUpUI editStructurePopUpUIPrefab;
        private StructureDevControllerUI structureDevControllerUI;
        public void init(StructureDevControllerUI structureDevControllerUI, string title, string date) {
            this.structureDevControllerUI = structureDevControllerUI;
            this.title.text = title;
            this.date.text = date;
            editButton.onClick.AddListener(() => {
                EditStructurePopUpUI editStructurePopUpUI = GameObject.Instantiate(editStructurePopUpUIPrefab);
                editStructurePopUpUI.transform.SetParent(transform.parent.parent.parent.parent,false); // Don't ask it works
                editStructurePopUpUI.init(this);
            });
            downloadButton.onClick.AddListener(() => {
                
            });
            GetComponent<Button>().onClick.AddListener(() => {
                string path = StructureGeneratorHelper.getPath(this.title.text);
                WorldManager.getInstance().setWorldPath(path);
                SceneManager.LoadScene("StructureGenerator");
            });
        }

        public void setTitle(string newTitle) {
            if (newTitle.Equals(title.text)) {
                return;
            }
            string oldPath = Path.Combine(StructureGeneratorHelper.getFolderPath(),title.text);
            string newPath = Path.Combine(StructureGeneratorHelper.getFolderPath(),newTitle);
            Directory.Move(oldPath, newPath);
            structureDevControllerUI.displayList();
        }
        public void deleteSelf() {
            Directory.Delete(Path.Combine(StructureGeneratorHelper.getFolderPath(),title.text),true);
            structureDevControllerUI.displayList();
        }
        public string getTitle() {
            return title.text;
        }
    }
}

#endif