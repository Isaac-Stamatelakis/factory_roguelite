using System;
using UnityEngine;

namespace Robot.Tool.Instances.Gun
{
    public class LaserGunExplosionProjectile : MonoBehaviour
    {
        private float damage;
        private float lifeTime;
        [SerializeField] float Acceleration;
        private Vector2 direction;
        private float speed;

        private const int NOT_EXPLODING = -1;
        private int explodeCounter = NOT_EXPLODING;
        const int TICK_RATE = 10;
        private int explosions = 0;
        public void Initialize(float damage, Vector2 direction, float speed)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            this.damage = damage;
            this.direction = direction;
            this.speed = speed;
        }

        public void Update()
        {
            speed += Acceleration * Time.deltaTime;
            if (speed > 0) transform.Translate(direction * (speed * Time.deltaTime), Space.World);
            lifeTime += Time.deltaTime;
            if (lifeTime > 5f) Destroy(gameObject);
        }

        public void FixedUpdate()
        {
            
            if (explodeCounter == NOT_EXPLODING) return;
            explodeCounter++;
            if (explodeCounter < TICK_RATE) return;
            explodeCounter = 0;
            explosions++;
            int entityLayer = 1 << LayerMask.NameToLayer("Entity");
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position,8f,Vector2.zero,Mathf.Infinity,entityLayer);
            
            foreach (var hit in hits)
            {
                Vector2 damageDirection = hit.collider.ClosestPoint(transform.position).normalized;
                IDamageableEntity damageableEntity = hit.collider.GetComponent<IDamageableEntity>();
                damageableEntity?.Damage(damage,damageDirection);
            }
            if (explosions > 5) Destroy(gameObject);
        }


        public void OnTriggerEnter2D(Collider2D other)
        {
            if (explodeCounter != NOT_EXPLODING) return;
            if (other.gameObject.CompareTag("Player")) return;
            speed = 0;
            GetComponent<SpriteRenderer>().enabled = false;
            explodeCounter = TICK_RATE;
        }
    }
}
