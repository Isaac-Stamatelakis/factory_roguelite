using Player.Controls;
using Player.Movement.Standard;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{

    public class FlightMovement : BasePlayerMovement
    {
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private InputActions.FlightMovementActions movementActions;

        private Vector2 movementVector;

        // Start is called before the first frame update
        public FlightMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            rb = playerRobot.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            playerRobot.AnimationController.ToggleBool(PlayerAnimationState.Air,true);
            movementActions = playerRobot.GetComponent<PlayerScript>().InputActions.FlightMovement;
            movementActions.Move.performed += OnMovePress;
            movementActions.Move.canceled += OnMoveRelease;
            movementActions.Enable();
        }

        private void OnMovePress(InputAction.CallbackContext context)
        {
            movementVector = context.ReadValue<Vector2>();
        }

        private void OnMoveRelease(InputAction.CallbackContext context)
        {
            movementVector = Vector2.zero;
        }

        public override void MovementUpdate()
        {
            if (movementVector == Vector2.zero)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            float speed = 5;
            float speedUpgrades = RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut?.SelfLoadOuts,
                (int)RobotUpgrade.Speed);
            if (playerRobot.TryConsumeEnergy(
                    (ulong)(speedUpgrades * 16 *
                            (SelfRobotUpgradeInfo.SPEED_INCREASE_COST_PER_SECOND * Time.deltaTime)), 0.1f))
            {
                speed += speedUpgrades;
            }
            
            Vector2 velocity = movementVector * speed;
            if (movementVector.x != 0) spriteRenderer.flipX = velocity.x < 0;
            
            rb.velocity = velocity;
        }

        public override void FixedMovementUpdate()
        {
            
        }

        public override void Disable()
        {
            movementActions.Move.performed -= OnMovePress;
            movementActions.Move.canceled -= OnMoveRelease;
            movementActions.Disable();
        }

        protected override InputActionMap GetInputActionMap()
        {
            return movementActions;
        }
    }
}
