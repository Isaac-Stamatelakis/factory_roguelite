using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEntityProperties : EntityProperties
{
    [SerializeField] protected int amount;
    [SerializeField] protected float lifeTime = 0f;
    public int Amount {get{return amount;} set{amount = value;}}
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
