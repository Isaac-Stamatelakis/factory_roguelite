using System;
using System.Collections.Generic;
using System.IO;
using UI.ConfirmPopup;
using UnityEngine;
using UnityEngine.UI;
using World.BackUp;
using WorldModule;

namespace UI.TitleScreen.Backup
{
    public class BackupUI : MonoBehaviour
    {
        [SerializeField] private BackupListElementUI backupListElementUIPrefab;
        [SerializeField] private ConfirmPopupUI confirmPopupUIPrefab;
        [SerializeField] private VerticalLayoutGroup mList;
        [SerializeField] private Button mBackButton;
        [SerializeField] private Button mRestoreButton;
        [SerializeField] private Button mDeleteButton;
        [SerializeField] private Color highlightColor;

        private List<string> backupFiles;
        private List<BackupListElementUI> backupListElementUIs;
        private int selectedIndex = -1;
        private string worldName;

        public void Start()
        {
            mRestoreButton.interactable = false;
            mDeleteButton.interactable = false;
        }

        public void Display(string worldName)
        {
            this.worldName = worldName;
            backupFiles = new List<string>();
            backupListElementUIs = new List<BackupListElementUI>();
            
            mRestoreButton.onClick.AddListener(RestorePress);
            mDeleteButton.onClick.AddListener(DeletePress);
            mBackButton.onClick.AddListener(CanvasController.Instance.PopStack);
            
            string backupFolderPath = WorldLoadUtils.GetBackUpPath(worldName);
            string[] dateFolders = Directory.GetDirectories(backupFolderPath);
            int index = 0;
            foreach (string dateFolder in dateFolders)
            {
                string date = Path.GetFileName(dateFolder);
                string[] backupPaths = Directory.GetDirectories(dateFolder);
                foreach (string backUpPath in backupPaths)
                {
                    backupFiles.Add(backUpPath);
                    BackupListElementUI backupListElementUI = Instantiate(backupListElementUIPrefab, mList.transform);
                    backupListElementUIs.Add(backupListElementUI);
                    
                    
                    string time = Path.GetFileName(backUpPath);
                    backupListElementUI.Display(date,time,index,SelectElement);
                    index++;
                }
            }
        }

        private void SelectElement(int index)
        {
            
            if (selectedIndex >= 0)
            {
                backupListElementUIs[selectedIndex].GetComponent<Image>().color = backupListElementUIPrefab.GetComponent<Image>().color;
            }
            if (selectedIndex == index)
            {
                mRestoreButton.interactable = false;
                mDeleteButton.interactable = false;
                selectedIndex = -1;
                return;
            }

            selectedIndex = index;
            backupListElementUIs[selectedIndex].GetComponent<Image>().color = highlightColor;
            mRestoreButton.interactable = true;
            mDeleteButton.interactable = true;
        }

        private void RestorePress()
        {
            ConfirmPopupUI confirmPopupUI = GameObject.Instantiate(confirmPopupUIPrefab);
            confirmPopupUI.Display(
                () =>
                {
                    WorldBackUpUtils.RestoreBackUp(worldName, backupFiles[selectedIndex]);
                }
                ,$"Are you sure you want to restore the backup at path '{backupFiles[selectedIndex]}'?"
            );
        }

        private void DeletePress()
        {
            ConfirmPopupUI confirmPopupUI = GameObject.Instantiate(confirmPopupUIPrefab);
            confirmPopupUI.Display(Delete,$"Are you sure you want to delete the backup at path '{backupFiles[selectedIndex]}'?");
        }

        private void Delete()
        {
            if (selectedIndex < 0) return;
            
            Directory.Delete(backupFiles[selectedIndex], true);
            BackupListElementUI backupListElementUI = backupListElementUIs[selectedIndex];
            GameObject.Destroy(backupListElementUI.gameObject);
            selectedIndex = -1;
        }
        
    }
}
