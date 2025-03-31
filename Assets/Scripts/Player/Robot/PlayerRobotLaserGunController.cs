using System;
using System.Collections.Generic;
using Player.Tool;
using Robot.Tool.Instances;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using UnityEngine;

namespace Player.Robot
{
    public class PlayerRobotLaserGunController : MonoBehaviour
    {
        private static readonly int Shooting = Animator.StringToHash("Shooting");
        private const string LASER_SHOOT_ANIMATION = "ShootLaser";
        private Animator animator;
        private SpriteRenderer robotSpriteRenderer;
        private SpriteRenderer spriteRenderer;
        private Vector3 defaultPosition;
        private PlayerRobot playerRobot;
        public void Initialize(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
            robotSpriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            defaultPosition = transform.localPosition;
        }
        public void Update()
        {
            spriteRenderer.flipX = robotSpriteRenderer.flipX;
            if (robotSpriteRenderer.flipX && transform.localPosition.x > 0)
            {
                transform.localPosition = defaultPosition + Vector3.left * (2 * defaultPosition.x);
            } else if (!robotSpriteRenderer.flipX && transform.localPosition.x < 0)
            {
                transform.localPosition = defaultPosition;
            }

            bool shootingLaser = Input.GetMouseButton(0);
            if (shootingLaser && animator.GetCurrentAnimatorStateInfo(0).IsName("Static"))
            {
                RobotStatLoadOutCollection laserLoadOut = playerRobot.RobotUpgradeLoadOut.ToolLoadOuts.GetValueOrDefault(RobotToolType.LaserGun);
                float speed = RobotUpgradeUtils.GetContinuousValue(laserLoadOut, (int)LaserGunUpgrade.FireRate);
                float fireRate = LaserGun.GetFireRate(speed);
                animator.speed = 1/fireRate;
                animator.Play(LASER_SHOOT_ANIMATION);
            }
            
            animator.SetBool(Shooting,shootingLaser);
            
        }
    }
}
