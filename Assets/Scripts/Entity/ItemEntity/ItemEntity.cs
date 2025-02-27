using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Transmutable;
using UnityEngine;
using Newtonsoft.Json;

namespace Entities {
    public class ItemEntity : Entity, ISerializableEntity
    {
        private const int BLINK_RATE = 4;
        private const float BLINK_THRESHOLD = 10;
        private const float LIFE_SPAN = 300;
        private const float MIN_MERGE_TIME = 1f;
        private const float MAX_FALL_SPEED = 10f;
        private bool firedDelayedCast = false;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        [SerializeField] public ItemSlot itemSlot;
        [SerializeField] protected float lifeTime = 0f;
        
        public float LifeTime {get{return lifeTime;}}

        public override void initalize()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            gameObject.AddComponent<BoxCollider2D>();
            rb = gameObject.AddComponent<Rigidbody2D>();

            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
            if (lifeTime < LIFE_SPAN - BLINK_THRESHOLD) return;
            int dif = (int)(BLINK_RATE*(lifeTime - LIFE_SPAN));
            spriteRenderer.enabled = dif % 2 == 0;
            
            if (lifeTime > LIFE_SPAN) {
                Destroy(gameObject);
            }
        }

        private void ClampFallSpeed()
        {
            if (rb.velocity.y > MAX_FALL_SPEED)
            {
                var vector2 = rb.velocity;
                vector2.y = MAX_FALL_SPEED;
                rb.velocity = vector2;
            }
        }

        public virtual void FixedUpdate()
        {
            IterateLifeTime();
            ClampFallSpeed();

        }

        public SeralizedEntityData serialize()
        {
            SerializedItemEntityData serializedItemEntityData = new SerializedItemEntityData(
                ItemSlotFactory.seralizeItemSlot(itemSlot),
                rb.velocity.x,
                rb.velocity.y
            );
            return new SeralizedEntityData(
                type: EntityType.Item,
                transform.position,
                JsonConvert.SerializeObject(serializedItemEntityData)
            );
        }

        private class SerializedItemEntityData
        {
            public string ItemData;
            public float xVelocity;
            public float yVelocity;

            public SerializedItemEntityData(string itemData, float xVelocity, float yVelocity)
            {
                ItemData = itemData;
                this.xVelocity = xVelocity;
                this.yVelocity = yVelocity;
            }
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

        public static void SpawnFromData(Vector2 position, string data, Transform container)
        {
            if (data == null) return;
            try
            {
                SerializedItemEntityData serializedItemEntityData =JsonConvert.DeserializeObject<SerializedItemEntityData>(data);
                ItemSlot itemSlot = ItemSlotFactory.DeserializeSlot(serializedItemEntityData.ItemData);
                ItemEntityFactory.SpawnItemEntity(position,itemSlot,container,new Vector2(serializedItemEntityData.xVelocity,serializedItemEntityData.yVelocity));
            }
            catch (JsonSerializationException)
            {
                
            }
            
        }
    }
}

