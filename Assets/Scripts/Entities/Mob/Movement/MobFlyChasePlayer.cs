using System;
using Player;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Entities.Mob.Movement
{
    public class MobFlyChasePlayer : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Transform playerTransform;
        private bool seesPlayer;
        private int counter = 0;
        const int SEARCH_TIME = 10;
        public float ChaseSpeed = 3f;
        public float ChangeRandomDirectionTime = 2f;
        public float WanderRange = 3f;
        private float timeSinceLastDirectionChange = 0;
        private Vector2 randomDirection;

        private static int PlayerLayer;
        private static int BlockLayer;
        public void Start()
        {
            rb = GetComponent<Rigidbody2D>();
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
        }

        private void WanderRandomly()
        {
            timeSinceLastDirectionChange += Time.fixedTime;
            
            if (timeSinceLastDirectionChange >= ChangeRandomDirectionTime)
            {
                randomDirection = GetRandomDirection();
                timeSinceLastDirectionChange = 0f;
            }
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + randomDirection, ChaseSpeed /2f * Time.deltaTime);
        }

        private Vector2 GetRandomDirection()
        {
            return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * WanderRange;
        }
        private bool CanSeePlayer()
        {
            const int layer = Global.BLOCK_LAYER | Global.PLAYER_LAYER;
            Vector2 direction = (Vector2)(playerTransform.position - transform.position).normalized;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 15, layer);
          
            return hit.collider && hit.collider.CompareTag("Player");
        }
    }
}
