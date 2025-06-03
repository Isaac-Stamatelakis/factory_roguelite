using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Mob.Display
{
    
    public class WormBodyController : MonoBehaviour
    {
        private enum MovementMode
        {
            ChasePlayer,
            ChaseOldPlayerPosition,
            RunAway
        }
        
        public float Speed;
        public float AngularSpeed;
        public float MaxBonusSpeed;
        public float RunAwayDistance;
        public float Accerleration;
        public float HeadDamage;
        public float BodyDamage;
        [SerializeField] private Sprite headSprite;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Sprite tailSprite;
        
        [SerializeField] private int length;
        [SerializeField] private float follow;
        
        private MovementMode movementMode;

        private Transform[] partTransforms;
        private float bonusSpeed;
        private Vector2 chasePosition;
        const float SPRITE_ANGLE = 90;
        private Transform playerTransform;
        private float currentRoutineTime;
        private const float MAX_ROUTINE_TIME = 10f;
        private const int SPAWNS_PER_UPDATE = 2;
        
        private bool assembled = false;
        public void Start()
        {
            partTransforms = new Transform[length+2];
            partTransforms[0] = CreatePart(headSprite,"Head",HeadDamage);
            AssemblePart();
            playerTransform = PlayerManager.Instance.GetPlayer().transform;
            var vector3 = transform.localPosition;
            vector3.z = 5;
            transform.localPosition = vector3;
        }
        
        public void FixedUpdate()
        {
            if (!assembled)
            {
                AssemblePart();
            }
            currentRoutineTime -= Time.deltaTime;
            switch (movementMode)
            {
                case MovementMode.ChasePlayer:
                    ChasePlayer();
                    break;
                case MovementMode.ChaseOldPlayerPosition:
                    ChaseOldPlayerPosition();
                    break;
                case MovementMode.RunAway:
                    RunAway();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssemblePart()
        {
            int startIdx = transform.childCount;
            for (int i = 0; i < SPAWNS_PER_UPDATE; i++)
            {
                int idx = i + startIdx;
                partTransforms[idx] = CreatePart(bodySprite,$"Body{idx}",BodyDamage);
                if (idx < length) continue;
                partTransforms[length+1] =CreatePart(tailSprite,"Tail",BodyDamage);
                assembled = true;
                break;
            }
        }

        private Transform CreatePart(Sprite sprite, string partName, float damage)
        {
            GameObject partObject = new GameObject(partName);
            partObject.AddComponent<WormBodyPart>();
            partObject.layer = gameObject.layer;
            BoxCollider2D partHealthCollider = partObject.AddComponent<BoxCollider2D>();
            partObject.transform.SetParent(transform,false);
            Vector3 spawnPosition = partObject.transform.position;
            spawnPosition.z += transform.childCount / 1024f;
            partObject.transform.position = spawnPosition;
            SpriteRenderer spriteRenderer = partObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            GameObject damageCollider = new GameObject("Damage");
            damageCollider.transform.SetParent(partObject.transform,false);
            BoxCollider2D boxCollider2D = damageCollider.AddComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            boxCollider2D.size = partHealthCollider.size / 2f;
            MobDamageTrigger mobDamageTrigger = damageCollider.AddComponent<MobDamageTrigger>();
            //partObject.AddComponent<MobFluidTrigger>(); For now this is too laggy
            mobDamageTrigger.Damage = damage;
            return partObject.transform;
        }

        public void ChasePlayer()
        {
            bonusSpeed += Accerleration*Time.deltaTime;
            MoveTowardsPosition(Speed+bonusSpeed, playerTransform.position);
            
            if (bonusSpeed > MaxBonusSpeed || currentRoutineTime < 0)
            {
                currentRoutineTime = 2f;
                movementMode = MovementMode.ChaseOldPlayerPosition;
                chasePosition = playerTransform.position;
                bonusSpeed = 0;
            }
        }
        
        private void ChaseOldPlayerPosition()
        {
            MoveTowardsPosition(Speed, chasePosition);
            float distance = ((Vector2)partTransforms[0].position - chasePosition).magnitude;
            if (distance < 0.1f || currentRoutineTime < 0)
            {
                currentRoutineTime = MAX_ROUTINE_TIME;
                movementMode = MovementMode.RunAway;
                chasePosition += RunAwayDistance * (chasePosition-(Vector2)partTransforms[0].position).normalized;
            }
        }

        private void RunAway()
        {
            MoveTowardsPosition(Speed, chasePosition);
            float distance = ((Vector2)partTransforms[0].position - chasePosition).magnitude;
            if (distance < 0.1f || currentRoutineTime < 0) 
            {
                currentRoutineTime = MAX_ROUTINE_TIME;
                movementMode = MovementMode.ChasePlayer;
            }
        }

        private void MoveTowardsPosition(float realSpeed, Vector2 target)
        {
            Vector2 direction = (target-(Vector2)partTransforms[0].position).normalized;
            
            if (direction == Vector2.zero) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = partTransforms[0].eulerAngles.z+SPRITE_ANGLE;
            float maxAngleChange = Time.deltaTime*25f*AngularSpeed;
            
            float angleDifference = Mathf.DeltaAngle(currentAngle, angle);
            float clampedAngleDifference = Mathf.Clamp(angleDifference, -maxAngleChange, maxAngleChange);
            
            float newAngle = currentAngle + clampedAngleDifference;
            partTransforms[0].rotation = Quaternion.Euler(0, 0, newAngle-SPRITE_ANGLE);
            Vector2 moveDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

            partTransforms[0].transform.position += (Vector3)moveDirection * (realSpeed * Time.deltaTime);
            
            for (int i = 1; i < partTransforms.Length; i++)
            {
                Transform current = partTransforms[i];
                if (!current) break;
                Transform previous = partTransforms[i - 1];
                Vector3 newPosition =  Vector2.Lerp(current.position, previous.position, follow);
                newPosition.z = current.position.z;
                current.position = newPosition;
                
                Vector2 dir = (previous.position - current.position).normalized;
                float bodyAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg-SPRITE_ANGLE;
                current.rotation = Quaternion.Euler(0, 0, bodyAngle);
            }
        }

        
    }
}
