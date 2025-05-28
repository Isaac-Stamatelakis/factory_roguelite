using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Robot.Upgrades.Instances.RocketBoots
{
    public class RocketBoots
    {
        public float FlightTime;
        public float Boost;
        private PlayerParticles playerParticles;

        private const float UPWARDS_ACCELERATION = 8f;
        private const float DOWNWARDS_ACCERATION = 100f;
        private const float MAX_SPEED = 8f;

        public RocketBoots(PlayerRobot playerRobot)
        {
            this.playerParticles = playerRobot.PlayerParticles;
        }

        public void UpdateBoost(bool spaceDown)
        {
            if (spaceDown)
            {
                if (FlightTime > 0)
                {
                    Boost += UPWARDS_ACCELERATION * Time.deltaTime;
                    playerParticles.PlayParticle(PlayerParticle.RocketBoots);
                    if (Boost > MAX_SPEED) Boost = MAX_SPEED;
                }
                FlightTime -= Time.deltaTime;
                return;
            }
            Boost -= DOWNWARDS_ACCERATION * Time.deltaTime;
            if (Boost < 0) Boost = 0;
        }

        public void SetFlightTime(int upgrades)
        {
            FlightTime = 1 + upgrades;
        }
        
    }
}
