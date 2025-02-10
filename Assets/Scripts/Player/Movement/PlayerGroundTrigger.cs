using System;
using UnityEngine;

namespace Player.Movement
{
    public class PlayerGroundTrigger : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            playerRobot.OnBlock = true;
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.OnBlock = false;
        }
    }
}
