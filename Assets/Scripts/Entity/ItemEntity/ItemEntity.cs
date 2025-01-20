using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Newtonsoft.Json;

namespace Entities {
    public abstract class ItemEntity : Entity, ISerializableEntity
    {
        [SerializeField] public ItemSlot itemSlot;
        [SerializeField] protected float lifeTime = 0f;
        public float LifeTime {get{return lifeTime;}}

        protected void iterateLifeTime() {
            lifeTime += Time.fixedDeltaTime;
            if (lifeTime > Global.ItemEntityLifeSpawn) {
                Destroy(gameObject);
            }
        }

        public virtual void FixedUpdate()
        {
            iterateLifeTime();
        }

        public SeralizedEntityData serialize()
        {
            SeralizedItemEntity serializedItemSlot = new SeralizedItemEntity(
                ItemSlotFactory.seralizeItemSlot(itemSlot),
                transform.position.x,
                transform.position.y
            );
            return new SeralizedEntityData(
                type: EntityType.Item,
                transform.position,
                Newtonsoft.Json.JsonConvert.SerializeObject(serializedItemSlot)
            );
        }

        public void deseralize(string data)
        {
            throw new System.NotImplementedException();
        }

        private class SeralizedItemEntity {
            public string itemData;
            public float x;
            public float y;
            public SeralizedItemEntity(string itemData, float x, float y)
            {
                this.itemData = itemData;
                this.x = x;
                this.y = y;
            }
        }
    }
}

