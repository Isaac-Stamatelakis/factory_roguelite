using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI.ConfirmPopup;
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
            WorldMetaData[] metaDataArray = new WorldMetaData[worlds.Length];
            for (int i = 0; i < worlds.Length; i++)
            {
                string worldName = Path.GetFileName(worlds[i].TrimEnd(Path.DirectorySeparatorChar));
                formattedWorlds[i] = worldName;
                WorldMetaData worldMetaData = WorldLoadUtils.GetWorldMetaData(worldName);
                metaDataArray[i] = worldMetaData;
                
            }
            var combined = formattedWorlds.Zip(metaDataArray, (world, meta) => new { World = world, Meta = meta });
            var sorted = combined.OrderByDescending(x => x.Meta.LastAccessDate);
            var sortedList = sorted.ToList();
            for (int i = 0; i < sortedList.Count(); i++)
            {
                string worldName = sortedList[i].World;
                worldNames.Add(worldName);
                DisplayWorld(worldName,i);
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
            string defaultWorldFolder =  Path.Combine(Application.persistentDataPath, WorldLoadUtils.DefaultWorldFolder);

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
            int index = worldNames.Count - 1;
            DisplayWorld(worldName, index);
            worldElements[index].transform.SetSiblingIndex(0);
        }

        private void DisplayWorld(string worldName, int index)
        {
            SelectWorldElement worldElement = Instantiate(selectWorldElementPrefab,elementList);
            WorldMetaData worldMetaData = WorldLoadUtils.GetWorldMetaData(worldName);
            WorldDisplayData worldDisplayData = new WorldDisplayData(worldName,worldMetaData.CreationDate, worldMetaData.LastAccessDate);
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
