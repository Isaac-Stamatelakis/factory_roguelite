using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI.ConfirmPopup;
using UI.TitleScreen.Backup;
using UnityEngine;
using UnityEngine.UI;
using World.Serialization;
using WorldModule;

namespace UI.TitleScreen.Select
{
    public class SelectWorldUI : MonoBehaviour
    {
        [SerializeField] private SelectWorldElement selectWorldElementPrefab;
        [SerializeField] private Button playButton;
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button unselectButton;
        [SerializeField] private Button createButton;
        [SerializeField] private BackupUI backupUIPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Color highlightColor;
        [SerializeField] private Transform elementList;
        [SerializeField] private WorldCreationUI worldCreationUIPrefab;
        [SerializeField] private ConfirmPopupUI confirmPopupUIPrefab;
        private List<Button> selectRequireButtons;
        private int selected = -1;
        private List<SelectWorldElement> worldElements = new List<SelectWorldElement>();
        private List<string> worldNames = new List<string>();
        
        public void Start()
        {
            selectRequireButtons = new List<Button>
            {
                playButton,
                editButton,
                deleteButton,
                restoreButton,
                unselectButton
            };
            
            playButton.onClick.AddListener(() =>
            {
                PlayWorld(selected);
            });
            unselectButton.onClick.AddListener(() =>
            {
                if (selected >= 0)
                {
                    worldElements[selected].SetHighlight(null);
                }
                selected = -1;
                SetButtonInteractility(false);
                
            });
            createButton.onClick.AddListener(() =>
            {
                WorldCreationUI worldCreationUI = Instantiate(worldCreationUIPrefab);
                worldCreationUI.Initalize(AddWorld);
                CanvasController.Instance.DisplayObject(worldCreationUI.gameObject);
            });
            
            restoreButton.onClick.AddListener(() =>
            {
                BackupUI backupUI = Instantiate(backupUIPrefab);
                backupUI.Display(worldNames[selected]);
                CanvasController.Instance.DisplayObject(backupUI.gameObject);
            });

            
            deleteButton.onClick.AddListener(() =>
            {
                ConfirmPopupUI confirmPopupUI = Instantiate(confirmPopupUIPrefab);
                void DeleteAction() => DeleteWorld(selected);
                confirmPopupUI.Display(DeleteAction, "Are you sure you want to delete this world?");
            });
            SetButtonInteractility(false);
            backButton.onClick.AddListener(CanvasController.Instance.PopStack);
            string[] worlds = GetWorlds();
            string[] formattedWorlds = new string[worlds.Length];
            List<WorldMetaData> worldMetaDataList = new List<WorldMetaData>();
            for (int i = 0; i < worlds.Length; i++)
            {
                string worldName = Path.GetFileName(worlds[i].TrimEnd(Path.DirectorySeparatorChar));
                formattedWorlds[i] = worldName;
                try
                {
                    WorldMetaData worldMetaData = WorldLoadUtils.GetWorldMetaData(worldName);
                    worldMetaDataList.Add(worldMetaData);
                }
                catch (IOException e)
                {
                    worldMetaDataList.Add(null);
                    Debug.LogWarning($"Corrupted Meta Data {e}");
                }
                
            }
            var combined = formattedWorlds.Zip(worldMetaDataList, (world, meta) => new { World = world, Meta = meta });
            var sorted = combined.OrderByDescending(x => x.Meta?.LastAccessDate);
            var sortedList = sorted.ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                WorldMetaData worldMetaData = sortedList[i].Meta;
                string worldName = sortedList[i].World;
                worldNames.Add(worldName);
                DisplayWorld(worldName,i,worldMetaData);
            }
            
        }

        private void DeleteWorld(int index)
        {
            WorldCreation.DeleteWorld(worldNames[selected]);
            worldNames.RemoveAt(index);
            Destroy(worldElements[index].gameObject);
            worldElements.RemoveAt(index);
        }

        private string[] GetWorlds()
        {
            string defaultWorldFolder =  Path.Combine(Application.persistentDataPath, WorldLoadUtils.DEFAULT_WORLD_FOLDER);
            if (!Directory.Exists(defaultWorldFolder))
            {
                Directory.CreateDirectory(defaultWorldFolder);
            }
            try
            {
                return Directory.GetDirectories(defaultWorldFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return Array.Empty<string>();
            }
        }

        private void AddWorld(string worldName)
        {
            worldNames.Add(worldName);
            WorldMetaData worldMetaData = WorldLoadUtils.GetWorldMetaData(worldName);
            int index = worldNames.Count - 1;
            DisplayWorld(worldName, index, worldMetaData);
            worldElements[index].transform.SetSiblingIndex(0);
        }

        private void DisplayWorld(string worldName, int index, WorldMetaData worldMetaData)
        {
            SelectWorldElement worldElement = Instantiate(selectWorldElementPrefab,elementList,false);
            
            WorldDisplayData worldDisplayData = new WorldDisplayData(worldName,worldMetaData?.CreationDate, worldMetaData?.LastAccessDate, worldMetaData==null);
            worldElement.Initalize(index,ClickWorldElement,worldDisplayData);
            worldElements.Add(worldElement);
        }

        private void SetButtonInteractility(bool state)
        {
            foreach (Button requireButton in selectRequireButtons)
            {
                requireButton.interactable = state;
            }
        }

        private void PlayWorld(int index)
        {
            OpenWorld.LoadWorld(worldNames[index]);
        }

        private void ClickWorldElement(int index)
        {
            if (selected == index)
            {
                PlayWorld(index);
                return;
            }
            SetButtonInteractility(true);
            if (selected >= 0)
            {
                worldElements[selected].SetHighlight(null);
            }
            selected = index;
            worldElements[index].SetHighlight(highlightColor);
            
        }
    }
}
