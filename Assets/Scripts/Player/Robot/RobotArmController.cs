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
    
    public class RobotArmController : MonoBehaviour
    {
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
        private Vector2 radialPosition;
        
        public Material defaultMaterial;
        public Material rainbowMaterial;

        public void Initialize(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            defaultPosition = transform.localPosition;
        }

        public void PlayAnimationState(RobotArmState state, int subState)
        {
            animator.SetBool(ActiveParameter,true);
            currentState = state;
            animator.Play(GetStateAnimation(state),0,0);
            animator.speed = GetAnimationSpeed(state);
            spriteRenderer.material = GetStateMaterial(state,subState);
        }

        public void SyncAnimation(RobotArmState robotArmState, float time)
        {
            currentState = robotArmState;
            float speed = GetAnimationSpeed(robotArmState);
            animator.speed = speed;
            animator.Play(GetStateAnimation(robotArmState),0,time*speed);
        }

        
        public void SetToolColor(Color color)
        {
            spriteRenderer.material.SetColor(ToolColorMatParameter,color);
        }
 
        private string GetStateAnimation(RobotArmState state)
        {
            return state switch
            {
                RobotArmState.LaserGun => "Gun",
                RobotArmState.LaserExplosion => "Blast",
                RobotArmState.ConduitCutter => "Conduit",
                RobotArmState.LaserDrill => "Drill",
                RobotArmState.Buildinator => "Drill",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private Material GetStateMaterial(RobotArmState state, int subState)
        {
            switch (state)
            {
                case RobotArmState.LaserDrill:
                case RobotArmState.Buildinator:
                case RobotArmState.LaserGun:
                case RobotArmState.LaserExplosion:
                    return defaultMaterial;
                case RobotArmState.ConduitCutter:
                    return subState == (int)ConduitCutterMode.All ? rainbowMaterial : defaultMaterial;
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
                    float upgrades = RobotUpgradeUtils.GetContinuousValue(laserLoadOut, (int)LaserGunUpgrade.FireRate);
                    LaserGunMode mode = state == RobotArmState.LaserGun ? LaserGunMode.Light : LaserGunMode.Blast;
                    return LaserGun.GetAnimationSpeed(upgrades,mode);
                case RobotArmState.ConduitCutter:
                case RobotArmState.LaserDrill:
                case RobotArmState.Buildinator:
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

        public Vector3 GetEdgePosition(float radius)
        {
            return (Vector2)transform.position + radialPosition* spriteRenderer.bounds.extents * radius;
        }

        public Vector2 GetLocalEdgePosition()
        {
            return (Vector2)transform.localPosition + radialPosition* spriteRenderer.bounds.extents;
        }

        public void AngleToPosition(Vector3 target)
        {
            Vector3 direction = (target - playerRobot.transform.position).normalized;
            shootDirection = direction.x < 0 ? Direction.Left : Direction.Right;
            float angle = Mathf.Atan2(direction.y, direction.x);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            radialPosition = new Vector2(cos, sin);
            float sign = shootDirection == Direction.Left ? -1 : 1;
            spriteRenderer.flipY = shootDirection == Direction.Left;
            transform.localPosition =  new Vector3(defaultPosition.x * cos + defaultPosition.y * sin, defaultPosition.x * sin + sign*defaultPosition.y * cos, 1f);
            transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
        }
    }
}
