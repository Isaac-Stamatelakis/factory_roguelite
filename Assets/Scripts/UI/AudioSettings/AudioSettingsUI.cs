using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.AudioSettings
{
    public enum AudioSetting
    {
        Music,
        Tiles,
        Machines,
        UI
    }

    internal static class AudioSettingUtils
    {
        private const string PLAYER_PREF_PREFIX = "audio_";

        internal static void SaveAudioSetting(AudioSetting audioSetting, float val)
        {
            string key = PLAYER_PREF_PREFIX + audioSetting.ToString().ToLower();
            PlayerPrefs.SetFloat(key, val);
        }

        internal static float GetAudioSetting(AudioSetting audioSetting)
        {
            string key = PLAYER_PREF_PREFIX + audioSetting.ToString().ToLower();
            return PlayerPrefs.GetFloat(key);
        }

        internal static void InitializeAudioLevels()
        {
            var settingInteractableDictionary = GetAudioSettingInteractableDictionary();
            AudioSetting[] audioSettings = System.Enum.GetValues(typeof(AudioSetting)) as AudioSetting[];
            foreach (AudioSetting audioSetting in audioSettings)
            {
                if (!settingInteractableDictionary.TryGetValue(audioSetting, out List<AudioSettingInteractable> interactableList)) continue;
                float level = GetAudioSetting(audioSetting);
                foreach (AudioSettingInteractable audioSettingInteractable in interactableList)
                {
                    audioSettingInteractable.SetAudioLevel(level);
                }
            }
        }

        internal static Dictionary<AudioSetting, List<AudioSettingInteractable>> GetAudioSettingInteractableDictionary()
        {
            AudioSettingInteractable[] audioSettingInteractables = GameObject.FindObjectsOfType<AudioSettingInteractable>();
            Dictionary<AudioSetting, List<AudioSettingInteractable>> settingInteractableDictionary = new Dictionary<AudioSetting, List<AudioSettingInteractable>>();
            foreach (AudioSettingInteractable audioSettingInteractable in audioSettingInteractables)
            {
                AudioSetting? audioSetting = audioSettingInteractable.GetAudioSetting();
                if (audioSetting == null) continue;
                settingInteractableDictionary.TryAdd(audioSetting.Value,new List<AudioSettingInteractable>());
                settingInteractableDictionary[audioSetting.Value].Add(audioSettingInteractable);
            }

            return settingInteractableDictionary;
        }
    }
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] private Scrollbar mMusicScrollBar;
        [SerializeField] private Scrollbar mTilesScrollBar;
        [SerializeField] private Scrollbar mMachinesScrollBar;
        private Dictionary<AudioSetting, List<AudioSettingInteractable>> audioSettingInteractableDictionary;
        public void Start()
        {
            mBackButton.onClick.AddListener(CanvasController.Instance.PopStack);
            audioSettingInteractableDictionary = AudioSettingUtils.GetAudioSettingInteractableDictionary();
            InitializeScrollBar(mMusicScrollBar, AudioSetting.Music);
            InitializeScrollBar(mTilesScrollBar, AudioSetting.Tiles);
            InitializeScrollBar(mMachinesScrollBar, AudioSetting.Machines);
        }

        private void InitializeScrollBar(Scrollbar scrollbar, AudioSetting audioSetting)
        {
            float initial = AudioSettingUtils.GetAudioSetting(audioSetting);
            scrollbar.value = initial;
            TextMeshProUGUI textElement = scrollbar.GetComponentInChildren<TextMeshProUGUI>();
            textElement.text = GetTextValue(audioSetting, initial);
            scrollbar.onValueChanged.AddListener((value) =>
            {
                AudioSettingUtils.SaveAudioSetting(audioSetting,value);
                textElement.text = GetTextValue(audioSetting, value);
                if (!audioSettingInteractableDictionary.TryGetValue(audioSetting, out List<AudioSettingInteractable> interactableList)) return;
                foreach (AudioSettingInteractable audioSettingInteractable in interactableList)
                {
                    audioSettingInteractable.SetAudioLevel(value);
                }
            });
        }

        private string GetTextValue(AudioSetting audioSetting, float value)
        {
            string audioName = audioSetting.ToString().Replace("_", " ");
            return $"{audioName}:{value:P0}";
        }
    }
}
