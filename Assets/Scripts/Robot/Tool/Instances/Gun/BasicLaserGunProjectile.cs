using System;
using UnityEngine;

namespace Robot.Tool.Instances.Gun
{
    public interface IDamageableEntity
    {
        public void Damage(float damage,Vector2 damageDirection);
    }
    public class BasicLaserGunProjectile : MonoBehaviour
    {
        [SerializeField] private float SizeChangeRate = 0.5f;
        private float damage;
        private float lifeTime;
        private Vector2 direction;
        private float speed;
        private Color defaultColor;
        private SpriteRenderer spriteRenderer;

        
        public void Initialize(float damage, Vector2 direction, float speed)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            this.damage = damage;
            this.direction = direction;
            this.speed = speed;
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultColor = spriteRenderer.color;
            Update();
        }

        public void Update()
        {
            float scaleChange = Time.deltaTime*SizeChangeRate*speed;
            transform.Translate(direction * (scaleChange/2f + speed * Time.deltaTime), Space.World);
            lifeTime += Time.deltaTime;
            var scale = transform.localScale;
            scale.x += scaleChange;
            transform.localScale = scale;
            spriteRenderer.color = Color.Lerp(defaultColor, Color.white, Mathf.PingPong(Time.time, 1));
            if (lifeTime > 2f) Destroy(gameObject);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player")) return;
            IDamageableEntity damageableEntity = other.gameObject.GetComponent<IDamageableEntity>();
            damageableEntity?.Damage(damage,direction);
            Destroy(gameObject);
        }
    }
}
