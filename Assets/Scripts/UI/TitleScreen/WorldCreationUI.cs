using System;
using System.Collections;
using System.IO;
using TMPro;
using UI.QuestBook;
using UnityEngine;
using UnityEngine.UI;
using WorldModule;

namespace UI.TitleScreen
{
    
    public class WorldCreationUI : MonoBehaviour
    {
        private enum WorldPreset
        {
            Standard,
            CompactMachineDemo
        }
        [SerializeField] private Button createButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Toggle cheatToggle;
        [SerializeField] private TMP_Dropdown presetDropdown;
        private Action<string> createCallback;
        // Start is called before the first frame update
        public void Initalize(Action<string> callback)
        {
            this.createCallback = callback;
        }
        public void Start()
        {
            exitButton.onClick.AddListener(CanvasController.Instance.PopStack);
            createButton.onClick.AddListener(() =>
            {
                StartCoroutine(CreateWorldClick());
            });
            if (WorldCreation.ENABLE_PRESETS)
            {
                presetDropdown.options = GlobalHelper.EnumToDropDown<WorldPreset>();
            }
            presetDropdown.gameObject.SetActive(WorldCreation.ENABLE_PRESETS);
            
        }

        private IEnumerator CreateWorldClick()
        {
            if (nameInputField.text.Length == 0)
            {
                yield break;
            }

            string worldName = nameInputField.text.ToLower().Replace(" ","_");
            while (true)
            {
                string worldPath = WorldLoadUtils.GetWorldPath(worldName);
                int count = 0;
                if (!Directory.Exists(worldPath))
                {
                    break;
                }
                count++;
                worldName = $"{worldName}_{count}";
            }
            WorldPreset preset = WorldCreation.ENABLE_PRESETS ? (WorldPreset)presetDropdown.value : WorldPreset.Standard;
            WorldPresetData worldPresetData = GetPresetData(preset);
            WorldCreationData worldCreationData = new WorldCreationData(worldName, worldPresetData.StructureName, cheatToggle.isOn, worldPresetData.QuestBookName);
            yield return StartCoroutine(WorldCreation.CreateWorld(worldCreationData));
            createCallback?.Invoke(worldName);
            CanvasController.Instance.PopStack();
        }

        private WorldPresetData GetPresetData(WorldPreset worldPreset)
        {
            switch (worldPreset)
            {
                case WorldPreset.Standard:
                    return new WorldPresetData(WorldCreation.DIM_0_STRUCTURE_NAME,QuestBookUtils.MAIN_QUEST_BOOK_NAME);
                case WorldPreset.CompactMachineDemo:
                    return new WorldPresetData(WorldCreation.DIM_0_STRUCTURE_NAME,QuestBookUtils.MAIN_QUEST_BOOK_NAME);
                default:
                    throw new ArgumentOutOfRangeException(nameof(worldPreset), worldPreset, null);
            }
        }

        private struct WorldPresetData
        {
            public string StructureName;
            public string QuestBookName;

            public WorldPresetData(string structureName, string questBookName)
            {
                StructureName = structureName;
                QuestBookName = questBookName;
            }
        }
        
    }
}
