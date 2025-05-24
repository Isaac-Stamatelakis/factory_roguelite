using System;
using UnityEngine;

namespace Player
{
    public enum PlayerAnimation
    {
        Walk,
        Air,
        Idle
    }

    public enum PlayerAnimationState
    {
        Walk,
        Action
    }
    public class PlayerAnimationController
    {
        
        public PlayerAnimationController(PlayerRobot playerRobot, Animator animator)
        {
            this.playerRobot = playerRobot;
            this.animator = animator;
        }
        private readonly PlayerRobot playerRobot;
        private readonly Animator animator;
        private static readonly int Walk = Animator.StringToHash("IsWalking");
        private static readonly int Air = Animator.StringToHash("InAir");
        private static readonly int Action = Animator.StringToHash("Action");
        private static readonly int AnimationDirection = Animator.StringToHash("Direction");

        public void PlayAnimation(PlayerAnimation playerAnimation, bool usingTool)
        {
            string animationName = GetAnimationName(playerAnimation, usingTool);
            animator.Play(animationName);
        }
        
        public void PlayAnimation(PlayerAnimation playerAnimation, bool usingTool, float time)
        {
            string animationName = GetAnimationName(playerAnimation, usingTool);
            animator.Play(animationName,0,time);
        }

        private string GetAnimationName(PlayerAnimation playerAnimation, bool usingTool)
        {
            return playerAnimation.ToString() + (usingTool ? "Action" : string.Empty);
        }
        

        public void ToggleBool(PlayerAnimationState playerAnimationState, bool state)
        {
            int stateHash = playerAnimationState switch
            {
                PlayerAnimationState.Walk => Walk,
                PlayerAnimationState.Action => Action,
                _ => throw new ArgumentOutOfRangeException(nameof(playerAnimationState), playerAnimationState, null)
            };
            animator.SetBool(stateHash, state);
        }

        public void SetAnimationDirection(float dir)
        {
            animator.SetFloat(AnimationDirection,dir);
        }

        public void SetAnimationSpeed(float speed)
        {
            animator.speed = speed;
        }

        
    }
}
