using System;
using Player;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Entities.Mob.Movement
{
    public interface IEnterFluidEntityMovement
    {
        public void OnEnterFluid(Vector2 fluidPosition);
        public void OnExitFluid();
    }
    public class MobFlyChasePlayer : MonoBehaviour, IEnterFluidEntityMovement
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
        private bool inFluid;
        private bool escapingFluid;
        private const float FLUID_MOVE_RANGE = 0.5f;

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
            if (escapingFluid)
            {
                transform.position = Vector2.MoveTowards(transform.position, randomNearPosition, ChaseSpeed * Time.deltaTime);
                spriteRenderer.flipX = randomNearPosition.x < transform.position.x;
                float distance = Vector2.Distance(randomNearPosition, transform.position);
                if (distance < 0.01f)
                {
                    if (inFluid)
                    {
                        randomNearPosition = GetRandomNearByPosition(FLUID_MOVE_RANGE);
                    }
                    else
                    {
                        escapingFluid = false;
                    }
                }
                return;
            }
            if (seesPlayer)
            {
                ChasePlayer();
            }
            else
            {
                WanderRandomly();
            }
        }

        public void OnEnterFluid(Vector2 fluidPosition)
        {
            escapingFluid = true;
            inFluid = true;
            Vector2 direction = ((Vector2)transform.position - fluidPosition).normalized;
            randomNearPosition = (Vector2)transform.position + direction * FLUID_MOVE_RANGE;
        }

        public void OnExitFluid()
        {
            inFluid = false;
        }
        void FixedUpdate()
        {
            if (inFluid) return;
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
                randomNearPosition = GetRandomNearByPosition(WanderRange);
                spriteRenderer.flipX = randomNearPosition.x < transform.position.x;
                timeSinceLastDirectionChange = 0f;
            }
            
            transform.position = Vector2.MoveTowards(transform.position, randomNearPosition, ChaseSpeed * Time.deltaTime);
        }

        private Vector2 GetRandomNearByPosition(float range)
        {
            return (Vector2)transform.position + range * UnityEngine.Random.insideUnitCircle;
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
                if (hit.collider && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("PlayerComponent"))) return true;
            }
            return false;
        }
    }
}
