using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using WorldModule.Caves;

namespace DevTools.Structures {
    public class StructureDevControllerUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup list;
        [SerializeField] private Button addButton;
        [SerializeField] private StructureSelectorUI listElementPrefab;
        [SerializeField] private NewStructurePopUpUI newStructurePopUpUIPrefab;
        public void init() {
            addButton.onClick.AddListener(() => {
                NewStructurePopUpUI newStructurePopUpUI = GameObject.Instantiate(newStructurePopUpUIPrefab);
                newStructurePopUpUI.init(this);
                newStructurePopUpUI.transform.SetParent(transform,false);
            });
            displayList();
        }

        public void displayList() {
            GlobalHelper.deleteAllChildren(list.transform);
            string directoryPath = StructureGeneratorHelper.getFolderPath();
            string[] folders = Directory.GetDirectories(directoryPath);
            List<FolderInfo> folderValues = new List<FolderInfo>();
            foreach (string folderPath in folders)
            {
                string folderName = Path.GetFileName(folderPath);
                DateTime lastModified = Directory.GetLastWriteTime(folderPath);
                folderValues.Add(new FolderInfo(folderName,lastModified));
                
            }
            folderValues = folderValues.OrderByDescending(f => f.lastModified).ToList();
            foreach (FolderInfo folderInfo in folderValues) {
                StructureSelectorUI listElement = GameObject.Instantiate(listElementPrefab);
                listElement.init(this, folderInfo.name,folderInfo.lastModified.ToString());
                listElement.transform.SetParent(list.transform);
            }
        }

        private class FolderInfo {
            public string name;
            public DateTime lastModified;
            public FolderInfo(string name, DateTime lastModified) {
                this.name = name;
                this.lastModified = lastModified;
            }
        }
    }
}

