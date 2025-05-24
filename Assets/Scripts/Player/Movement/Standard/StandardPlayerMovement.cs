using Player.Controls;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.Instances.RocketBoots;
using Robot.Upgrades.LoadOut;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement.Standard
{
    public abstract class BasePlayerMovement
    {
        protected PlayerRobot playerRobot;

        protected BasePlayerMovement(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
        }

        public abstract void MovementUpdate();
        public abstract void InitializeListeners();
    }

    public class StandardPlayerMovement : BasePlayerMovement
    {
        private PlayerMovementInput playerMovementInput;
        private DirectionalMovementStats movementStats;
        private JumpMovementStats jumpStats;
        private float inputDir;
        private float moveDirTime;
        private JumpEvent jumpEvent;
        private Rigidbody2D rb;
        private bool holdingJump;
        private bool holdingDown;
        private int bonusJumps;
        private RocketBoots rocketBoots;

        public StandardPlayerMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            rb = playerRobot.GetComponentInChildren<Rigidbody2D>();
            movementStats = playerRobot.MovementStats;
            jumpStats = playerRobot.JumpStats;
        }

        public override void MovementUpdate()
        {

        }

        public override void InitializeListeners()
        {
            playerMovementInput.PlayerMovement.Move.performed += OnMovePerformed;
            playerMovementInput.PlayerMovement.Move.canceled += OnMoveCancelled;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            inputDir = context.ReadValue<float>();
        }

        private void OnMoveCancelled(InputAction.CallbackContext context)
        {
            inputDir = 0;
        }

        private void StandardMoveUpdate()
        {
            bool blockInput = playerRobot.BlockMovement;
            Vector2 velocity = rb.velocity;

            bool movedLeft = !playerRobot.CollisionStateActive(CollisionState.OnWallLeft) && !blockInput &&
                             inputDir > 0;
            bool movedRight = !playerRobot.CollisionStateActive(CollisionState.OnWallRight) && !blockInput &&
                              inputDir < 0;
            bool moveUpdate = movedLeft != movedRight; // xor
            if (!moveUpdate)
            {

            }

            UpdateMovementAnimations();

            const float MAX_MOVE_DIR = 1;

            if (moveDirTime > MAX_MOVE_DIR) moveDirTime = MAX_MOVE_DIR;
            if (moveDirTime < -MAX_MOVE_DIR) moveDirTime = -MAX_MOVE_DIR;

            int sign = moveDirTime < 0 ? -1 : 1;
            float wishdir = movementStats.accelationModifier * moveDirTime * sign;

            float speed = movementStats.speed;
            float speedUpgrades = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts,
                (int)RobotUpgrade.Speed);
            ;
            if (wishdir > 0.05f &&
                playerRobot.TryConsumeEnergy(
                    speedUpgrades * SelfRobotUpgradeInfo.SPEED_INCREASE_COST_PER_SECOND * Time.deltaTime, 0.1f))
            {
                speed += speedUpgrades;
            }

            var fluidInformation = playerRobot.fluidCollisionInformation;
            if (fluidInformation.Colliding) speed *= fluidInformation.SpeedModifier;
            switch (playerRobot.CurrentTileMovementType)
            {
                case TileMovementType.None:
                    break;
                case TileMovementType.Slippery:
                    speed *= 1.2f;
                    break;
                case TileMovementType.Slow:
                    speed *= movementStats.slowSpeedReduction;
                    break;
            }

            if (!playerRobot.IsOnGround()) speed *= movementStats.airSpeedIncrease;
            velocity.x = sign * Mathf.Lerp(0, speed, wishdir);

            SpaceBarMovementUpdate(ref velocity);
            UpdateVerticalMovement(ref velocity);
            rb.velocity = velocity;

            void UpdateMovementAnimations()
            {
                animator.SetBool(Walk, moveUpdate);
                if (!IsGrounded()) return;
                if (!moveUpdate)
                {
                    animator.speed = 1;
                    animator.Play(isUsingTool ? "IdleAction" : "Idle");
                    return;
                }

                const float ANIMATOR_SPEED_INCREASE = 0.25f;
                animator.speed = 1 + ANIMATOR_SPEED_INCREASE *
                    RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);
                const float NO_TIME_CHANGE = -1;
                PlayWalkAnimation(NO_TIME_CHANGE);
            }

            void OnNoMove()
            {
                float dif = playerRobot.GetFriction();

                if (moveDirTime > 0)
                {
                    moveDirTime -= dif;
                    if (moveDirTime < 0) moveDirTime = 0;
                }

                if (moveDirTime < 0)
                {
                    moveDirTime += dif;
                    if (moveDirTime > 0) moveDirTime = 0;
                }
            }
        }
        
        public bool CanJump()
        {
            return playerRobot.IsGrounded() || (bonusJumps > 0 && playerRobot.TryConsumeEnergy(SelfRobotUpgradeInfo.BONUS_JUMP_COST, 0));
        }


        private void PlayWalkAnimation(float time)
        {
            if (time > 0)
            {
                animator.Play(isUsingTool ? "WalkAction" : "Walk", 0, time);
            }
            else
            {
                animator.Play(isUsingTool ? "WalkAction" : "Walk");
            }

            bool walkingBackwards = isUsingTool && (
                (gunController.ShootDirection == Direction.Left && moveDirTime > 0) ||
                (gunController.ShootDirection == Direction.Right && moveDirTime < 0));
            animator.SetFloat(AnimationDirection, walkingBackwards ? -1 : 1);
        }


        private void UpdateVerticalMovement(ref Vector2 velocity)
        {
            var fluidCollisionInformation = playerRobot.fluidCollisionInformation;
            float fluidGravityModifer =
                fluidCollisionInformation.Colliding ? fluidCollisionInformation.GravityModifier : 1f;

            if (jumpEvent != null)
            {
                if (playerRobot.CollisionStateActive(CollisionState.HeadContact))
                {
                    rb.gravityScale = fluidGravityModifer * playerRobot.DefaultGravityScale;
                    jumpEvent = null;
                }
                else if (holdingJump)
                {
                    rb.gravityScale = fluidGravityModifer *
                                      jumpEvent.GetGravityModifier(jumpStats.initialGravityPercent,
                                          jumpStats.maxGravityTime) * playerRobot.DefaultGravityScale;
                    jumpEvent.IterateTime();
                    if (ControlUtils.GetControlKey(PlayerControl.MoveDown))
                    {
                        jumpEvent.IterateTime();
                        rb.gravityScale *= 1.5f;
                    }
                }
                else
                {
                    rb.gravityScale = fluidGravityModifer * playerRobot.DefaultGravityScale;
                }

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    jumpEvent = null;
                }

                return;
            }

            float fluidSpeedModifier =
                fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;

            bool blockInput = playerRobot.BlockMovement;
            if (rocketBoots != null && rocketBoots.Active)
            {
                if (blockInput)
                {
                    rocketBoots.Terminate();
                    rocketBoots = null;
                    return;
                }

                if (rocketBoots.Boost > 0)
                {
                    rb.gravityScale = 0;
                    float bonusJumpHeight =
                        RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,
                            (int)RobotUpgrade.JumpHeight);
                    velocity.y = fluidSpeedModifier * rocketBoots.Boost * (1 + 0.33f * bonusJumpHeight);
                }
                else
                {
                    rb.gravityScale = fluidGravityModifer * playerRobot.DefaultGravityScale;
                }
            }

            if (!blockInput)
            {
                const float BONUS_FALL_MODIFIER = 1.25f;
                rb.gravityScale = ControlUtils.GetControlKey(PlayerControl.MoveDown)
                    ? playerRobot.DefaultGravityScale * BONUS_FALL_MODIFIER
                    : playerRobot.DefaultGravityScale;
            }
            rb.gravityScale *= fluidGravityModifer;
        }

        public void OnGrounded()
        {
            if (bonusJumps <= 0)
            {
                bonusJumps = playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts?.GetCurrent()?.GetDiscreteValue((int)RobotUpgrade.BonusJump) ?? 0;
            }

            int rocketBootUpgrades = RobotUpgradeUtils.GetDiscreteValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.RocketBoots);
            if (rocketBootUpgrades > 0)
            {
                rocketBoots?.Terminate();
                rocketBoots ??= new RocketBoots();
                rocketBoots.FlightTime = 1+rocketBootUpgrades;
            }
            else
            {
                rocketBoots = null;
            }
        }

        void OnJumpPressed(InputAction.CallbackContext context)
        {
            holdingJump = true;
            if (playerRobot.IgnorePlatformFrames > 0 || (!CanJump() && playerRobot.CoyoteFrames <= 0)) return;

            if (bonusJumps > 0 && playerRobot.CoyoteFrames <= 0)
            {
                bonusJumpParticles.Play();
                bonusJumps--;
            }

            var fluidCollisionInformation = playerRobot.fluidCollisionInformation;
            float fluidModifier = fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;
            float bonusJumpHeight = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.JumpHeight);

            var vector2 = rb.velocity;
            vector2.y = fluidModifier*(jumpStats.jumpVelocity+bonusJumpHeight);
            rb.velocity = vector2;
            
            playerRobot.OnJump();
            jumpEvent = new JumpEvent();
        }

        void OnJumpReleased(InputAction.CallbackContext context)
        {
            holdingJump = false;
        }

        void OnDropDownPlatformPressed(InputAction.CallbackContext context)
        {
            bool onPlatform = playerRobot.CollisionStateActive(CollisionState.OnPlatform);
            bool onSlopePlatform = playerRobot.IsOnSlopedPlatform();
            if (!onPlatform && !onSlopePlatform) return;

            if (onPlatform) playerRobot.ResetIgnorePlatformFrames();
            if (onSlopePlatform)
            {
                playerRobot.ResetLiveYFrames();
                playerRobot.ResetIgnoreSlopePlatformFrames();
            }
        }

        void OnDownPressed(InputAction.CallbackContext context)
        {
            holdingDown = true;
        }

        void OnDownReleased(InputAction.CallbackContext context)
        {
            holdingDown = false;
        }


        private void SpaceBarMovementUpdate(ref Vector2 velocity)
        {
            if (playerRobot.BlockMovement)
            {
                rb.gravityScale = playerRobot.DefaultGravityScale;   
                jumpEvent = null;
                return;
            }
            
            if (!IsOnGround() && rocketBoots != null)
            {
                if (!rocketBoots.Active && ControlUtils.GetControlKeyDown(PlayerControl.Jump))
                {
                    
                    StartCoroutine(rocketBoots.Activate(RobotUpgradeAssets.RocketBootParticles, transform));
                }

                if (rocketBoots.Active && TryConsumeEnergy(SelfRobotUpgradeInfo.ROCKET_BOOTS_COST_PER_SECOND * Time.deltaTime, 0))
                {
                    if (rocketBoots.Boost <= 0)
                    {
                        if (rb.velocity.y < 0)
                        {
                            var vector2 = rb.velocity;
                            vector2.y = 0;
                            rb.velocity = vector2;
                        }
                        rocketBoots.Boost = rb.velocity.y/2f;
                    }
                    rocketBoots.UpdateBoost(ControlUtils.GetControlKey(PlayerControl.Jump));
                    
                    if (rocketBoots.FlightTime < 0)
                    {
                        rocketBoots.Terminate();
                        rocketBoots = null;
                        rb.gravityScale =  fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f * defaultGravityScale;
                    }
                }
            }
        }
        private class JumpEvent
        {
            private float holdTime;
            public void IterateTime()
            {
                holdTime += Time.deltaTime;
            }
            
            public float GetGravityModifier(float initialGravityPercent, float maxGravityTime)
            {
                return Mathf.Lerp(initialGravityPercent,1,1/maxGravityTime*holdTime);
            }
        }

    }
}
