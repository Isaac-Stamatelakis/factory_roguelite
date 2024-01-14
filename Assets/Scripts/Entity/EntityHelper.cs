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
                    if (tileItemEntityProperties.Id > 0) {
                        EntityData entityData = new EntityData();
                        entityData.amount = tileItemEntityProperties.Amount;
                        entityData.id = tileItemEntityProperties.Id;
                        entityData.x = tileItemEntityProperties.transform.position.x;
                        entityData.y = tileItemEntityProperties.transform.position.y;
                        entityDataList.Add(entityData);
                    }
                }
            }
            return entityDataList;
        }
        return null;
    }
}
