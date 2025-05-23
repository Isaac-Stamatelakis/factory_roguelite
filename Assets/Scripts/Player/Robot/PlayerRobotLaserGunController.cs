using System;
using System.Collections.Generic;
using Player.Tool;
using Robot.Tool;
using Robot.Tool.Instances;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using UnityEngine;

namespace Player.Robot
{
    public enum RobotArmState
    {
        LaserDrill,
        Buildinator,
        LaserGun,
        LaserExplosion,
        ConduitCutter,
            
    }
    public class PlayerRobotLaserGunController : MonoBehaviour
    {
        
        private static readonly int Shooting = Animator.StringToHash("Shooting");
        private static readonly int ActiveParameter = Animator.StringToHash("Active");
        private static readonly int ToolColorMatParameter = Shader.PropertyToID("_ToolColor");
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector3 defaultPosition;
        private PlayerRobot playerRobot;
        private Direction shootDirection;
        public Direction ShootDirection => shootDirection;
        private float shootAngle;
        private RobotArmState? currentState;

        public void Initialize(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            defaultPosition = transform.localPosition;
            gameObject.SetActive(false);
         
        }

        public void PlayAnimationState(RobotArmState state)
        {
            animator.SetBool(ActiveParameter,true);
            if (state == currentState) return;
            currentState = state;
            animator.Play(GetStateAnimation(state),0,0);
            
            animator.speed = 1f;
        }

        public void SetToolColor(Color color)
        {
            spriteRenderer.material.SetColor(ToolColorMatParameter,color);
        }
 
        private string GetStateAnimation(RobotArmState state)
        {
            switch (state)
            {
                case RobotArmState.LaserGun:
                    return "Gun";
                case RobotArmState.LaserExplosion:
                    return "Blast";
                case RobotArmState.ConduitCutter:
                    return "Conduit";
                case RobotArmState.LaserDrill:
                    return "Drill";
                case RobotArmState.Buildinator:
                    return "Drill";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private float GetAnimationSpeed(RobotArmState state)
        {
            switch (state)
            {
                case RobotArmState.LaserGun:
                case RobotArmState.LaserExplosion:
                    RobotStatLoadOutCollection laserLoadOut = playerRobot.RobotUpgradeLoadOut.ToolLoadOuts.GetValueOrDefault(RobotToolType.LaserGun);
                    float speed = RobotUpgradeUtils.GetContinuousValue(laserLoadOut, (int)LaserGunUpgrade.FireRate);
                    float fireRate = LaserGun.GetFireRate(speed);
                    if (state == RobotArmState.LaserExplosion) fireRate *= 4;
                    return fireRate;
                case RobotArmState.ConduitCutter:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        
        public void OnNoClick()
        {
            if (!currentState.HasValue) return;
            currentState = null;
            animator.SetBool(ActiveParameter,false);
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
