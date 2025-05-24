using UnityEngine;

namespace Player
{
    public class PlayerDamage
    {
        private float timeSinceDamaged = 0;
        private PlayerRobot playerRobot;

        public PlayerDamage(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
        }

        public void IterateDamageTime(float deltaTime)
        {
            timeSinceDamaged += deltaTime;
        }
        public bool Damage(float amount)
        {
            if (!IsDamageable()) return false;
            playerRobot.ResetInvinicibleFrames();
            playerRobot.ResetLiveYFrames();
            playerRobot.RobotData.Health -= amount;
            timeSinceDamaged = 0;
            if (playerRobot.RobotData.Health > 0) return true;
            
            playerRobot.Die();
            return false;
        }

        private bool IsDamageable()
        {
            return playerRobot.InvincibilityFrames <= 0 && playerRobot.RobotData.Health >= 0 && !playerRobot.DevMode.noHit;
        }
    }
}
