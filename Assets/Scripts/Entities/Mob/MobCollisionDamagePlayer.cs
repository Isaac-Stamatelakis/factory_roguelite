using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class MobCollisionDamagePlayer : MonoBehaviour
{
    public float Damage;
    public void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        Debug.Log(other.gameObject.name);
        other.gameObject.GetComponent<PlayerRobot>().PlayerDamage.Damage(Damage);
    }
}
