using System;
using Chunks;
using Dimensions;
using Player.Controls;
using Player.Robot;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.Instances.RocketBoots;
using Robot.Upgrades.LoadOut;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using Tiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement.Standard
{
    public abstract class BasePlayerMovement
    {
        protected PlayerRobot playerRobot;

        internal BasePlayerMovement(PlayerRobot playerRobot)
        {
            this.playerRobot = playerRobot;
        }

        public abstract void MovementUpdate();
        public abstract void FixedMovementUpdate();
        public abstract void Disable();
        protected abstract InputActionMap GetInputActionMap();

        public void SetMovementStatus(bool active)
        {
            var inputMap = GetInputActionMap();
            if (active)
            {
                inputMap.Enable();
            }
            else
            {
                inputMap.Disable();
            }
        }
    }

    public interface IMovementGroundedListener
    {
        public void OnGrounded();
    }

    public interface IOnWallCollisionMovementListener
    {
        public void OnWallCollision();
    }
    
    public interface IOnFluidCollisionMovementListener
    {
        public void OnFluidCollision();
    }
    
    public interface IOnSlopeCollisionMovementListener
    {
        public void OnSlopeCollision(Direction slopeDirection);
    }
    
    public interface IOnSlopeExitMovementListener
    {
        public void OnSlopeExit();
    }

    public interface IOnSlopeStayMovementListener
    {
        public void OnSlopeStay(Direction slopeDirection);
    }
    

    public interface IOnTeleportMovementListener
    {
        public void OnTeleport();
    }
    
    public class StandardPlayerMovement : BasePlayerMovement, IMovementGroundedListener, IOnWallCollisionMovementListener, IOnFluidCollisionMovementListener
    , IOnSlopeExitMovementListener, IOnTeleportMovementListener, IOnSlopeCollisionMovementListener, IOnSlopeStayMovementListener
    {
        private readonly DirectionalMovementStats movementStats;
        private readonly JumpMovementStats jumpStats;
        private readonly Rigidbody2D rb;
        
        private float inputDir;
        private float moveDirTime;
        private JumpEvent jumpEvent;
        private bool holdingJump;
        public bool HoldingDown =>  holdingDown;
        private bool holdingDown;
        private int bonusJumps;
        private RocketBoots rocketBoots;
        private SpriteRenderer spriteRenderer;
        private InputActions inputActions;
        private PlayerScript playerScript;

        private float fallTime;
        private int coyoteFrames;
        private bool freezeY;
        private int slipperyFrames;
        private bool immuneToNextFall = false;
        private int slopeFrames = 0;
        private Direction? slopeState;

        private bool walkingDownSlope;
        private TileMovementType currentTileMovementType;
        private int baseCollidableLayer;
        private PlayerSlopeStateTrigger slopeStateTrigger;
        public StandardPlayerMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            rb = playerRobot.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            playerScript = playerRobot.GetComponent<PlayerScript>();
            playerRobot.playerColliders.SetStateStandard();
            slopeStateTrigger = playerRobot.playerColliders.SlopeTrigger.GetComponent<PlayerSlopeStateTrigger>();
            
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            movementStats = playerRobot.MovementStats;
            jumpStats = playerRobot.JumpStats;
            baseCollidableLayer = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform"));
            ToggleRocketBoots(RobotUpgradeUtils.GetDiscreteValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,(int)RobotUpgrade.RocketBoots) > 0);

            inputActions = playerRobot.GetComponent<PlayerScript>().InputActions;
            var playerMovementInput = inputActions.StandardMovement;
            
            playerMovementInput.Jump.performed += OnJumpPressed;
            playerMovementInput.Jump.canceled += OnJumpReleased;
            playerMovementInput.Enable();
            
            var constantMovementActions = inputActions.ConstantMovement;
            constantMovementActions.MoveHorizontal.performed += OnMovePerformed;
            constantMovementActions.MoveHorizontal.canceled += OnMoveCancelled;
            
            constantMovementActions.Down.performed += OnDownPressed;
            constantMovementActions.Down.canceled += OnDownReleased;
            
            holdingDown = constantMovementActions.Down.IsPressed();
            inputDir = constantMovementActions.MoveHorizontal.ReadValue<float>();
        }
        
        public override void MovementUpdate()
        {
            if (playerRobot.IsGrounded())
            {
                coyoteFrames = jumpStats.coyoteFrames;
            }
            
            Vector2 velocity = rb.velocity;

            bool movedLeft = !playerRobot.CollisionStateActive(CollisionState.OnWallLeft) &&
                             inputDir < 0;
            bool movedRight = !playerRobot.CollisionStateActive(CollisionState.OnWallRight) &&
                              inputDir > 0;
            bool moveUpdate = movedLeft != movedRight; // xor
            
            if (!moveUpdate) ReduceMoveDir();
            if (movedLeft) ApplyMovement(Direction.Left);
            if (movedRight) ApplyMovement(Direction.Right);
            
            UpdateMovementAnimations();
            UpdateHorizontalMovement(ref velocity);
            UpdateVerticalMovement(ref velocity);
            rb.velocity = velocity;
            slopeStateTrigger.UpdateSize(velocity.x,jumpEvent!=null);
            

            void UpdateMovementAnimations()
            {
                var animationController = playerRobot.AnimationController;
                animationController.ToggleBool(PlayerAnimationState.Walk, moveUpdate);
                if (!playerRobot.IsGrounded()) return;
                if (!moveUpdate)
                {
                    animationController.SetAnimationSpeed(1);
                    animationController.PlayAnimation(PlayerAnimation.Idle);
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
                float dif = GetFriction();

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

        public override void FixedMovementUpdate()
        {
            coyoteFrames--;
            slipperyFrames--;
            slopeFrames--;

            if (playerScript.InputActions.ConstantMovement.TryClimb.IsPressed()) // This has to be seperated from standard input movement so holding up/down and going through platforms doesn't cancel when switching between states
            {
                if (TryStartClimbing()) return;
            }

            if (slopeFrames < 0 && slopeState.HasValue)
            {
                slopeState = null;
            }
            playerRobot.PlatformCollider.enabled = playerRobot.IgnorePlatformFrames < 0 && rb.velocity.y < 0.01f;
            bool ignoreSlopedPlatforms = HoldingDown && (playerRobot.CollisionStateActive(CollisionState.OnGround) || playerRobot.CollisionStateActive(CollisionState.OnPlatform));
            if (ignoreSlopedPlatforms)
            {
                playerRobot.ResetIgnoreSlopePlatformFrames();
            }
            playerRobot.TogglePlatformCollider();
            playerRobot.ToggleSlopePlatformCollider(PlayerRobot.SlopePlatformCollisionState.Left, ignoreSlopedPlatforms);
            playerRobot.ToggleSlopePlatformCollider(PlayerRobot.SlopePlatformCollisionState.Right, ignoreSlopedPlatforms);
         
            bool grounded = playerRobot.IsGrounded();
            playerRobot.AnimationController.ToggleBool(PlayerAnimationState.Air,coyoteFrames < 0 && !grounded);
            if (!grounded && coyoteFrames < 0)
            {
                playerRobot.AnimationController.PlayAnimation(PlayerAnimation.Air);
            }

            if (!playerRobot.InFluid() && playerRobot.IsOnGround())
            {
                currentTileMovementType = GetTileMovementModifier();
                slipperyFrames = currentTileMovementType == TileMovementType.Slippery ? movementStats.iceNoAirFrictionFrames : 0;
            }
                
            CalculateFallTime();
            ClampFallSpeed();
            rb.constraints = GetFreezeConstraints();
        }
        
        private RigidbodyConstraints2D GetFreezeConstraints()
        {
            const RigidbodyConstraints2D FREEZE_Y =
                RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            const RigidbodyConstraints2D FREEZE_Z = RigidbodyConstraints2D.FreezeRotation;
            ;
            const float epilson = 0.1f;
            if (freezeY) return FREEZE_Y;
            if (currentTileMovementType == TileMovementType.Slippery && playerRobot.CollisionStateActive(CollisionState.OnSlope)) return RigidbodyConstraints2D.FreezeRotation;

            if (playerRobot.LiveYUpdates > 0) return FREEZE_Z;
            
            if (playerRobot.CollisionStateActive(CollisionState.OnSlope) || playerRobot.IsOnSlopedPlatform())
            {
                bool moving = rb.velocity.x != 0;
                bool touchingWall = playerRobot.CollisionStateActive(CollisionState.OnWallLeft) || playerRobot.CollisionStateActive(CollisionState.OnWallRight);
                return moving && !touchingWall ? FREEZE_Z : FREEZE_Y;
            }
            if (playerRobot.CollisionStateActive(CollisionState.OnWallLeft) || playerRobot.CollisionStateActive(CollisionState.OnWallRight)) return FREEZE_Z;
            return playerRobot.IsOnGround() && rb.velocity.y < epilson
                ? FREEZE_Y
                : FREEZE_Z;
        }
        
        private void ClampFallSpeed()
        {
            const float TERMINAL_VELOCITY = 20f;
            if (!(rb.velocity.y < -TERMINAL_VELOCITY)) return;
            
            var vector2 = rb.velocity;
            vector2.y = -TERMINAL_VELOCITY;
            rb.velocity = vector2;
        }


        private bool TryStartClimbing()
        {
            if (playerRobot.BlockClimbingFrames >= 0) return false;
            if (playerRobot.GetClimbable((Vector2)playerRobot.transform.position) == null) return false;
            //if (playerRobot.GetClimbable((Vector2)playerRobot.transform.position + Vector2.up * Global.TILE_SIZE / 2f) == null) return false;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0;
            Vector3 position = playerRobot.transform.position;
            float x = position.x;
            x = Mathf.Floor(x*2)/2f+0.25f;
            position.x = x;
            playerRobot.transform.position = position;
            playerRobot.SetMovementState(PlayerMovementState.Climb);
            return true;
        }
        
        private TileMovementType GetTileMovementModifier()
        {
            Vector2 extent = spriteRenderer.sprite.bounds.extents;
            float y = playerRobot.transform.position.y - spriteRenderer.sprite.bounds.extents.y - Global.TILE_SIZE/4f;
            float xDif = extent.x-0.1f;
            Vector2 bottomLeft = new Vector2(playerRobot.transform.position.x-xDif, y);
            Vector2 bottomRight = new Vector2(playerRobot.transform.position.x+xDif, y);
            TileMovementType left = GetMovementTypeAtWorldPosition(bottomLeft);
            TileMovementType right = GetMovementTypeAtWorldPosition(bottomRight);
            
            // Enums are ordered by priority
            return left > right ? left : right; 
        }

        private TileItem GetTileItemBelow(Vector2 position)
        {
            // This can be made more efficent without a get component
            Vector2 tileCenter = TileHelper.getRealTileCenter(position);
            RaycastHit2D objHit = Physics2D.BoxCast(tileCenter,new Vector2(Global.TILE_SIZE-0.02f,Global.TILE_SIZE-0.02f),0,Vector2.zero,Mathf.Infinity,baseCollidableLayer);
            if (ReferenceEquals(objHit.collider, null)) return null;
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(tileCenter);
            ILoadedChunkSystem system = playerScript.CurrentSystem;
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            
            return partition?.GetTileItem(positionInPartition, TileMapLayer.Base);
        }
        private TileMovementType GetMovementTypeAtWorldPosition(Vector2 position)
        {
            TileItem tileItem = GetTileItemBelow(position);
            return tileItem?.tileOptions.movementModifier ?? TileMovementType.None;
        }
        
        private void CalculateFallTime()
        {
            if (!playerRobot.IsOnGround() && !playerRobot.IsOnSlopedPlatform() && !playerRobot.InFluid())
            {
                if (rb.velocity.y < 0) fallTime += Time.fixedDeltaTime;
                return;
            }
            
            if (immuneToNextFall)
            {
                immuneToNextFall = false;
                fallTime = 0;
                return;
            }
            
            if (fallTime <= 0) return;
            
            
            
            const float DAMAGE_RATE = 2;
            const float MIN_DAMAGE = 1f;

            float damage = DAMAGE_RATE * fallTime * fallTime;
    
            fallTime = 0f;
            if (damage < MIN_DAMAGE) return;
            
            playerRobot.PlayerDamage.Damage(damage);
        }
        public override void Disable()
        {
            var playerMovementInput = inputActions.StandardMovement;
            playerMovementInput.Jump.performed -= OnJumpPressed;
            playerMovementInput.Jump.canceled -= OnJumpReleased;

            inputActions.ConstantMovement.MoveHorizontal.performed -= OnMovePerformed;
            inputActions.ConstantMovement.MoveHorizontal.canceled -= OnMoveCancelled;
            
            inputActions.ConstantMovement.Down.performed -= OnDownPressed;
            inputActions.ConstantMovement.Down.canceled -= OnDownReleased;
            playerMovementInput.Disable();
        }

        public void OnPlayerPaused()
        {
            immuneToNextFall = true;
            freezeY = true;
        }

        public void OnPlayerUnpaused()
        {
            freezeY = false;
        }

        protected override InputActionMap GetInputActionMap()
        {
            return inputActions.StandardMovement;
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
                animationController.PlayAnimation(PlayerAnimation.Walk);
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
            switch (currentTileMovementType)
            {
                case TileMovementType.None:
                    break;
                case TileMovementType.Slippery:
                    speed *= 1.2f;
                    break;
                case TileMovementType.Slow:
                    speed *= movementStats.slowSpeedReduction;
                    break;
                case TileMovementType.Fast:
                    speed *= 1.25f;
                    break;
                case TileMovementType.SuperFast:
                    speed *= 1.5f;
                    break;
                case TileMovementType.LightningFast:
                    speed *= 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (!playerRobot.IsOnGround()) speed *= movementStats.airSpeedIncrease;
            velocity.x = sign * Mathf.Lerp(0, speed, wishdir);
        }

        private void UpdateVerticalMovement(ref Vector2 velocity)
        {
            var fluidCollisionInformation = playerRobot.fluidCollisionInformation;
            float fluidGravityModifer =
                fluidCollisionInformation.Colliding ? fluidCollisionInformation.GravityModifier : 1f;

            if (jumpEvent != null)
            {
                UpdateJumpEvent();
                return;
            }

            float fluidSpeedModifier =
                fluidCollisionInformation.Colliding ? fluidCollisionInformation.SpeedModifier : 1f;


            if (holdingJump && rocketBoots != null && rocketBoots.FlightTime > 0)
            {
                RocketBootUpdate(ref velocity, fluidSpeedModifier);
                return;
            }

            rb.gravityScale = playerRobot.DefaultGravityScale * fluidGravityModifer;
            const float BONUS_FALL_MODIFIER = 1.25f;
            if (holdingDown) rb.gravityScale *= BONUS_FALL_MODIFIER;

            if (slopeState.HasValue && currentTileMovementType != TileMovementType.Slippery && playerRobot.IgnoreSlopePlatformFrames < 0 && !playerRobot.CollisionStateActive(CollisionState.OnPlatform))
            {
                if (slopeState.Value == Direction.Left)
                {
                    walkingDownSlope = inputDir < 0;
                    
                } else if (slopeState.Value == Direction.Right)
                {
                    walkingDownSlope = inputDir > 0;
                }

                float bonusDownwardsSpeed = 0;//Mathf.Abs(velocity.x) / 150;
                float dir = walkingDownSlope ? -(1+bonusDownwardsSpeed) : (1-bonusDownwardsSpeed);
                velocity.y = dir*Mathf.Abs(velocity.x);
            }
            return;

            void UpdateJumpEvent()
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
            }
        }

        public void OnGrounded()
        {
            if (bonusJumps <= 0)
            {
                bonusJumps = playerRobot.RobotUpgradeLoadOut.SelfLoadOuts.GetCurrent()?.GetDiscreteValue((int)RobotUpgrade.BonusJump) ?? 0;
            }

            rocketBoots?.SetFlightTime(RobotUpgradeUtils.GetDiscreteValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,(int)RobotUpgrade.RocketBoots));
        }

        public void RocketBootUpdate(ref Vector2 velocity, float fluidSpeedModifier)
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
            rocketBoots.UpdateBoost(holdingJump);
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
            if (playerRobot.IgnorePlatformFrames > 0 || (!CanJump() && coyoteFrames <= 0)) return;
            
            if (bonusJumps > 0 && coyoteFrames <= 0)
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

            coyoteFrames = 0;
            fallTime = 0;
            slipperyFrames /= 2;
            playerRobot.OnJump();
            jumpEvent = new JumpEvent();
        }

        void OnJumpReleased(InputAction.CallbackContext context)
        {
            holdingJump = false;
            jumpEvent = null;
        }
        
        void OnDownPressed(InputAction.CallbackContext context)
        {
            holdingDown = true;
        }

        void OnDownReleased(InputAction.CallbackContext context)
        {
            holdingDown = false;
        }
        
        public float GetFriction()
        {
            if (slipperyFrames > 0) return movementStats.iceFriction;

            return playerRobot.IsOnGround() ? movementStats.friction : movementStats.airFriction;
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

        public void OnWallCollision()
        {
            slipperyFrames = 0;
        }
        

        public void OnFluidCollision()
        {
            fallTime = 0;
        }

        public void OnSlopeExit()
        {
            slopeFrames = walkingDownSlope ? 5 : 0; // Give one frame of matching horizontal and vertical movement so player doesn't fall of ledge when walking down
            if (!walkingDownSlope)
            {
                slopeState = null;
            }
            if (holdingJump) return;
            var vector2 = rb.velocity;
            vector2.y = 0;
            rb.velocity = vector2;
        }

        public void OnTeleport()
        {
            fallTime = 0;
        }

        public void ToggleRocketBoots(bool active)
        {
            switch (active)
            {
                case true when rocketBoots == null:
                {
                    rocketBoots = new RocketBoots(playerRobot);
                    if (playerRobot.IsGrounded())
                    {
                        rocketBoots.SetFlightTime(RobotUpgradeUtils.GetDiscreteValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,(int)RobotUpgrade.RocketBoots));
                    }

                    break;
                }
                case false when rocketBoots != null:
                    rocketBoots = null;
                    break;
            }
        }

        public void OnSlopeCollision(Direction slopeDirection)
        {
            slopeFrames = int.MaxValue;
            slopeState = slopeDirection;
        }
        
        public void OnSlopeStay(Direction slopeDirection)
        {
            slopeState = slopeDirection;
        }
        
    }
}
