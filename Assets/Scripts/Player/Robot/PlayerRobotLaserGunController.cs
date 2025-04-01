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
        private Direction shootDirection;
        private float shootAngle;
        
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

        public Vector3 GetEdgePosition()
        {
            return transform.position;
        }

        public void AngleToPosition(Vector3 target)
        {
            Vector3 direction = (target - playerRobot.transform.position).normalized;
            shootDirection = direction.x < 0 ? Direction.Left : Direction.Right;
            float angle = Mathf.Atan2(direction.y, direction.x);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float sign = shootDirection == Direction.Left ? -1 : 1;
            spriteRenderer.flipY = shootDirection == Direction.Left;
            transform.localPosition =  new Vector3(defaultPosition.x * cos + defaultPosition.y * sin, defaultPosition.x * sin + sign*defaultPosition.y * cos, 1f);
            transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
        }
    }
}
