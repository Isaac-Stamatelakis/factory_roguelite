using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TileItemEntityProperties : StackableItemEntity
{

    public override void initalize() {
        gameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.layer = LayerMask.NameToLayer("Entity");
        BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
    
        spriteRenderer.sprite = itemSlot.itemObject.getSprite();
        
        transform.localScale = new Vector3(0.5f, 0.5f,1f);
        boxCollider.size = spriteRenderer.sprite.bounds.size;
    }


    // Update is called once per frame
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    
}
