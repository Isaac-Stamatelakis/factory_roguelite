using Player.Controls;
using Player.Movement.Standard;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using TileEntity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{
    
    public class ClimbingMovement : BasePlayerMovement
    {
        private Rigidbody2D rb;
        private InputActions.LadderMovementActions movementActions;

        private float movementDirection;
        // Start is called before the first frame update
        public ClimbingMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            rb = playerRobot.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX |  RigidbodyConstraints2D.FreezeRotation;
            playerRobot.AnimationController.ToggleBool(PlayerAnimationState.Air,true);
            
            movementActions = playerRobot.GetComponent<PlayerScript>().InputActions.LadderMovement;
            movementActions.Move.performed += OnMovePress;
            movementActions.Move.canceled += OnMoveRelease;
            movementActions.HorizontalEscape.performed += OnEscapeHorizontalPress;
            movementActions.JumpEscape.performed += OnEscapeVerticalPress;
            
            movementActions.Enable();
        }

        private void OnMovePress(InputAction.CallbackContext context)
        {
            movementDirection = context.ReadValue<float>();
        }

        private void OnMoveRelease(InputAction.CallbackContext context)
        {
            movementDirection = 0;
        }

        private void OnEscapeHorizontalPress(InputAction.CallbackContext context)
        {
            playerRobot.SetStandardMovementWithSpeed(context.ReadValue<float>());
            playerRobot.BlockClimbingFrames = 5;
        }

        private void OnEscapeVerticalPress(InputAction.CallbackContext context)
        {
            playerRobot.SetMovementState(PlayerMovementState.Standard);
            playerRobot.BlockClimbingFrames = 5;
            playerRobot.ResetIgnorePlatformFrames();
        }

        public override void MovementUpdate()
        {
            if (movementDirection == 0)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            Vector2 position = playerRobot.transform.position;
            IClimableTileEntity climableTileEntity = playerRobot.GetClimbable(position);
            if (climableTileEntity == null)
            {
                if (movementDirection < 0)
                {
                    playerRobot.SetStandardMovementHoldingDown();
                }
                else
                {
                    playerRobot.SetMovementState(PlayerMovementState.Standard);
                }
                
                return;
            }

            if (movementDirection > 0)
            {
                playerRobot.PlatformCollider.enabled = false;
            }
            else
            {
                IClimableTileEntity below = playerRobot.GetClimbable(position + Vector2.down);
                playerRobot.PlatformCollider.enabled = below == null;
            }
            
            Vector2 velocity = rb.velocity;
            velocity.y = climableTileEntity.GetSpeed() * movementDirection;
            rb.velocity = velocity;
        }

        public override void FixedMovementUpdate()
        {
            playerRobot.AnimationController.PlayAnimation(playerRobot.IsGrounded() ? PlayerAnimation.Idle : PlayerAnimation.Air);
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        public override void Disable()
        {
            movementActions.Move.performed -= OnMovePress;
            movementActions.Move.canceled -= OnMoveRelease;
            movementActions.HorizontalEscape.performed -= OnEscapeHorizontalPress;
            movementActions.JumpEscape.performed -= OnEscapeVerticalPress;
            movementActions.Disable();
        }

        protected override InputActionMap GetInputActionMap()
        {
            return movementActions;
        }
    }
}
