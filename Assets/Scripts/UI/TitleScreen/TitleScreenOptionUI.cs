using UnityEngine;
using UnityEngine.UI;

namespace UI.TitleScreen
{
    public class TitleScreenOptionUI : MonoBehaviour
    {
        [SerializeField] private Button videoButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button discordButton;
        [SerializeField] private Button backButton;
        [SerializeField] private UIAssetManager uiAssetManager;

        public void Start()
        {
            controlsButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
            audioButton.onClick.AddListener(() =>
            {
                uiAssetManager.DisplayObject("CONTROLS");
            });
            videoButton.onClick.AddListener(() =>
                        {
                            uiAssetManager.DisplayObject("CONTROLS");
                        });
            discordButton.onClick.AddListener(() =>
            {
                Application.OpenURL("https://discord.com/");
            });
            backButton.onClick.AddListener(CanvasController.Instance.PopStack);
            
        }
    }
}
