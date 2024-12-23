using System;
using Player.Controls.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            titleScreenButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScreen");
            });
            controlButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
            statisticsButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
            videoSettingButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
            audioSettingButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
        }

        public void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
