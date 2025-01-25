using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Transmutable;
using UnityEngine;
using Newtonsoft.Json;

namespace Entities {
    public class ItemEntity : Entity, ISerializableEntity
    {
        public bool CollidedWithPlayer;
        private const float MIN_MERGE_TIME = 1f;
        private bool firedDelayedCast = false;
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
            return new SeralizedEntityData(
                type: EntityType.Item,
                transform.position,
                ItemSlotFactory.seralizeItemSlot(itemSlot)
            );
        }
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
        
            ItemEntity itemEntity = other.gameObject.GetComponentInParent<ItemEntity>();
            TryMergeItemEntities(itemEntity);
        }
        
        private void TryMergeItemEntities(ItemEntity other)
        {
            if (lifeTime < MIN_MERGE_TIME)
            {
                StartCoroutine(DelayMergeCast(MIN_MERGE_TIME - lifeTime));
                return;
            }

            MergeItemEntities(other);
        }

        private void MergeItemEntities(ItemEntity other)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            
            ItemSlot hitObjectSlot = other.itemSlot;
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) return;
            
            ItemSlotUtils.InsertIntoSlot(itemSlot,hitObjectSlot,Global.MaxSize);
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) Destroy(other.gameObject);
        }

        private IEnumerator DelayMergeCast(float delay)
        {
            if (firedDelayedCast) yield break;
            firedDelayedCast = true;
            yield return new WaitForSeconds(delay + 0.05f);
            
            RaycastHit2D[] leftHits = Physics2D.RaycastAll(transform.position, Vector2.left, 0.25f, 1 << LayerMask.NameToLayer("Entity"));
            foreach (RaycastHit2D leftHit in leftHits) {
                if (leftHit.collider.gameObject.Equals(gameObject)) continue;
                if (ItemSlotUtils.IsItemSlotNull(itemSlot) || itemSlot.amount > Global.MaxSize) break;
                if (leftHit.collider.gameObject.tag != "ItemEntity") continue;
                ItemEntity leftEntity = leftHit.collider.gameObject.GetComponent<ItemEntity>();
                MergeItemEntities(leftEntity);
                
            }
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

