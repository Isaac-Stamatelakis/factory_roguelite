using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEntityProperties : EntityProperties
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

    public override void Update()
    {
        base.Update();
        iterateLifeTime();
    }
}
