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
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(node.ImageSeralizedItemSlot);
            ItemSlotUI itemSlotUI = ItemSlotUIFactory.newItemSlotUI(itemSlot,transform,null,false);
        }

        protected override void openContent()
        {
            UIAssetManager assetManager = nodeNetwork.QuestBookUI.AssetManager;
            QuestBookTaskPageUI pageUI = assetManager.cloneElement<QuestBookTaskPageUI>("TASK_PAGE");
            pageUI.init(node,nodeNetwork);
            pageUI.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
        }
    }
}

