using System;
using UI.TitleScreen.Select;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TitleScreen
{
    public class TitleScreenUI : MonoBehaviour
    {
        [SerializeField] private Button options;
        [SerializeField] private Button play;
        [SerializeField] private Button exit;
        [SerializeField] private TitleScreenOptionUI titleScreenOptionUIPrefab;
        [SerializeField] private SelectWorldUI selectWorldUIPrefab; 
        public void Start()
        {
            play.onClick.AddListener(() =>
            {
                CanvasController.Instance.DisplayObject(Instantiate(selectWorldUIPrefab).gameObject);
            });
            options.onClick.AddListener(() =>
            {
                CanvasController.Instance.DisplayObject(Instantiate(titleScreenOptionUIPrefab).gameObject);
            });
            exit.onClick.AddListener(Application.Quit);

        }
    }
}
