using Player.Controls;
using Player.Movement.Standard;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Movement
{
    
    public class CreativeFlightMovement : BasePlayerMovement
    {
        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private InputActions.FlightMovementActions flightMovement;

        private Vector2 movementVector;
        // Start is called before the first frame update
        public CreativeFlightMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            Rigidbody2D rb = playerRobot.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            playerTransform = playerRobot.transform;
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            playerRobot.AnimationController.ToggleBool(PlayerAnimationState.Air,true);
            
            flightMovement = playerRobot.GetComponent<PlayerScript>().InputActions.FlightMovement;
            flightMovement.Move.performed += OnMovePress;
            flightMovement.Move.canceled += OnMoveRelease;
            flightMovement.Enable();
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
            if (movementVector == Vector2.zero) return;
            Vector3 position = playerTransform.position;
            float movementSpeed = DevMode.Instance.FlightSpeed * Time.deltaTime;
            position += (Vector3)movementVector * movementSpeed;
            playerTransform.position = position;
            if (movementVector.x == 0) return;
            spriteRenderer.flipX = movementVector.x < 0;
        }
        
        public override void Dispose()
        {
            flightMovement.Move.performed -= OnMovePress;
            flightMovement.Move.canceled -= OnMoveRelease;
            flightMovement.Disable();
        }

        protected override InputActionMap GetInputActionMap()
        {
            return flightMovement;
        }
    }
}
