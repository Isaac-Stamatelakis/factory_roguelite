using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule;
using TileMapModule.Type;

namespace PlayerModule {
    public class PlayerPlatformDetector : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider2D physicCollider;
        [SerializeField] private Rigidbody2D rb;
        private bool onPlatform;
        public bool OnPlatform {get => onPlatform;}
        private bool jumpDownDisable;

        public void FixedUpdate() {
            if (jumpDownDisable) {
                return;
            }
            physicCollider.enabled = rb.velocity.y <= 0.1f;
            
        }

        public void Update() {
            if (onPlatform && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Space)) {
                onPlatform = false;
                StartCoroutine(disablePlatformCollisions());
            }
        }
        private IEnumerator disablePlatformCollisions() {
            jumpDownDisable = true;
            physicCollider.enabled = false;
            yield return new WaitForSeconds(0.15f);
            physicCollider.enabled = true;
            jumpDownDisable = false;
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            onPlatform = getOnPlatform(other);
        }

        private bool getOnPlatform(Collider2D other) {
            TileGridMap tileGridMap = other.GetComponent<TileGridMap>();
            if (tileGridMap == null) {
                return false;
            }
            return tileGridMap.type == TileMapType.Platform;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            onPlatform = false;
        }
    }
}

