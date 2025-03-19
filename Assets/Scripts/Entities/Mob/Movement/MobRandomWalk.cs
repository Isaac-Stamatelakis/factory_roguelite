using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Mobs {
    public class MobRandomWalk : MonoBehaviour
    {
        [Header("Chance that every Fixed Update (25/second), the entity walks")]
        [SerializeField] private int frequency = 100;
        [Header("Range of time they walk")]
        [SerializeField] private Vector2Int durationRange = new Vector2Int(150,300);
        [SerializeField] private float speed;
        [SerializeField] private float jumpHeight;
        [SerializeField] private Animation walkAnimation;
        [SerializeField] private Animation idleAnimation;

        private int duration;
        private Direction direction;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        public void Start() {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
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
                    spriteRenderer.flipX = true;
                } else {
                    direction = Direction.Right;
                    spriteRenderer.flipX = false;
                }
            } else {
                if (direction == Direction.Left) {
                    rb.AddForce(speed*Vector2.left);
                } else if (direction == Direction.Right) {
                    rb.AddForce(speed*Vector2.right);
                }
                duration--;
            }
        }
    }

}
