using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Instances {
    
    /// <summary>
    /// The starter robot
    /// <summary>   
    [CreateAssetMenu(fileName = "RB~New Happy INF", menuName = "Robots/HAPPYINF")]
    public class HAPPY_INF : Robot
    {
        [SerializeField]
        public float speed;
        public override void handleMovement(Transform playerTransform)
        {
            Vector3 position = playerTransform.position;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                position.x -= speed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                position.x += speed;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                position.y += speed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                position.y -= speed;
            }
            playerTransform.position = position;
        }

        public override void init(GameObject playerGameObject)
        {
            Rigidbody2D rigidbody2D = playerGameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Static;
        }
    }
}

