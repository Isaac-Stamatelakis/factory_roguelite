using Player.Controls;
using Player.Robot;
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
    }

    public class StandardPlayerMovement : BasePlayerMovement
    {
        private readonly PlayerMovementInput playerMovementInput;
        private readonly DirectionalMovementStats movementStats;
        private readonly JumpMovementStats jumpStats;
        private readonly Rigidbody2D rb;
        
        private float inputDir;
        private float moveDirTime;
        private JumpEvent jumpEvent;
        private bool holdingJump;
        private bool holdingDown;
        private int bonusJumps;
        private RocketBoots rocketBoots;
        private SpriteRenderer spriteRenderer;

        public StandardPlayerMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            rb = playerRobot.GetComponent<Rigidbody2D>();
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            movementStats = playerRobot.MovementStats;
            jumpStats = playerRobot.JumpStats;
            
            playerMovementInput = new PlayerMovementInput();
            
            playerMovementInput.PlayerMovement.Move.performed += OnMovePerformed;
            playerMovementInput.PlayerMovement.Move.canceled += OnMoveCancelled;
            
            playerMovementInput.PlayerMovement.Jump.performed += OnJumpPressed;
            playerMovementInput.PlayerMovement.Jump.canceled += OnJumpReleased;
            
            playerMovementInput.PlayerMovement.Down.performed += OnDownPressed;
            playerMovementInput.PlayerMovement.Down.canceled += OnDownReleased;
            
            playerMovementInput.Enable();
        }

        public override void MovementUpdate()
        {
            bool blockInput = playerRobot.BlockMovement;
            Vector2 velocity = rb.velocity;

            bool movedLeft = !playerRobot.CollisionStateActive(CollisionState.OnWallLeft) && !blockInput &&
                             inputDir < 0;
            bool movedRight = !playerRobot.CollisionStateActive(CollisionState.OnWallRight) && !blockInput &&
                              inputDir > 0;
            bool moveUpdate = movedLeft != movedRight; // xor
            
            if (!moveUpdate) ReduceMoveDir();
            if (movedLeft) ApplyMovement(Direction.Left);
            if (movedRight) ApplyMovement(Direction.Right);
            
            UpdateMovementAnimations();
            
            if (blockInput)
            {
                rb.gravityScale = playerRobot.DefaultGravityScale;   
                jumpEvent = null;
                return;
            }
            
            
            UpdateHorizontalMovement(ref velocity);
            UpdateVerticalMovement(ref velocity);
            rb.velocity = velocity;

            void UpdateMovementAnimations()
            {
                var animationController = playerRobot.AnimationController;
                animationController.ToggleBool(PlayerAnimationState.Walk, moveUpdate);
                if (!playerRobot.IsGrounded()) return;
                if (!moveUpdate)
                {
                    animationController.SetAnimationSpeed(1);
                    animationController.PlayAnimation(PlayerAnimation.Idle,playerRobot.IsUsingTool);
                    return;
                }

                const float ANIMATOR_SPEED_INCREASE = 0.25f;
                float animationSpeed = 1+ANIMATOR_SPEED_INCREASE * RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts, (int)RobotUpgrade.Speed);
                animationController.SetAnimationSpeed(animationSpeed);
                const float NO_TIME_CHANGE = -1;
                PlayWalkAnimation(NO_TIME_CHANGE);
            }

            void ReduceMoveDir()
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
            
            void ApplyMovement(Direction direction)
            {
                int dirSign = direction == Direction.Right ? 1 : -1;
                bool isMovingInCurrentDirection = (dirSign > 0 && moveDirTime > 0) || (dirSign < 0 && moveDirTime < 0);
                
                float modifier = isMovingInCurrentDirection 
                    ? movementStats.moveModifier 
                    : movementStats.turnRate;
    
        
                moveDirTime += dirSign * modifier * Time.deltaTime;
                spriteRenderer.flipX = direction == Direction.Left;
                
                float minMove = movementStats.minMove * dirSign;
                if ((dirSign > 0 && moveDirTime > 0 && moveDirTime < movementStats.minMove) ||
                    (dirSign < 0 && moveDirTime < 0 && moveDirTime > -movementStats.minMove))
                {
                    moveDirTime = minMove;
                }
                
                if (playerRobot.WalkingIntoSlope(direction))
                {
                    playerRobot.ResetLiveYFrames();
                }
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            inputDir = context.ReadValue<float>();
        }

        private void OnMoveCancelled(InputAction.CallbackContext context)
        {
            inputDir = 0;
        }
        
        
        public bool CanJump()
        {
            return playerRobot.IsGrounded() || (bonusJumps > 0 && playerRobot.TryConsumeEnergy(SelfRobotUpgradeInfo.BONUS_JUMP_COST, 0));
        }


        private void PlayWalkAnimation(float time)
        {
            PlayerAnimationController animationController = playerRobot.AnimationController;
            RobotArmController armController = playerRobot.gunController;
            if (time > 0)
            {
                animationController.PlayAnimation(PlayerAnimation.Walk, playerRobot.IsUsingTool,time);
            }
            else
            {
                animationController.PlayAnimation(PlayerAnimation.Walk, playerRobot.IsUsingTool);
            }

            bool walkingBackwards = playerRobot.IsUsingTool && (
                (armController.ShootDirection == Direction.Left && moveDirTime > 0) ||
                (armController.ShootDirection == Direction.Right && moveDirTime < 0));
            animationController.SetAnimationDirection(walkingBackwards ? -1 : 1);
        }


        private void UpdateHorizontalMovement(ref Vector2 velocity)
        {
            const float MAX_MOVE_DIR = 1;

            if (moveDirTime > MAX_MOVE_DIR) moveDirTime = MAX_MOVE_DIR;
            if (moveDirTime < -MAX_MOVE_DIR) moveDirTime = -MAX_MOVE_DIR;

            int sign = moveDirTime < 0 ? -1 : 1;
            float wishdir = movementStats.accelationModifier * moveDirTime * sign;

            float speed = movementStats.speed;
            float speedUpgrades = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts,
                (int)RobotUpgrade.Speed);
            
            if (wishdir > 0.05f && playerRobot.TryConsumeEnergy(speedUpgrades * SelfRobotUpgradeInfo.SPEED_INCREASE_COST_PER_SECOND * Time.deltaTime, 0.1f))
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
        }

        private void UpdateVerticalMovement(ref Vector2 velocity)
        {
            var fluidCollisionInformation = playerRobot.fluidCollisionInformation;
            float fluidGravityModifer = fluidCollisionInformation.Colliding ? fluidCollisionInformation.GravityModifier : 1f;
            
            if (jumpEvent != null)
            {
                if (playerRobot.CollisionStateActive(CollisionState.HeadContact))
                {
                    rb.gravityScale = fluidGravityModifer * playerRobot.DefaultGravityScale;
                    jumpEvent = null;
                }
                else if (holdingJump)
                {
                    rb.gravityScale = fluidGravityModifer * jumpEvent.GetGravityModifier(jumpStats.initialGravityPercent, jumpStats.maxGravityTime) * playerRobot.DefaultGravityScale;
                    jumpEvent.IterateTime();
                    if (holdingDown)
                    {
                        jumpEvent.IterateTime();
                        rb.gravityScale *= 1.5f;
                    }
                }
                else
                {
                    rb.gravityScale = fluidGravityModifer * playerRobot.DefaultGravityScale;
                }
                return;
            }

            float fluidSpeedModifier = fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;

           
            if (rocketBoots != null && holdingJump)
            {
                RocketBootUpdate(ref velocity,fluidSpeedModifier);
                if (rocketBoots != null) return;
            }
      
            rb.gravityScale = playerRobot.DefaultGravityScale * fluidGravityModifer;
            const float BONUS_FALL_MODIFIER = 1.25f;
            if (holdingDown) rb.gravityScale *= BONUS_FALL_MODIFIER;
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

        public void RocketBootUpdate(ref Vector2 velocity, float fluidSpeedModifier)
        {
            if (holdingJump)
            {
                if (rocketBoots.Boost > 0)
                {
                    rb.gravityScale = 0;
                    float bonusJumpHeight = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts, (int)RobotUpgrade.JumpHeight);
                    velocity.y = fluidSpeedModifier * rocketBoots.Boost * (1 + 0.33f * bonusJumpHeight);
                }
                else
                {
                    if (rb.velocity.y < 0)
                    {
                        var vector2 = rb.velocity;
                        vector2.y = 0;
                        rb.velocity = vector2;
                    }
                    rocketBoots.Boost = rb.velocity.y/2f;
                }
            }
            rocketBoots.UpdateBoost(holdingJump);
            if (rocketBoots.FlightTime >= 0) return;
            rocketBoots.Terminate();
            rocketBoots = null;
        }

        void OnJumpPressed(InputAction.CallbackContext context)
        {
            holdingJump = true;
            if (holdingDown)
            {
                bool onPlatform = playerRobot.CollisionStateActive(CollisionState.OnPlatform);
                bool onSlopePlatform = playerRobot.IsOnSlopedPlatform();
                if (onPlatform || onSlopePlatform)
                {
                    if (onPlatform) playerRobot.ResetIgnorePlatformFrames();
                    if (onSlopePlatform)
                    {
                        playerRobot.SetLiveY(12);
                        playerRobot.ResetIgnoreSlopePlatformFrames();
                    }
                    return;
                }
            }
            if (playerRobot.IgnorePlatformFrames > 0 || (!CanJump() && playerRobot.CoyoteFrames <= 0)) return;

            if (bonusJumps > 0 && playerRobot.CoyoteFrames <= 0)
            {
                playerRobot.PlayerParticles.PlayParticle(PlayerParticle.BonusJump);
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
        
        void OnDownPressed(InputAction.CallbackContext context)
        {
            holdingDown = true;
        }

        void OnDownReleased(InputAction.CallbackContext context)
        {
            holdingDown = false;
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
