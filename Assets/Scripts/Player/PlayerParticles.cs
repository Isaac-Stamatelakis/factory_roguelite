using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Player
{
    public enum PlayerParticle
    {
        BonusJump
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(particle), particle, null);
            }
        }
        
        
    }
}
