using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEntityHelper
{
    public static GameObject spawnItemEntity(Vector2 position, ItemSlot itemSlot, Transform entityContainer) {
        if (itemSlot == null) {
            return null;
        }
        GameObject tileItemEntity = new GameObject();
        tileItemEntity.AddComponent<TileItemEntityProperties>();
        TileItemEntityProperties itemProperties = tileItemEntity.GetComponent<TileItemEntityProperties>();
        itemProperties.itemSlot = itemSlot;
        itemProperties.transform.position = new Vector3(position.x,position.y,0);
        itemProperties.transform.parent = entityContainer;
        itemProperties.initalize();
        return tileItemEntity;
    }

    public static GameObject spawnItemEntityWithVelocity(Vector2 position, ItemSlot itemSlot, Transform entityContainer, Vector2 velocity) {
        GameObject tileItemEntity = spawnItemEntity(position,itemSlot,entityContainer);
        tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
        return tileItemEntity;
    }
}
