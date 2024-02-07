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
        Vector2Int spriteSize = Global.getSpriteSize(spriteRenderer.sprite);
        int xScale = 1;
        if (spriteSize.x > 2) {
            xScale = 1/spriteSize.x;
        }
        int yScale = 1;
        if (spriteSize.y > 2) {
            yScale = 1/spriteSize.y;
        }
        transform.localScale = new Vector3(xScale, yScale,1f);
        boxCollider.size = spriteRenderer.sprite.bounds.size;
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    
}
