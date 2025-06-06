using System;
using System.Collections;
using System.Collections.Generic;
using Fluids;
using Item.Display;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using Items;
using Items.Tags;
using Items.Transmutable;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace Entities {
    public class ItemEntity : Entity, ISerializableEntity
    {
        private const int BLINK_RATE = 4;
        private const float BLINK_THRESHOLD = 10;
        private const float LIFE_SPAN = 300;
        private const float MAX_FALL_SPEED = 10f;
        private Rigidbody2D rb;
        [SerializeField] public ItemSlot itemSlot;
        [SerializeField] protected float lifeTime = 0f;
        private SpriteRenderer spriteRenderer;
        private float perservedSpeed = 0;
        private bool touchingBoundary = false;
        
        public float LifeTime {get{return lifeTime;}}

        private const int CAST_RATE = 1;

        public override void Initialize()
        {
            gameObject.AddComponent<BoxCollider2D>();
            rb = gameObject.AddComponent<Rigidbody2D>();

            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            gameObject.layer = LayerMask.NameToLayer("ItemEntity");

            BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        
            ItemWorldDisplay itemWorldDisplay = gameObject.AddComponent<ItemWorldDisplay>();
            itemWorldDisplay.Display(itemSlot);
            spriteRenderer = GetComponent<SpriteRenderer>();
            transform.localScale = new Vector3(0.5f, 0.5f,1f);
           
            SetColliderSize(boxCollider);
        }

        private void SetColliderSize(BoxCollider2D boxCollider2D)
        {
            if (spriteRenderer.sprite)
            {
                SetSizeFrameSprite(spriteRenderer.sprite);   
                return;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).tag != "SpriteOverlay") continue;
                var overlaySpriteRenderer = transform.GetChild(i).GetComponent<SpriteRenderer>();
                SetSizeFrameSprite(overlaySpriteRenderer.sprite);
                return;
            }
            // Default
            boxCollider2D.size = Global.TILE_SIZE * Vector2.one;

            return;
            void SetSizeFrameSprite(Sprite sprite)
            {
                Vector2 size = sprite.bounds.size;
                if (size.x * size.y >= 1)
                {
                    size.x *= 0.95f; // Slight down scaling so 32x32 pixel tiles don't get stuck in blocks
                }
                boxCollider2D.size = size;
                
            }
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
            if (!touchingBoundary)
            {
                perservedSpeed = rb.velocity.y;
            }
            IterateLifeTime();
            if ((int)lifeTime % CAST_RATE == 0)
            {
                RaycastHit2D[] leftHits = Physics2D.RaycastAll(transform.position, Vector2.left, 0.25f, 1 << LayerMask.NameToLayer("ItemEntity"));
                foreach (RaycastHit2D leftHit in leftHits) {
                    if (leftHit.collider.gameObject.Equals(gameObject)) continue;
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot) || itemSlot.amount > Global.MAX_SIZE) break;
                    if (leftHit.collider.gameObject.tag != "ItemEntity") continue;
                    ItemEntity leftEntity = leftHit.collider.gameObject.GetComponent<ItemEntity>();
                    MergeItemEntities(leftEntity);
                }
            }
            ClampFallSpeed();

        }

        public SeralizedEntityData serialize()
        {
            SerializedItemEntityData serializedItemEntityData = new SerializedItemEntityData(
                ItemSlotFactory.seralizeItemSlot(itemSlot),
                rb.velocity.x,
                touchingBoundary ? perservedSpeed : rb.velocity.y
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
        

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.tag == "PartitionBoundary")
            {
                touchingBoundary = true;
            }
        }

        public void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.tag == "PartitionBoundary")
            {
                var vector2 = rb.velocity;
                vector2.y = perservedSpeed;
                rb.velocity = vector2;
                touchingBoundary = false;
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Fluid")
            {
                Vector2 bottomPosition = (Vector2)transform.position + Vector2.down * GetComponent<SpriteRenderer>().bounds.extents.y;
                Vector2 collisionPoint = other.ClosestPoint(bottomPosition);
                FluidTileMap fluidTileMap = other.GetComponent<FluidTileMap>();
                fluidTileMap ??= other.GetComponentInParent<FluidTileMap>();
                FluidTileItem collidingFluid = fluidTileMap.GetFluidItem(collisionPoint);
                if (!collidingFluid) return;
                if (collidingFluid.fluidOptions.DestroysItems)
                {
                    GameObject.Destroy(gameObject);
                    return;
                }

                float slowFactor = collidingFluid.fluidOptions.SpeedSlowFactor;
                
                rb.gravityScale = slowFactor * slowFactor;
                rb.velocity *= rb.gravityScale;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Fluid")
            {
                rb.gravityScale = 1;
            }
        }

        private void MergeItemEntities(ItemEntity other)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            
            ItemSlot hitObjectSlot = other.itemSlot;
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) return;
            if (!ItemSlotUtils.AreEqualNoNullCheck(hitObjectSlot, itemSlot)) return;
            ItemSlotUtils.InsertIntoSlot(itemSlot,hitObjectSlot,Global.MAX_SIZE);
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) Destroy(other.gameObject);
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

