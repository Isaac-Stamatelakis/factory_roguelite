using UnityEngine;

namespace Robot.Upgrades.Instances.RocketBoots
{
    public class RocketBoots
    {
        public float FlightTime;
        public float Boost;
        public bool Active = false;

        private const float UPWARDS_ACCELERATION = 5f;
        private const float DOWNWARDS_ACCERATION = 5f;
        private const float MAX_SPEED = 2f;

        public void UpdateBoost(bool spaceDown)
        {
            if (spaceDown)
            {
                if (FlightTime > 0)
                {
                    Boost += UPWARDS_ACCELERATION * Time.deltaTime; 
                    if (Boost > MAX_SPEED) Boost = MAX_SPEED;
                }
                FlightTime -= Time.deltaTime;
                return;
            }
            
            Boost -= DOWNWARDS_ACCERATION * Time.deltaTime;
            if (Boost < 0) Boost = 0;
            
        }
    }
}
