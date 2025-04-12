using System;
using Entities.Mobs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Mob
{
    public class MobEntityParticleController : MonoBehaviour
    {
        private ParticleSystem DeathParticles;

        public void Initialize(ParticleSystem prefab)
        {
            DeathParticles = Instantiate(prefab,transform,false);
        }

        public void PlayDeathParticles(Vector2 position, MobEntity.MobDeathParticles particles)
        {
            if (particles == MobEntity.MobDeathParticles.None) return;
            DeathParticles.transform.position = position;
            DeathParticles.Play();
        }
    }
}
