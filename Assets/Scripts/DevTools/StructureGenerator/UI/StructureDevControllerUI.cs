using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using WorldModule.Caves;

namespace DevTools.Structures {
    public abstract class DevToolListUI : MonoBehaviour
    {
        [SerializeField] protected VerticalLayoutGroup mList;
        [SerializeField] protected Button mAddButton;

        protected abstract void OnAddButtonClick();
        public abstract void DisplayList();
        public void Initialize() {
            mAddButton.onClick.AddListener(OnAddButtonClick);
            DisplayList();
        }
    }
    public class StructureDevControllerUI : DevToolListUI
    {
        [SerializeField] private StructureSelectorUI listElementPrefab;
        [SerializeField] private NewStructurePopUpUI newStructurePopUpUIPrefab;
        
        private class FolderInfo {
            public string name;
            public DateTime lastModified;
            public FolderInfo(string name, DateTime lastModified) {
                this.name = name;
                this.lastModified = lastModified;
            }
        }

        protected override void OnAddButtonClick()
        {
            NewStructurePopUpUI newStructurePopUpUI = GameObject.Instantiate(newStructurePopUpUIPrefab);
            newStructurePopUpUI.Init(this);
            newStructurePopUpUI.transform.SetParent(transform,false);
        }

        public override void DisplayList()
        {
            GlobalHelper.DeleteAllChildren(mList.transform);
            string directoryPath = StructureGeneratorHelper.GetFolderPath();
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
                listElement.transform.SetParent(mList.transform);
            }
        }
    }
}

