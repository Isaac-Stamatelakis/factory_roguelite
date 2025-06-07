using System;
using Chunks;
using Chunks.Systems;
using Dimensions;
using Item.Slot;
using PlayerModule;
using TileEntity;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player.UI
{
    public interface IExitActionlessUI
    {
        
    }
    public class PlayerDeathScreenUI : MonoBehaviour, IExitActionlessUI
    {
        [SerializeField] private Button mRespawnButton;
        [SerializeField] private Button mTitleScreenButton;

        private PlayerScript playerScript;
        public void Start()
        {
            mRespawnButton.onClick.AddListener(RespawnClick);
            mTitleScreenButton.onClick.AddListener(TitleScreenClick);
        }

        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }
        private void RespawnClick()
        {
            playerScript.PlayerRobot.Respawn();
            CanvasController.Instance.PopStack();
        }

        private void TitleScreenClick()
        {
            RespawnClick();
            SceneManager.LoadScene("TitleScreen");

        }
    }
}
