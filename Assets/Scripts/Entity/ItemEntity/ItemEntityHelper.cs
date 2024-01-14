using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEntityHelper
{
    public static GameObject spawnItemEntity(Vector2 position, int id, int amount, Transform entityContainer) {
        if (id < 0) {
            return null;
        }
        GameObject tileItemEntity = new GameObject();
        tileItemEntity.AddComponent<TileItemEntityProperties>();
        TileItemEntityProperties itemProperties = tileItemEntity.GetComponent<TileItemEntityProperties>();
        itemProperties.Id = id;
        itemProperties.transform.position = new Vector3(position.x,position.y,0);
        itemProperties.transform.position = new Vector3(position.x,position.y,0);
        itemProperties.transform.parent = entityContainer;
        itemProperties.initalize(amount);
        return tileItemEntity;
    }

    public static GameObject spawnItemEntityWithVelocity(Vector2 position, int id, int amount, Transform entityContainer, Vector2 velocity) {
        GameObject tileItemEntity = spawnItemEntity(position,id,amount,entityContainer);
        tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
        return tileItemEntity;
    }
}
