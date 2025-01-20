using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Transmutable;
using UnityEngine;
using Newtonsoft.Json;

namespace Entities {
    public class ItemEntity : Entity, ISerializableEntity
    {
        [SerializeField] public ItemSlot itemSlot;
        [SerializeField] protected float lifeTime = 0f;
        public float LifeTime {get{return lifeTime;}}

        public override void initalize()
        {
            gameObject.AddComponent<SpriteRenderer>();
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            gameObject.AddComponent<BoxCollider2D>();
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            gameObject.layer = LayerMask.NameToLayer("Entity");

            BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        
            spriteRenderer.sprite = itemSlot.itemObject.getSprite();

            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                spriteRenderer.color = transmutableItemObject.getMaterial().color;
            }
            transform.localScale = new Vector3(0.5f, 0.5f,1f);
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }

        private void IterateLifeTime() {
            lifeTime += Time.fixedDeltaTime;
            if (lifeTime > Global.ItemEntityLifeSpawn) {
                Destroy(gameObject);
            }
        }

        public virtual void FixedUpdate()
        {
            IterateLifeTime();
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
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
        
            ItemEntity itemEntity = other.gameObject.GetComponentInParent<ItemEntity>();
            MergeItemEntities(itemEntity);
        }
        
        private void MergeItemEntities(ItemEntity other) {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            
            ItemSlot hitObjectSlot = other.itemSlot;
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) return;
            
            ItemSlotUtils.InsertIntoSlot(itemSlot,hitObjectSlot,Global.MaxSize);
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) Destroy(other.gameObject);
        }

        public void deseralize(string data)
        {
            
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

