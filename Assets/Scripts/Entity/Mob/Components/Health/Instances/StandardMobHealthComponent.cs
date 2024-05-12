using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Entities.Mobs {
    public class StandardMobHealthComponent : MobHealthComponent
    {
        [SerializeField] protected float health = 10;

        public override void deseralize(string data)
        {
            try {
                health = Convert.ToInt32(data);
            } catch (FormatException e) {
                Debug.LogError(e);
                health = 0;
            } 
            
        }

        public override void hit(float damage, DamageType damageType)
        {
            if (weaknesses.Contains(damageType)) {
                health -= 2 * damage;
            } else if (resistances.Contains(damageType)) {
                health -= 0.5f * damage;
            } else {
                health -= damage;
            }
        }

        public override string serialize()
        {
            return health.ToString();
        }
    }
}

