using System.Collections;
using System.Collections.Generic;
using ItemModule;
using UnityEngine;

namespace Entities.Mobs {
    public class Penguin : MobEntity
    {
        [SerializeField] private float health;
        [SerializeField] private LootTable lootTable;
        private Rigidbody2D rb;
        public LootTable getLootTable()
        {
            return lootTable;
        }
        public override void initalize()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void FixedUpdate() {
            
        }
    }
}

