using System;
using System.Collections;
using System.Collections.Generic;
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
        private ToolObjectPool particlePool;
        private bool damaged = false;
        private ParticleSystem particles;
        
        public void Initialize(float damage, Vector2 direction, float speed, ToolObjectPool particlePool)
        {
            if (direction == Vector2.zero) direction = Vector2.up;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            this.particlePool = particlePool;
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
            if (lifeTime > 2f && !damaged) Destroy(gameObject);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (damaged) return;
            if (other.gameObject.CompareTag("Player")) return;
            damaged = true;
            IDamageableEntity damageableEntity = other.gameObject.GetComponent<IDamageableEntity>();
            damageableEntity?.Damage(damage,direction);
            var poolObject = particlePool?.TakeFromPool();
            if (!poolObject)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            particles = poolObject.GetComponent<ParticleSystem>();
            particles.transform.position = other.ClosestPoint(transform.position);
            var particleModule = particles.main;
            particleModule.loop = false;
            particles.Play();
            StartCoroutine(DelayDestroy());
        }

        private IEnumerator DelayDestroy()
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.4f);
            particlePool.ReturnToPool(particles.gameObject);
            Destroy(gameObject);
        }
    }
}
