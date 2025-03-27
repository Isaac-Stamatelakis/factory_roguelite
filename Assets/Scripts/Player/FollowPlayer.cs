using UnityEngine;

namespace Player
{
    public class FollowPlayer : MonoBehaviour
    {
        private Transform playerTransform;
        public void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public void LateUpdate()
        {
            transform.position = playerTransform.position;
        }
    }
}
