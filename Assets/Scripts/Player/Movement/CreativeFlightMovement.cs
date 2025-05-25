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
        private InputActions.CreativeMovementActions creativeMovementActions;

        private Vector2 movementVector;
        // Start is called before the first frame update
        public CreativeFlightMovement(PlayerRobot playerRobot) : base(playerRobot)
        {
            Rigidbody2D rb = playerRobot.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            playerTransform = playerRobot.transform;
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            
            creativeMovementActions = playerRobot.GetComponent<PlayerScript>().InputActions.CreativeMovement;
            creativeMovementActions.Move.performed += OnMovePress;
            creativeMovementActions.Move.canceled += OnMoveRelease;
            
            creativeMovementActions.Enable();
        }

        public void OnMovePress(InputAction.CallbackContext context)
        {
            movementVector = context.ReadValue<Vector2>();
        }

        public void OnMoveRelease(InputAction.CallbackContext context)
        {
            movementVector = Vector2.zero;
        }

        public override void MovementUpdate()
        {
            Vector3 position = playerTransform.position;
            float movementSpeed = DevMode.Instance.FlightSpeed * Time.deltaTime;
            position += (Vector3)movementVector * movementSpeed;
            playerTransform.position = position;
            spriteRenderer.flipX = movementVector.x < 0;
        }
        
        public override void Dispose()
        {
            creativeMovementActions.Disable();
        }
    }
}
