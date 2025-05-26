using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities.Mobs {
    public class MobRandomWalk : MonoBehaviour
    {
        private enum RotationMode
        {
            FlipSprite,
            RotateObject
        }
        [Header("Chance that every Fixed Update (25/second), the entity walks")]
        [SerializeField] private int frequency = 100;
        [Header("Range of time they walk")]
        [SerializeField] private Vector2Int durationRange = new Vector2Int(150,300);
        [SerializeField] private float speed;
        [SerializeField] private float jumpHeight;
        [SerializeField] private RotationMode rotationMode;
        private int duration;
        private Direction direction;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        
        public void Start() {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        }
        public void FixedUpdate() {
            if (duration < 0) {
                int ran = Random.Range(0,frequency);
                if (ran > 0) {
                    return;
                }
                duration = Random.Range(durationRange.x,durationRange.y);
                ran = Random.Range(0,2);
                if (ran == 0) {
                    direction = Direction.Left;
                    Rotate(true);
                } else {
                    direction = Direction.Right;
                    Rotate(false);
                }

                return;
            }
            if (direction == Direction.Left) {
                rb.AddForce(speed*Vector2.left);
            } else if (direction == Direction.Right) {
                rb.AddForce(speed*Vector2.right);
            }
            duration--;
        }

        private void Rotate(bool left)
        {
            switch (rotationMode)
            {
                case RotationMode.FlipSprite:
                    spriteRenderer.flipX = !left;
                    break;
                case RotationMode.RotateObject:
                    transform.localRotation = Quaternion.Euler(0, !left ? 180 : 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
