using System.Collections;
using System.Collections.Generic;
using Item.Slot;
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
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(node?.ImageSeralizedItemSlot?.id);
            if (ReferenceEquals(itemObject,null)) return;
            ItemDisplayUtils.DisplayItemSprite(image,itemObject,0);
        }

        protected override void openContent()
        {
            UIAssetManager assetManager = nodeNetwork.QuestBookUI.AssetManager;
            QuestBookTaskPageUI pageUI = assetManager.cloneElement<QuestBookTaskPageUI>("TASK_PAGE");
            pageUI.Initialize(node,nodeNetwork);
            pageUI.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
        }
    }
}

