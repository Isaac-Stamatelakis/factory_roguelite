using UnityEngine;

namespace Player.Movement
{
    public class PlayerPlatformTrigger : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
        }
        public void OnTriggerEnter2D(Collider2D other)
        {
            playerRobot.OnPlatform = true;
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.OnPlatform = false;
        }
    }
}
