using System;
using Chunks;
using Chunks.Systems;
using Dimensions;
using Item.Slot;
using PlayerModule;
using TileEntity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player.UI
{
    public class PlayerDeathScreenUI : MonoBehaviour
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
            GameObject.Destroy(gameObject);
        }

        private void TitleScreenClick()
        {
            RespawnClick();
            SceneManager.LoadScene("TitleScreen");

        }
    }
}
