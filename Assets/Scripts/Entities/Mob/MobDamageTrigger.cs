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
        public void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerRobot playerRobot = other.GetComponent<PlayerRobot>();
            if (!playerRobot) return;
            if (!playerRobot.Damage(Damage)) return;
            Vector2 difference = (other.transform.position - transform.position).normalized;
            other.GetComponent<Rigidbody2D>().AddForce(10*difference,ForceMode2D.Impulse);
        }
    }
}
