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
        NanoBots
    }
    public class PlayerParticles
    {
        private PlayerRobot playerRobot;
        private ParticleSystem bonusJumpParticles;
        private ParticleSystem teleportParticles;
        private ParticleSystem nanoBotParticles;

        public PlayerParticles(PlayerRobot playerRobot, ParticleSystem bonusJumpParticles, ParticleSystem teleportParticles, ParticleSystem nanoBotParticles)
        {
            this.playerRobot = playerRobot;
            this.bonusJumpParticles = bonusJumpParticles;
            this.teleportParticles = teleportParticles;
            this.nanoBotParticles = nanoBotParticles;
        }
        

        public void PlayParticle(PlayerParticle particle)
        {
            switch (particle)
            {
                case PlayerParticle.BonusJump:
                    bonusJumpParticles.Play();
                    break;
                case PlayerParticle.Teleportation:
                    teleportParticles.Play();
                    break;
                case PlayerParticle.NanoBots:
                    nanoBotParticles.Play();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(particle), particle, null);
            }
        }
        
        
    }
}
