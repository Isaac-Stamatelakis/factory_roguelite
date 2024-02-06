using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemEntity : Entity
{
    [SerializeField] public ItemSlot itemSlot;
    [SerializeField] protected float lifeTime = 0f;
    public float LifeTime {get{return lifeTime;}}

    protected void iterateLifeTime() {
        lifeTime += Time.deltaTime;
        if (lifeTime > Global.ItemEntityLifeSpawn) {
            Destroy(gameObject);
        }
    }

    public virtual void Update()
    {
        iterateLifeTime();
    }
}
