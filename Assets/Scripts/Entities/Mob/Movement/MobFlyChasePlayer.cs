using System;
using Player;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Entities.Mob.Movement
{
    public class MobFlyChasePlayer : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Transform playerTransform;
        private bool seesPlayer;
        private int counter = 0;
        const int SEARCH_TIME = 10;
        public float ChaseSpeed = 3f;
        public float ChangeRandomDirectionTime = 2f;
        public float WanderRange = 3f;
        private float timeSinceLastDirectionChange = 0;
        private Vector2 randomNearPosition;

        private static int PlayerLayer;
        private static int BlockLayer;
        public void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerTransform = PlayerManager.Instance.GetPlayer().transform;
            seesPlayer = CanSeePlayer();
            counter = UnityEngine.Random.Range(0, SEARCH_TIME);
        }

        public void Update()
        {
            if (seesPlayer)
            {
                ChasePlayer();
            }
            else
            {
                WanderRandomly();
            }
        }

        void FixedUpdate()
        {
            counter++;
            if (counter < SEARCH_TIME) return;
            counter = 0;
            seesPlayer = CanSeePlayer();
        }
        private void ChasePlayer()
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, ChaseSpeed * Time.deltaTime);
            randomNearPosition = playerTransform.position;
            spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
        }

        private void WanderRandomly()
        {
            timeSinceLastDirectionChange += Time.deltaTime;
            
            if (timeSinceLastDirectionChange >= ChangeRandomDirectionTime)
            {
                randomNearPosition = GetRandomNearByPosition();
                spriteRenderer.flipX = randomNearPosition.x < transform.position.x;
                timeSinceLastDirectionChange = 0f;
            }
            
            transform.position = Vector2.MoveTowards(transform.position, randomNearPosition, ChaseSpeed * Time.deltaTime);
        }

        private Vector2 GetRandomNearByPosition()
        {
            return (Vector2)transform.position + WanderRange * UnityEngine.Random.insideUnitCircle;
        }
        private bool CanSeePlayer()
        {
            const int layer = Global.BLOCK_LAYER | Global.PLAYER_LAYER;
            Vector2 direction = (Vector2)(playerTransform.position - transform.position).normalized;
            const int rayCount = 5;
            
            const float coneAngle = 18;
            const float angleStep = coneAngle / (rayCount - 1);
            const float halfConeAngle = coneAngle / 2;
            
            for (int i = 0; i < rayCount; i++)
            {
                float angle = -halfConeAngle + angleStep * i;
                Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * direction;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 15, layer);
                if (hit.collider && hit.collider.CompareTag("Player")) return true;
            }
            return false;
        }
    }
}
