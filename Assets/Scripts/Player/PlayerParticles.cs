using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Player
{
    public enum PlayerParticle
    {
        BonusJump,
        Teleportation,
        NanoBots,
        RocketBoots
    }
    [System.Serializable]
    public class PlayerParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem bonusJumpParticles;
        [SerializeField] private ParticleSystem teleportParticles;
        [SerializeField] private ParticleSystem nanoBotParticles;
        [SerializeField] private ParticleSystem rocketBootParticles;

        
        public void PlayParticle(PlayerParticle particle)
        {
            ParticleSystem particles = GetParticleSystem(particle);
            if (!particles.isEmitting)
            {
                particles.Play();
            }
        }

        private ParticleSystem GetParticleSystem(PlayerParticle particle)
        {
            return particle switch
            {
                PlayerParticle.BonusJump => bonusJumpParticles,
                PlayerParticle.Teleportation => teleportParticles,
                PlayerParticle.NanoBots => nanoBotParticles,
                PlayerParticle.RocketBoots => rocketBootParticles,
                _ => throw new ArgumentOutOfRangeException(nameof(particle), particle, null)
            };
        }
        
        
    }
}
