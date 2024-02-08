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
            Vector3 position = playerTransform.position;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                position.x -= 1;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                position.x += 1;
            }
            if (Input.GetKey(KeyCode.Space)) {
                position.y += 1;
            }
            playerTransform.position = position;
        }

        public override void init(GameObject playerGameObject)
        {
            Rigidbody2D rigidbody2D = playerGameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}

