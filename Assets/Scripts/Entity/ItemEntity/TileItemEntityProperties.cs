using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TileItemEntityProperties : StackableItemEntity
{

    public override void initalize() {
        this.amount = 1;
        
        gameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.layer = LayerMask.NameToLayer("Entity");
        BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        
        IdDataMap idDataMap = IdDataMap.getInstance();
        spriteRenderer.sprite = idDataMap.GetSprite(id);
        transform.localScale = new Vector3(Global.TileItemEntityScalar, Global.TileItemEntityScalar,1f);
        boxCollider.size = spriteRenderer.sprite.bounds.size;
    }

    public void initalize(int amount) {
        initalize();
        this.amount = amount;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    
}
