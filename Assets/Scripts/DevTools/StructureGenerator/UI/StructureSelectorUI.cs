using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using WorldModule;
using WorldModule.Caves;

#if UNITY_EDITOR
    
namespace DevTools.Structures {
    public class StructureSelectorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI date;
        [SerializeField] private Button editButton;
        [SerializeField] private EditStructurePopUpUI editStructurePopUpUIPrefab;
        private StructureDevControllerUI structureDevControllerUI;
        public void init(StructureDevControllerUI structureDevControllerUI, string title, string date) {
            WorldLoadUtils.UsePersistentPath = false;
            this.structureDevControllerUI = structureDevControllerUI;
            this.title.text = title;
            this.date.text = date;
            editButton.onClick.AddListener(() => {
                EditStructurePopUpUI editStructurePopUpUI = GameObject.Instantiate(editStructurePopUpUIPrefab);
                editStructurePopUpUI.transform.SetParent(transform.parent.parent.parent.parent,false); // Don't ask it works
                editStructurePopUpUI.init(this);
            });
            GetComponent<Button>().onClick.AddListener(() => {
                string path = StructureGeneratorHelper.GetStructurePath(this.title.text);
                
                WorldManager.getInstance().SetWorldName(path);
                WorldManager.getInstance().WorldLoadType = WorldManager.WorldType.Structure;
                WorldLoadUtils.UsePersistentPath = false;
                SceneManager.LoadScene("MainScene");
            });
        }

        public void setTitle(string newTitle) {
            if (newTitle.Equals(title.text)) {
                return;
            }
            string oldPath = Path.Combine(StructureGeneratorHelper.GetFolderPath(),title.text);
            string newPath = Path.Combine(StructureGeneratorHelper.GetFolderPath(),newTitle);
            Directory.Move(oldPath, newPath);
            structureDevControllerUI.displayList();
        }
        public void deleteSelf() {
            Directory.Delete(Path.Combine(StructureGeneratorHelper.GetFolderPath(),title.text),true);
            structureDevControllerUI.displayList();
        }
        public string getTitle() {
            return title.text;
        }
    }
}

#endif