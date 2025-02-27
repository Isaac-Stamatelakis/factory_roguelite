using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Robot.Upgrades.Instances.RocketBoots
{
    public class RocketBoots
    {
        public float FlightTime;
        public float Boost;
        public bool Active = false;
        public ParticleSystem RocketParticles;

        private const float UPWARDS_ACCELERATION = 8f;
        private const float DOWNWARDS_ACCERATION = 100f;
        private const float MAX_SPEED = 8f;

        public void UpdateBoost(bool spaceDown)
        {
            if (spaceDown)
            {
                if (FlightTime > 0)
                {
                    Boost += UPWARDS_ACCELERATION * Time.deltaTime;
                    if (RocketParticles)
                    {
                        var emission = RocketParticles.emission;
                        emission.enabled = true;
                    }
                    if (Boost > MAX_SPEED) Boost = MAX_SPEED;
                }
                FlightTime -= Time.deltaTime;
                return;
            }
            if (RocketParticles)
            {
                var emission = RocketParticles.emission;
                emission.enabled = false;
            }
            Boost -= DOWNWARDS_ACCERATION * Time.deltaTime;
            if (Boost < 0) Boost = 0;
        }

        public IEnumerator Activate(AssetReference assetReference, Transform playerTransform)
        {
            Active = true;
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;
            RocketParticles = GameObject.Instantiate(handle.Result, playerTransform,false).GetComponent<ParticleSystem>();
            Addressables.Release(handle);
        }

        public void Terminate()
        {
            Active = false;
            if (RocketParticles)
            {
                GameObject.Destroy(RocketParticles.gameObject);
            }
        }
    }
}
