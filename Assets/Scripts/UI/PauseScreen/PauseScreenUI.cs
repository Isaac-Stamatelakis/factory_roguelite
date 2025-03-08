using System;
using Player.Controls.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WorldModule;

namespace UI.PauseScreen
{
    public class PauseScreenUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button titleScreenButton;
        [SerializeField] private Button controlButton;
        [SerializeField] private Button statisticsButton;
        [SerializeField] private Button videoSettingButton;
        [SerializeField] private Button audioSettingButton;
        [SerializeField] private UIAssetManager uiAssetManager;

        public void Start()
        {
            uiAssetManager.load();
            resumeButton.onClick.AddListener(MainCanvasController.Instance.PopStack);
            WorldManager worldManager = WorldManager.getInstance();

            if (worldManager?.WorldLoadType == WorldManager.WorldType.Structure)
            {
                TextMeshProUGUI titleText = titleScreenButton.GetComponentInChildren<TextMeshProUGUI>();
                titleText.text = "Dev Tools";
            }
            
            
            titleScreenButton.onClick.AddListener(() =>
            {
                
                switch (worldManager?.WorldLoadType)
                {
                    case WorldManager.WorldType.Default:
                        SceneManager.LoadScene("TitleScreen");
                        break;
                    case WorldManager.WorldType.Structure:
                        SceneManager.LoadScene("DevTools");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            });
            controlButton.onClick.AddListener(() =>
            {
                Display("CONTROLS");
            });
            statisticsButton.onClick.AddListener(() =>
            {
                Display("STATS");
            });
            videoSettingButton.onClick.AddListener(() =>
            {
                Display("VIDEO");
            });
            audioSettingButton.onClick.AddListener(() =>
            {
                Display("AUDIO");
            });
        }

        private void Display(string key)
        {
            uiAssetManager.DisplayObject(key,priority:1);
        }

        public void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
