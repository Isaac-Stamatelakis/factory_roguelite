using System;
using Player;
using Robot.Tool.Instances.Gun;
using Unity.VisualScripting;
using UnityEngine;

namespace Entities.Mob
{
    public class MobDamageTrigger : MonoBehaviour
    {
        public float Damage;

        public void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("DamagePlayer");
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerRobot playerRobot = other.GetComponent<PlayerRobot>();
            if (!playerRobot) return;
            if (!playerRobot.Damage(Damage)) return;
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            Vector2 direction = collisionPoint.normalized;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.AddForce(5*direction,ForceMode2D.Impulse);
        }
    }
}
