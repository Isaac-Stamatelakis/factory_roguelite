using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Instances {
    /// <summary>
    /// The starter robot
    /// <summary>   
    [CreateAssetMenu(fileName = "R~New Happy", menuName = "Robots/HAPPY")]
    public class HAPPY : Robot
    {
        public override void handleMovement(Transform playerTransform)
        {
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            PlayerRobot playerRobot = playerTransform.GetComponent<PlayerRobot>();

            Vector2 velocity = Vector2.zero;
            velocity.y = rb.velocity.y;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                velocity.x = -4f;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                velocity.x = +4f;
            }
            if (playerRobot.OnGround && rb.velocity.y <= 0 && Input.GetKey(KeyCode.Space)) {
                if (Input.GetKey(KeyCode.S)) {
                    playerRobot.NoCollisionWithPlatformCounter=5;
                } else {
                    velocity.y += 10f;
                }
                
                playerRobot.OnGround = false;
            }
            
            
            rb.velocity = velocity;
        }

        public override void init(GameObject playerGameObject)
        {
            Rigidbody2D rigidbody2D = playerGameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}

