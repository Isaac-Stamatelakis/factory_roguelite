using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using UI.NodeNetwork;

namespace UI.QuestBook {
    public class QuestBookNodeObject : NodeUI<QuestBookNode,QuestBookPageUI>
    {
        
        protected override void setImage()
        {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(node.ItemImageID);
            if (itemObject != null) {
                image.sprite = itemObject.getSprite();
            }
        }

        protected override void openContent()
        {
            GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.TaskContentPrefabPath));
            QuestBookTaskPageUI pageUI = instantiated.GetComponent<QuestBookTaskPageUI>();
            instantiated.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
            pageUI.init(node,nodeNetwork);
        }
    }
}

