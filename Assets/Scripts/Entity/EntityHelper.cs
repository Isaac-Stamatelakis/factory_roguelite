using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHelper

{

    public static List<EntityData> entityContainerToList(Transform container) {
        if (container!= null) {
            List<EntityData> entityDataList = new List<EntityData>();

            for (int n = 0; n < container.childCount; n++) {
                EntityProperties entityProperties = container.GetChild(n).GetComponent<EntityProperties>();
                if (entityProperties is TileItemEntityProperties) {
                    TileItemEntityProperties tileItemEntityProperties = (TileItemEntityProperties) entityProperties;
                    if (tileItemEntityProperties.itemSlot != null) {
                        ItemSlot itemSlot = tileItemEntityProperties.itemSlot;
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        EntityData entityData = new EntityData();
                        data["amount"] = itemSlot.amount;
                        entityData.id = itemSlot.itemObject.id;
                        entityData.x = tileItemEntityProperties.transform.position.x;
                        entityData.y = tileItemEntityProperties.transform.position.y;
                        entityData.data = data;
                        entityDataList.Add(entityData);
                    }
                }
            }
            return entityDataList;
        }
        return null;
    }
}
