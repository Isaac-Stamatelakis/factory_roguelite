using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldModule {
    public class BackgroundImageController : MonoBehaviour
    {
        private static BackgroundImageController instance;
        private int yOffset = 1;
        private Transform playerTransform;
        private Vector3 offset;

        public static BackgroundImageController Instance { get => instance; }

        public void Awake() {
            instance = this;
        }
        void Start()
        {
            playerTransform = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 distance = playerTransform.position-offset;
            transform.localPosition = new Vector3(
                -distance.x/30,
                -distance.y/30+yOffset,
                transform.localPosition.z
            );
        }
        public void setOffset(Vector2 offset) {
            this.offset = offset;
        }
    }

}
