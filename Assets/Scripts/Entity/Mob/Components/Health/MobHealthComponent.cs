using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Mobs {
    public abstract class MobHealthComponent : MonoBehaviour, ISerializableMobComponent
    {
        [SerializeField] protected List<DamageType> weaknesses;
        [SerializeField] protected List<DamageType> resistances;
        public abstract void hit(float damage, DamageType damageType);
        public abstract void deseralize(string data);
        public abstract string serialize();
    }
}

